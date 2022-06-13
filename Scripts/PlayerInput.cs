using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private MultiplayerManager multiplayerManager;

    public UnityEvent OnShoot = new UnityEvent();
    public UnityEvent<Vector2> OnMoveBody = new UnityEvent<Vector2>();
    public UnityEvent<Vector2> OnMoveTurret = new UnityEvent<Vector2>();
    private Client client;
    private State currentState;
    private bool close = true;
    private ulong mesID = 0;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        client  = GameObject.FindObjectOfType<Client>();
    }

    private void Start()
    {
        if (Application.targetFrameRate != 30)
            Application.targetFrameRate = 30;
        currentState = new State();
        currentState.shoot = false;
        currentState.hp = 50;
        currentState.player_id = client.id;
        Debug.Log(client.id);
        currentState.mes_id = mesID;
        currentState.mousePosition = GetMousePositon();
        currentState.movementVector = GetBodyMovement();
        StartCoroutine(Playback());
    }

    IEnumerator Playback()
    {
        do
        {
            currentState.mousePosition = GetMousePositon();
            currentState.movementVector = GetBodyMovement();
            mesID++;
            currentState.player_id = client.id;
            currentState.mes_id = mesID;
            currentState.shoot = GetShootingInput();
            client.SendMessageToServer("c:"+JsonConvert.SerializeObject(currentState)+"&\0");
            Debug.Log("c:" + JsonConvert.SerializeObject(currentState) + "&\0");
            yield return new WaitForSeconds(1f / 30f);  
        } while (close);
    }

    private bool GetShootingInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }
        return false;
    }

    private void GetTurretMovement()
    {
        OnMoveTurret?.Invoke(GetMousePositon());
    }

    private Vector2 GetMousePositon()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.nearClipPlane;
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        return mouseWorldPosition;
    }

    private Vector2 GetBodyMovement()
    {
        Vector2 movementVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        return movementVector;
    }
}
