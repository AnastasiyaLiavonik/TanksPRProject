using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealCrate : MonoBehaviour
{
    public HealCrateData healData;
    private Rigidbody2D rb2d;

    public UnityEvent OnDead = new UnityEvent();

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

    }

    public void Initialize(HealCrateData healData)
    {
        this.healData = healData;
    }

    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damagable>();
        if (damagable != null)
        {
            damagable.Heal(healData.heal);
            OnDead?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
