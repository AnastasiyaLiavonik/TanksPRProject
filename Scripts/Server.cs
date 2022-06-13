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

public class Server : MonoBehaviour
{
    public static Hashtable clientsList = new Hashtable();
    public static ConcurrentBag<string> chat = new ConcurrentBag<string>();
    public static MultiplayerManager multiplayerManager;
    public static TextMeshProUGUI pText;
    public static Button butt;
    public static bool flag = false;
    public static string[] tanks = { "light", "dark", "red", "green"};

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
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        Client client = new Client();
        Task.Delay(300).ContinueWith(t => client.Connecta(GetIPAddress()));
    }

    public void StartBtn()
    {
        SendStartingInfo();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    void SendStartingInfo()
    {
        string toSend = "i:";
        for (int i = 0; i< multiplayerManager.playersNumber; i++)
        {
            toSend += $"{i}-{tanks[i]};";
        }
        broadcast(toSend+'\0', "");
        broadcast("t:"+(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3).ToString()+'\0', "");
    }

    private void StartServer()
    {
        Debug.Log("Server Started ....");
        IPAddress localAddr = IPAddress.Parse(GetIPAddress());
        TcpListener serverSocket = new TcpListener(localAddr, 777);
        int counter = 0;

        serverSocket.Start();

        while (counter != multiplayerManager.playersNumber)
        {
            int c = counter;
            counter += 1;
            handleClinet client = new handleClinet();
            clientsList.Add(tanks[counter - 1], client.startClient(serverSocket, counter));
            sendMessageToTank(tanks[c], "Deus Vult:"+c.ToString());
        }
        flag = true;
        //clientSocket.Close();
        //serverSocket.Stop();
    }

    IEnumerator Example()
    {
        yield return new WaitUntil(() => flag);
        pText.text = "All " + multiplayerManager.playersNumber.ToString() + " players are connected. We are ready to begin!";
        butt.interactable = true;
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
    }  //end broadcast function

    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;

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
                    broadcast(dataFromClient, "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    } 
}