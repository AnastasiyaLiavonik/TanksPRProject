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
using TMPro;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    private static TcpClient clientSocket = new TcpClient();
    private static NetworkStream serverStream = default(NetworkStream);
    [SerializeField]
    public Server server;
    public static bool res = false;

    void Start()
    {
        Debug.Log("Host client started");
    }

    void Update()
    {

    }

    public void ConnectBtn()
    {
        TextMeshProUGUI connectionStatus = GameObject.Find("ConnectionStatus").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI IPAddressInputText = GameObject.Find("IPAddressInputText").GetComponent<TextMeshProUGUI>();
        if (!Regex.IsMatch(IPAddressInputText.text, "(?:[0-9]{1,3}\\.){3}[0-9]{1,3}"))
        {
            connectionStatus.text = "Entered IP address is incorrect.";
            return;
        }
        connectionStatus.text = "Connection...";

        Connect(IPAddressInputText.text);

        //SceneManager.LoadScene(3);
    }

    public static void Connect(string IP)
    {
        clientSocket.Connect(IP, 888);
        serverStream = clientSocket.GetStream();
        byte[] outStream = Encoding.ASCII.GetBytes("Ave Maria$");
        serverStream.Write(outStream, 0, outStream.Length);

        Thread ctThread = new Thread(getMessage);
        ctThread.Start();
    }

    private static void getMessage()
    {
        while (true)
        {
            serverStream = clientSocket.GetStream();
            int buffSize = 0;
            byte[] inStream = new byte[10000025];
            buffSize = clientSocket.ReceiveBufferSize;
            serverStream.Read(inStream, 0, buffSize);
            string returndata = System.Text.Encoding.ASCII.GetString(inStream);
            if(returndata != "Deus Vult" && res)
            {
                clientSocket.Close();
                break;
            }
            else if(returndata == "Deus Vult" && !res)
            {
                break;
            }
            else
            {
                Debug.Log("tank added");
            }
        }
    }
}