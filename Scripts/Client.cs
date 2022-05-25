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
using System.Threading.Tasks;

public class Client : ScriptableObject
{
    private static TcpClient clientSocket;
    private static NetworkStream serverStream = default(NetworkStream);
    [SerializeField]
    public Server server;
    public static bool res = false;
    public static bool flag = false;

    public Client()
    {
        
    }

    public void Connecta(string IP)
    {
        clientSocket = new TcpClient();
        clientSocket.Connect(IP, 777);
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