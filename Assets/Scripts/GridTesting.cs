using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTesting : MonoBehaviour
{
    [SerializeField] private float gridSize = 1f;

    void Start()
    {
        var tileGrid = new TileGrid<int>(15, 15, gridSize, transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
