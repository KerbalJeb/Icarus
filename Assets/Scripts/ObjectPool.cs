using System.Collections.Generic;
using UnityEngine;

// TODO instantiate objects asynchronously when low on free objects

/// <summary>
///     Used to pool game objects to prevent excessive memory allocation/garbage collection
/// </summary>
public class ObjectPool : MonoBehaviour
{
    /// <value>
    ///     The game object to pool
    /// </value>
    [SerializeField] private GameObject templateGameObject;

    /// <value>
    ///     The number of objects to initially create
    /// </value>
    [SerializeField] private int numObjects;

    private readonly List<GameObject> pool = new List<GameObject>();

    private void Start()
    {
        for (var i = 0; i < numObjects; i++) AddObject();
    }

    /// <summary>
    ///     Adds a new object to the pool
    /// </summary>
    private void AddObject()
    {
        GameObject go = Instantiate(templateGameObject);
        go.SetActive(false);
        pool.Add(go);
    }

    /// <summary>
    ///     Gets an object from the pool, will create a new object if none are available. Deactivate the object when done to
    ///     return it to the pool.
    /// </summary>
    /// <returns>The object (will be disabled)</returns>
    public GameObject GetObject()
    {
        for (var i = 0; i < numObjects; i++)
        {
            if (!pool[i].activeInHierarchy)
                return pool[i];
        }

        numObjects++;
        AddObject();
        return pool[numObjects - 1];
    }
}
