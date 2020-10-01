using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridTesting : MonoBehaviour
{
    [SerializeField] private float gridSize = 1f;
    private TileGrid tileGrid;
    private Camera cam;

    void Start()
    {
        tileGrid = new TileGrid(15, 15, gridSize, transform, GetComponent<MeshRenderer>().material.mainTexture);
        cam = Camera.main;
        GetComponent<MeshFilter>().mesh = tileGrid.GenerateMesh();
    }

    private void Update()
    {
        if (!Mouse.current.leftButton.isPressed) return;
        
        var mousePos = Mouse.current.position;
        var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
        if (tileGrid.InBounds(worldPos))
        {
            tileGrid.GetValue(worldPos).Empty = false;
        }
    }
}
