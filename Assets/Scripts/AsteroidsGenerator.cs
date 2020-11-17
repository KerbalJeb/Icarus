﻿
using System;
using System.Collections.Generic;
using TileSystem;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class AsteroidsGenerator
{
    private Vector3Int[] points;
    private float        LargeScaleNoise = 0.1f;
    private float        SmallScaleNoise = 0.2f;
    public AsteroidsGenerator(int xMax, int yMax, int noiseOffsetScale, int seed=0)
    {
        Random.InitState(seed);
        noiseOffsetScale = Math.Min(noiseOffsetScale, Math.Min(xMax, yMax));
        var squaredNoiseScale = noiseOffsetScale        * noiseOffsetScale;
        var noiseOffset       = Random.insideUnitCircle * 1e3f;
        
        BoundsInt boundsInt = new BoundsInt(-xMax, -yMax, 0, 2*xMax, 2*yMax, 1);
        var pointsList = new List<Vector3Int>();
        
        foreach (Vector3Int vector3Int in boundsInt.allPositionsWithin)
        {
            if (InsideAsteroid(vector3Int))
            {
                pointsList.Add(vector3Int);
            }
        }

        points = pointsList.ToArray();
        
        bool InsideAsteroid(Vector3Int cords)
        {
            int   x     = cords.x;
            int   y     = cords.y;
            float angle = Mathf.Atan2(x, y);
            float xPos  = (xMax - noiseOffsetScale) * Mathf.Cos(angle);
            float yPos  = (yMax - noiseOffsetScale) * Mathf.Sin(angle);

            float noise = Mathf.PerlinNoise(noiseOffset.x + xPos * LargeScaleNoise,
                                            noiseOffset.y + yPos * LargeScaleNoise);
            float smallNoise = Mathf.PerlinNoise(noiseOffset.x + xPos * SmallScaleNoise,
                                                 noiseOffset.y + yPos * SmallScaleNoise);

            float ellipseRad = xPos                * xPos
                             + yPos                * yPos
                             + (noise      - 0.5f) * squaredNoiseScale
                             + (smallNoise - 0.5f) * (squaredNoiseScale / 4f);

            float posRad = (x + 0.5f) * (x + 0.5f) + (y + 0.5f) * (y + 0.5f);
            return posRad < ellipseRad;
        }
    }

    public void SetTilemap(ref TileManager tilemap,  BasePart tileBase)
    {
        var tiles = new BasePart[points.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tileBase;
            tilemap.SetTile(points[i], tileBase);
        }
    }
}

