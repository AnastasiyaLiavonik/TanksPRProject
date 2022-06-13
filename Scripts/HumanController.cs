using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HumanController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private MultiplayerManager multiplayerManager;

    public UnityEvent OnShoot = new UnityEvent();
    public UnityEvent<Vector2> OnMoveBody = new UnityEvent<Vector2>();
    public UnityEvent<Vector2> OnMoveTurret = new UnityEvent<Vector2>();
    public Vector3 turretPointer; 

    // Start is called before the first frame update
    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    // Update is called once per frame

    public void GetShootingInput(bool shoot)
    {
        if(shoot) OnShoot?.Invoke();
    }

    public void GetTurretMovement(Vector2 mousePosition)
    {
        OnMoveTurret?.Invoke(mousePosition);

    }


    public void GetBodyMovement(Vector2 movementVector)
    {
        OnMoveBody?.Invoke(movementVector.normalized);
    }
}
