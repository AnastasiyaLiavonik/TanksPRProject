using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Cinemachine;

public static class Extensions
{
    public static T[] SubArray<T>(this T[] array, int offset, int length)
    {
        return new ArraySegment<T>(array, offset, length)
                    .ToArray();
    }
}

public class Client : MonoBehaviour
{
    private static TcpClient clientSocket;
    public static Hashtable clientsList = new Hashtable();
    private static NetworkStream serverStream = default(NetworkStream);
    [SerializeField]
    public Server server;
    public PlayerInput playerInput;
    public static bool isConnected = false;
    public static bool loadFlag = false;
    public bool gameContinues = true;
    public int id;
    public GameObject player;
    private Dictionary<string, string> enemyTanksDict = new Dictionary<string, string>();
    private Dictionary<string, string> playerTanksDict = new Dictionary<string, string>();
    private static Dictionary<int, string> tanksInMatch = new Dictionary<int, string>();
    private static Dictionary<int, HumanController> tanksControllersInMatch = new Dictionary<int, HumanController>();
    private static Dictionary<int, State> tanksStatesInMatch = new Dictionary<int, State>();
    private static Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();
    public Mutex mutex = new Mutex();
    private State currentState = new State();
    public ulong mesID = 0;
    public long startTime = 0;
    public bool deletedUnnecessary = false;
    public Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();

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
        positions.Add("PlayerControlledRed", new Vector3(-0.22f, -3.57f, 0.00f));
        positions.Add("PlayerControlledLight", new Vector3(-5.43f, -3.28f, 0.00f));
        positions.Add("PlayerControlledDark", new Vector3(-3.93f, -1.10f, 0.00f));
        positions.Add("PlayerControlledGreen", new Vector3(-1.15f, -1.11f, 0.00f));
        positions.Add("HumanControlledRed", new Vector3(-0.22f, -3.57f, 0.00f));
        positions.Add("HumanControlledLight", new Vector3(-5.43f, -3.28f, 0.00f));
        positions.Add("HumanControlledDark", new Vector3(-3.93f, -1.10f, 0.00f));
        positions.Add("HumanControlledGreen", new Vector3(-1.15f, -1.11f, 0.00f));
        int clients = FindObjectsOfType<Client>().Length;
        if (clients != 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
        StartCoroutine(Example());
        StartCoroutine(Playback());
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

    IEnumerator Playback()
    {
        while (SceneManager.GetActiveScene().buildIndex != 3)
        {
            yield return null;
        }
        yield return new WaitUntil(() => deletedUnnecessary);
        while (gameContinues)
        {
            Debug.Log(mesID);
            currentState.mousePosition = playerInput.GetMousePositon();
            currentState.movementVector = playerInput.GetBodyMovement();
            mesID++;
            currentState.player_id = id;
            currentState.mes_id = mesID;
            currentState.shoot = playerInput.GetShootingInput();
            if(mesID%50 == 0)
            {
                State2 state = new State2();
                state.id = id;
                state.position = player.transform.position;
                state.rotation = player.transform.rotation;
                SendMessageToServer("k:"+JsonConvert.SerializeObject(state)+"&\0");
            }
            SendMessageToServer("c:" + JsonConvert.SerializeObject(currentState) + "&\0");
            yield return new WaitForSeconds(1f / 100f);
        } 
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
            var enemyTanks = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemyTanks)
            {
                if(enemy.name == enemyTanksDict[tanksInMatch[id]] || !tanksInMatch.ContainsValue(enemyTanksDict.FirstOrDefault(x => x.Value ==  enemy.name).Key))
                {
                    Destroy(enemy);
                }
                else
                {
                    tanksStatesInMatch.Add(tanksInMatch.FirstOrDefault(x => x.Value == enemyTanksDict.FirstOrDefault(x => x.Value == enemy.name).Key).Key, new State());
                    tanksControllersInMatch.Add(tanksInMatch.FirstOrDefault(x => x.Value == enemyTanksDict.FirstOrDefault(x => x.Value == enemy.name).Key).Key, enemy.GetComponent<HumanController>());
                    enemy.transform.position = positions[enemy.name];
                    enemies.Add(tanksInMatch.FirstOrDefault(x => x.Value == enemyTanksDict.FirstOrDefault(x => x.Value == enemy.name).Key).Key, enemy);
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
                    tanksStatesInMatch.Add(tanksInMatch.FirstOrDefault(x => x.Value == playerTanksDict.FirstOrDefault(x => x.Value == player.name).Key).Key, new State());
                    tanksControllersInMatch.Add(tanksInMatch.FirstOrDefault(x => x.Value == playerTanksDict.FirstOrDefault(x => x.Value == player.name).Key).Key, player.transform.GetChild(0).gameObject.GetComponent<HumanController>());
                    playerInput = player.GetComponent<PlayerInput>();
                    this.player = player;
                    var cin = GameObject.Find("PlayerCinemachine").GetComponent<CinemachineVirtualCamera>();
                    cin.Follow = player.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.transform;
                    player.transform.position = positions[player.name];
                }
            }
            deletedUnnecessary = true;
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
                lock ((object)id)
                {
                    id = Int32.Parse(returnData.Split(":")[1]);
                }
                Debug.Log($"Your id is {id}");
                isConnected = true;
            }
            else if(isConnected)
            {
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
                        string[] a = returnData.Substring(2).Split("&")[0].Split("|");
                        a = a.SubArray(0,a.Length-1);
                            Debug.Log($"{tanksStatesInMatch.Count} {a.Length}");
                        if (a.Length != tanksStatesInMatch.Count)
                        {
                            Debug.Log("wrong message size");
                            }
                            else
                            {
lock (tanksStatesInMatch)
                        {
                            foreach(var tankState in a)
                            {
                                State state = JsonConvert.DeserializeObject<State>(tankState);
                                tanksStatesInMatch[state.player_id] = state;
                            }
                        }
                            }
                        
                        break;
                    }
                    case 'k':
                        {
                            State2 state = JsonConvert.DeserializeObject<State2>(returnData.Split("&")[0].Substring(2));
                            enemies[state.id].transform.position = state.position;
                            enemies[state.id].transform.rotation = state.rotation;
                            continue;
                        }
                }
            }
        }
    }

    public void Update()
    {
        foreach (var tank in tanksControllersInMatch)
        {
            lock (tanksStatesInMatch)
            {
                tank.Value.GetBodyMovement(tanksStatesInMatch[tank.Key].movementVector);
                tank.Value.GetShootingInput(tanksStatesInMatch[tank.Key].shoot);
                tank.Value.GetTurretMovement(tanksStatesInMatch[tank.Key].mousePosition);
            }
            
        }
    }
}