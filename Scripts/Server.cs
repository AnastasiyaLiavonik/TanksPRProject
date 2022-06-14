using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;


public class Server : MonoBehaviour
{
    public static Hashtable clientsList = new Hashtable();
    public static ConcurrentBag<string> chat = new ConcurrentBag<string>();
    public static MultiplayerManager multiplayerManager;
    public static TextMeshProUGUI pText;
    public static Button butt;
    private static Dictionary<int, State> tanksStatesInMatch = new Dictionary<int, State>();
    public static bool flag = false;
    public static string[] tanks = { "light", "dark", "red", "green"};
    public TcpListener serverSocket;
    public Mutex mutex = new Mutex();
    public bool gameContinues = true;
    public long startTime = 0;

    void Awake()
    {
        int servers = FindObjectsOfType<Server>().Length;
        if (servers != 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnEnable()
    {
        multiplayerManager = FindObjectsOfType<MultiplayerManager>()[0];
        TextMeshProUGUI mText = GameObject.Find("TextIP").GetComponent<TextMeshProUGUI>();
        butt = GameObject.Find("StartBtn").GetComponent<Button>();
        butt.interactable = false;
        mText.text = GetIPAddress();
        StartCoroutine(Example());
    }

    void Start()
    {
        pText = GameObject.Find("TextPlayers").GetComponent<TextMeshProUGUI>();
        Thread start = new Thread(StartServer);
        Task.Delay(300).ContinueWith(t => start.Start());
        Client client = FindObjectsOfType<Client>()[0];
        Task.Delay(300).ContinueWith(t => client.Connecta(GetIPAddress()));
    }

    public void StartBtn()
    {
        SendStartingInfo();
    }

    void SendStartingInfo()
    {
        string toSend = "i:";
        for (int i = 0; i< multiplayerManager.playersNumber; i++)
        {
            toSend += $"{i}-{tanks[i]};";
        }
        broadcast(toSend+'\0', "");
        startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 2;
        broadcast("t:"+(startTime).ToString()+'\0', "");
        StartCoroutine(Playback());
    }

    private void StartServer()
    {
        Debug.Log("Server Started ....");
        IPAddress localAddr = IPAddress.Parse(GetIPAddress());
        serverSocket = new TcpListener(localAddr, 777);
        int counter = 0;

        serverSocket.Start();

        while (counter != multiplayerManager.playersNumber)
        {
            int c = counter;
            counter += 1;
            handleClinet client = new handleClinet();
            clientsList.Add(tanks[counter - 1], client.startClient(serverSocket, counter));
            sendMessageToTank(tanks[c], "Deus Vult:"+c.ToString());
            tanksStatesInMatch.Add(counter-1, new State());
        }
        flag = true;
    }

    IEnumerator Playback()
    {
        while (SceneManager.GetActiveScene().buildIndex != 3)
        {
            yield return null;
        }
        yield return new WaitUntil(() => DateTimeOffset.UtcNow.ToUnixTimeSeconds() == startTime);
        while (gameContinues)
        {
            lock (tanksStatesInMatch)
            {
                Debug.Log("beg");
                string text = "c:";
                foreach (var tankPos in tanksStatesInMatch.Values)
                {
                    text += JsonConvert.SerializeObject(tankPos) + "|";
                    Debug.Log(tankPos.player_id);
                }
                broadcast(text + "&\0", "");
                Debug.Log("ret");
            }
            yield return new WaitForSeconds(1f / 200f);
        }
    }

    IEnumerator Example()
    {
        yield return new WaitUntil(() => flag);
        pText.text = "All " + multiplayerManager.playersNumber.ToString() + " players are connected. We are ready to begin!";
        butt.interactable = true;
    }

    public void Disconnect()
    {
        foreach ( var client in clientsList)
        {
            TcpClient tcc = (TcpClient)client;
            tcc.Close();
        }
        serverSocket.Stop();
    }

    static string GetIPAddress()
    {
        StringBuilder sb = new StringBuilder();
        String strHostName = string.Empty;
        strHostName = Dns.GetHostName();
        sb.Append("The Local Machine Host Name: " + strHostName);
        sb.AppendLine();
        IPHostEntry ipHostEntry = Dns.GetHostEntry(strHostName);
        IPAddress[] address = ipHostEntry.AddressList;
        sb.Append("The Local IP Address: " + address[1].ToString());
        sb.AppendLine();
        if (address[0].ToString().Contains("192"))
        {
            return address[0].ToString();
        }
        return address[1].ToString();
    }

    public static void sendMessageToTank(string tankName, string msg)
    {
        TcpClient client = (TcpClient)clientsList[tankName];
        NetworkStream broadcastStream = client.GetStream();
        Byte[] broadcastBytes = broadcastBytes = Encoding.ASCII.GetBytes(msg);
        broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
        broadcastStream.Flush();
    }

    public static void broadcast(string msg, string uName)
    {
        foreach (DictionaryEntry Item in clientsList)
        {
            TcpClient broadcastSocket;
            broadcastSocket = (TcpClient)Item.Value;
            NetworkStream broadcastStream = broadcastSocket.GetStream();
            Byte[] broadcastBytes = null;
            broadcastBytes = Encoding.ASCII.GetBytes(msg);
            broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
            broadcastStream.Flush();
        }
    }

    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        public Mutex mutex = new Mutex();

        public TcpClient startClient(TcpListener serverSocket, int counter)
        {
            this.clientSocket = serverSocket.AcceptTcpClient();
            this.clNo = tanks[counter - 1];

            byte[] bytesFrom = new byte[10000025];
            string dataFromClient = "";

            NetworkStream networkStream = clientSocket.GetStream();
            networkStream.Read(bytesFrom, 0, bytesFrom.Length);
            dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
            dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
            Debug.Log(dataFromClient);
            if (dataFromClient != "Ave Maria")
            {
                this.clientSocket.Close();
            }

            Thread ctThread = new Thread(doChat);
            ctThread.Start();


            return this.clientSocket;
        }

        private void doChat()
        {
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            NetworkStream networkStream = clientSocket.GetStream();
            while ((true))
            {
                try
                {
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    State state = JsonConvert.DeserializeObject<State>(dataFromClient.Split("&")[0].Substring(2));
                    lock (tanksStatesInMatch)
                    {
                        tanksStatesInMatch[state.player_id] = state;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }
    } 
}