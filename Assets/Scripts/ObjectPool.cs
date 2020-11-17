using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject       gameObject;
    [SerializeField] private int              numObjects;
    private                  List<GameObject> pool = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < numObjects; i++)
        {
            AddObject();
        }
    }

    private void AddObject()
    {
        var go = Instantiate(gameObject);
        go.SetActive(false);
        pool.Add(go);
    }

    public GameObject GetObject()
    {
        for (int i = 0; i < numObjects; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }
        numObjects++;
        AddObject();
        return pool[numObjects - 1];
    }
}

