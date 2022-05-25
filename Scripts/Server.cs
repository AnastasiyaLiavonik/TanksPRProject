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
    }

    void Start()
    {
        pText = GameObject.Find("TextPlayers").GetComponent<TextMeshProUGUI>();
        Thread start = new Thread(StartServer);
        Task.Delay(300).ContinueWith(t => start.Start());
        Client hostingClient = FindObjectsOfType<Client>()[0];
        Task.Delay(300).ContinueWith(t => Client.Connect(GetIPAddress()));
    }

    public void StartBtn()
    {
        SceneManager.LoadScene(3);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    private void StartServer()
    {
        Debug.Log("Server Started ....");
        IPAddress localAddr = IPAddress.Parse(GetIPAddress());
        TcpListener serverSocket = new TcpListener(localAddr, 888);
        int counter = 0;

        serverSocket.Start();

        while (counter != multiplayerManager.playersNumber)
        {
            counter += 1;
            handleClinet client = new handleClinet();
            clientsList.Add(tanks[counter - 1], client.startClient(serverSocket, counter));
            broadcast("Deus Vult" ,"", false);
        }
        pText.text = "All " + counter.ToString() + " players are connected. We are ready to begin!";
        butt.interactable = true;
        //clientSocket.Close();
        //serverSocket.Stop();
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
        foreach (var item in address)
        {
            if (Regex.IsMatch(item.ToString(), "(?:[0-9]{1,3}\\.){3}[0-9]{1,3}"))
            {
                return item.ToString();
            }
        }

        return address[1].ToString();
    }

    public static void broadcast(string msg, string uName, bool flag)
    {
        foreach (DictionaryEntry Item in clientsList)
        {
            TcpClient broadcastSocket;
            broadcastSocket = (TcpClient)Item.Value;
            NetworkStream broadcastStream = broadcastSocket.GetStream();
            Byte[] broadcastBytes = null;

            if (flag == true)
            {
                broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + msg);
            }
            else
            {
                broadcastBytes = Encoding.ASCII.GetBytes(msg);
            }

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
            int requestCount = 0;
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;
            requestCount = requestCount + 1;
            NetworkStream networkStream = clientSocket.GetStream();
            while ((true))
            {
                try
                {
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    if (dataFromClient != "Ave Maria")
                    {
                        chat.Add("From client - " + clNo + " : " + dataFromClient);
                    }
                    rCount = Convert.ToString(requestCount);

                    Server.broadcast(dataFromClient, clNo, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    } 
}