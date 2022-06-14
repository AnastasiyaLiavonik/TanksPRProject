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

    [SerializeField]
    public int playersNumber = 0;
    private bool winFlag = false;
    private bool defeatFlag = false;
    public static Hashtable clientsList = new Hashtable();


    private void Awake() 
    {
        int multiplayerManagers = FindObjectsOfType<MultiplayerManager>().Length;
        if (multiplayerManagers != 1)
        {
            Destroy(this.gameObject);
        }
        // if more then one music player is in the scene
        //destroy ourselves
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Start()
    {
        Debug.Log("Multiplayer launched");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 3)
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

    // Update is called once per frame
    void Update()
    {
        if (Application.targetFrameRate != 30)
            Application.targetFrameRate = 30;
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && winScreen != null && GameObject.FindGameObjectsWithTag("Player").Length != 0)
            {
                //foreach(GameObject go in GameObject.FindGameObjectsWithTag("Player"))
                //{
                //    go.SetActive(false);
                //}
                //winScreen.SetActive(true);
                //winFlag = true;
                //Invoke("back", 2.0f);
                
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
        Server s = GameObject.FindObjectOfType<Server>();
        s.Disconnect();
        SceneManager.LoadScene(0);
    }

}


