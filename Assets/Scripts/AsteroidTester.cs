﻿using System;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Tilemap))]
public class AsteroidTester : MonoBehaviour
{
    private                  Tilemap  tilemap;
    [SerializeField] private TileBase tileBase;
    public                   int      seed;
    private                  int      lastSeed;
    public                   int      xMax=30;
    public                   int      yMax=30;
    public                   int      noiseScale=15;
    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        var asteroid = new AsteroidsGenerator(30, 30, 15);
        asteroid.SetTilemap(ref tilemap, tileBase);
    }

    private void Update()
    {
        if (seed == lastSeed) return;
        tilemap.ClearAllTiles();
        var asteroid = new AsteroidsGenerator(xMax, yMax, noiseScale, seed);
        asteroid.SetTilemap(ref tilemap, tileBase);
        lastSeed = seed;
    }
}

