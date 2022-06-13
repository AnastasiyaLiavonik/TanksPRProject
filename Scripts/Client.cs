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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class Client : MonoBehaviour
{
    private static TcpClient clientSocket;
    public static Hashtable clientsList = new Hashtable();
    private static NetworkStream serverStream = default(NetworkStream);
    [SerializeField]
    public Server server;
    public HumanController humanController;
    public static bool isConnected = false;
    public static bool loadFlag = false;
    public int id;
    private Dictionary<string, string> enemyTanksDict = new Dictionary<string, string>();
    private Dictionary<string, string> playerTanksDict = new Dictionary<string, string>();
    private static Dictionary<int, string> tanksInMatch = new Dictionary<int, string>();
    private static Dictionary<int, HumanController> tanksControllersInMatch = new Dictionary<int, HumanController>();
    private static Dictionary<int, State> tanksStatesInMatch = new Dictionary<int, State>();
    Mutex mutex = new Mutex();
    public long startTime = 0;

    public Client()
    {
        
    }

    void Awake()
    {
        enemyTanksDict.Add("light", "HumanControlledLight");
        enemyTanksDict.Add("dark", "HumanControlledDark");
        enemyTanksDict.Add("red", "HumanControlledRed");
        enemyTanksDict.Add("green", "HumanControlledGreen");
        playerTanksDict.Add("light", "PlayerControlledLight");
        playerTanksDict.Add("dark", "PlayerControlledDark");
        playerTanksDict.Add("red", "PlayerControlledRed");
        playerTanksDict.Add("green", "PlayerControlledGreen");
        int servers = FindObjectsOfType<Client>().Length;
        if (servers != 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
        StartCoroutine(Example());
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void Connecta(string IP)
    {
        clientSocket = new TcpClient();
        clientSocket.Connect(IP, 777);
        serverStream = clientSocket.GetStream();
        SendMessageToServer("Ave Maria$");
        Thread ctThread = new Thread(GetMessage);
        ctThread.Start();

    }

    IEnumerator Example()
    {
        yield return new WaitUntil(() => loadFlag);
        SceneManager.LoadSceneAsync(3);

        while (SceneManager.GetActiveScene().buildIndex != 3)
        {
            yield return null;
        }

        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            Debug.Log($"In tank deleting: " +
                $"{id}");
            var enemyTanks = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemyTanks)
            {
                if(enemy.name == enemyTanksDict[tanksInMatch[id]] || !tanksInMatch.ContainsValue(enemyTanksDict.FirstOrDefault(x => x.Value ==  enemy.name).Key))
                {
                    Destroy(enemy);
                }
                else
                {
                    Debug.Log(enemy.name);
                    tanksStatesInMatch.Add(tanksInMatch.FirstOrDefault(x => x.Value == enemyTanksDict.FirstOrDefault(x => x.Value == enemy.name).Key).Key, new State());
                    tanksControllersInMatch.Add(tanksInMatch.FirstOrDefault(x => x.Value == enemyTanksDict.FirstOrDefault(x => x.Value == enemy.name).Key).Key, enemy.GetComponent<HumanController>());
                }
            }

            var playerTanks = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in playerTanks)
            {
                if (player.name != playerTanksDict[tanksInMatch[id]])
                {
                    Destroy(player);
                }
                else
                {
                    Debug.Log(player.name);
                    tanksStatesInMatch.Add(tanksInMatch.FirstOrDefault(x => x.Value == playerTanksDict.FirstOrDefault(x => x.Value == player.name).Key).Key, new State());
                    tanksControllersInMatch.Add(tanksInMatch.FirstOrDefault(x => x.Value == playerTanksDict.FirstOrDefault(x => x.Value == player.name).Key).Key, player.transform.GetChild(0).gameObject.GetComponent<HumanController>());
                }
            }
        }
    }



    public void SendMessageToServer(string msg)
    {
        if(DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= startTime || msg.Contains("Ave"))
        {
            byte[] outStream = Encoding.ASCII.GetBytes(msg);
            serverStream.Write(outStream, 0, outStream.Length);
        }
        
    }

    private void GetMessage()
    {
        while (true)
        {
            serverStream = clientSocket.GetStream();
            int buffSize = 0;
            byte[] inStream = new byte[10000025];
            buffSize = clientSocket.ReceiveBufferSize;
            serverStream.Read(inStream, 0, buffSize);
            string returnData = System.Text.Encoding.ASCII.GetString(inStream);
            Debug.Log($"'{returnData}'");
            if (!isConnected && !returnData.Contains("Deus Vult"))
            {
                Debug.Log("Disconnected because Server is not Crusader!");
                clientSocket.Close();
                break;
            }
            else if(!isConnected && returnData.Contains("Deus Vult"))
            {
                // register tank
                Debug.Log("tank added");
                mutex.WaitOne();
                id = Int32.Parse(returnData.Split(":")[1]);
                mutex.ReleaseMutex();
                Debug.Log($"Your id is {id}");
                isConnected = true;
            }
            else if(isConnected)
            {
                Debug.Log(id);
                char type = returnData[0];
                switch (type)
                {
                    case 't':
                        {
                            startTime = long.Parse(returnData.Split(":")[1]);
                            break;
                        }
                    case 'i':
                    {
                        var tanksString = returnData.Substring(2);
                        var tanks = tanksString.Split(";");
                        foreach (var tank in tanks)
                        {
                            if (!tank.Contains("-")) continue;
                            var tankInfo = tank.Split("-");                     
                            tanksInMatch.Add(Int32.Parse(tankInfo[0]), tankInfo[1]);
                        }
                        loadFlag = true;
                        break;
                    }
                    case 'c':
                    {
                        State state = JsonConvert.DeserializeObject<State>(returnData.Split("&")[0].Substring(2));
                        tanksStatesInMatch[state.player_id] = state;
                        break;
                    }
                }
            }
        }
    }

    public void Update()
    {
        foreach(var tank in tanksControllersInMatch)
        {
            tank.Value.GetBodyMovement(tanksStatesInMatch[tank.Key].movementVector);
            tank.Value.GetShootingInput(tanksStatesInMatch[tank.Key].shoot);
            tank.Value.GetTurretMovement(tanksStatesInMatch[tank.Key].mousePosition);
        }
    }
}