using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerRoleChoice : MonoBehaviour
{
    [SerializeField]
    public MultiplayerManager multiplayerManager;

    void Start()
    {
        multiplayerManager = FindObjectsOfType<MultiplayerManager>()[0];
    }


    public void StartNewSession()
    {
        Debug.Log("New session started");
        // start server
        SceneManager.LoadScene(6);
    }

    public void ConnectToSession()
    {
        // start client;
        Debug.Log("Connection to session...");
        SceneManager.LoadScene(7);
    }
}