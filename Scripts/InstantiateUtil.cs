using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateUtil : MonoBehaviour
{
    public GameObject objectToinstantiate;

    public void InstantiateObject()
    {
        Debug.Log(objectToinstantiate.name);
        Instantiate(objectToinstantiate);
    }
}
