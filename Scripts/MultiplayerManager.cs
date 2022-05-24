using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{
    private GameObject winScreen;
    private GameObject defeatScreen;
    private int playersNumber;
    private bool winFlag = false;
    private bool defeatFlag = false;
    public static Hashtable clientsList = new Hashtable();


    private void Awake() 
    {
    }

    public void Start()
    {
        Debug.Log("Multiplayer launched");
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            if (defeatScreen == null && !defeatFlag)
            {
                defeatFlag = true;
                defeatScreen = GameObject.FindGameObjectsWithTag("Defeat")[0];
                defeatScreen.SetActive(false);
            }
            if (winScreen == null && !winFlag)
            {
                winFlag = true;
                winScreen = GameObject.FindGameObjectsWithTag("Win")[0];
                winScreen.SetActive(false);
            }
        }
    }

    public void StartNewSession()
    {
        Debug.Log("New session started");
        // start server
        SceneManager.LoadScene(6);
    }

    public void TwoBtn()
    {
        playersNumber = 2;
        SceneManager.LoadScene(5);
    }

    public void ThreeBtn()
    {
        playersNumber = 3;
        SceneManager.LoadScene(5);
    }

    public void FourBtn()
    {
        playersNumber = 4;
        SceneManager.LoadScene(5);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex == 5)
        {
            TextMeshProUGUI mText = GameObject.Find("TextIP").GetComponent<TextMeshProUGUI>();
            mText.text = GetIPAddress();
        }
    }

    public void StartBtn()
    {
        SceneManager.LoadScene(3);
    }

    public void ConnectBtn()
    {
        SceneManager.LoadScene(3);
    }

    public void ConnectToSession()
    {
        // start client;
        Debug.Log("Connection to session...");
        SceneManager.LoadScene(7);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && winScreen != null && GameObject.FindGameObjectsWithTag("Player").Length != 0)
            {
                foreach(GameObject go in GameObject.FindGameObjectsWithTag("Player"))
                {
                    go.SetActive(false);
                }
                winScreen.SetActive(true);
                winFlag = true;
                Invoke("back", 2.0f);
                
            }
            else if(GameObject.FindGameObjectsWithTag("Player").Length == 0 && defeatScreen != null && GameObject.FindGameObjectsWithTag("Enemy").Length != 0)
            {
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
                {
                    go.SetActive(false);
                }
                defeatScreen.SetActive(true);
                defeatFlag = true;
                Invoke("back", 2.0f);
            }
        }
        
    }

    private void back()
    {
        SceneManager.LoadScene(0);
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
}


