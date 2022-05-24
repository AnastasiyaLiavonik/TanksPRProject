using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MultiplayerNumberChoice : MonoBehaviour
{
    [SerializeField]
    public MultiplayerManager multiplayerManager;

    void Start()
    {
        multiplayerManager = FindObjectsOfType<MultiplayerManager>()[0];
    }


    public void ThreeBtn()
    {
        multiplayerManager.playersNumber = 3;
        SceneManager.LoadScene(5);
    }

    public void TwoBtn()
    {
        multiplayerManager.playersNumber = 2;
        SceneManager.LoadScene(5);
    }

    public void FourBtn()
    {
        multiplayerManager.playersNumber = 4;
        SceneManager.LoadScene(5);
    }
}