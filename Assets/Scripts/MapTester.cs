using System;
using System.Collections.Generic;
using TileSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTester : MonoBehaviour
{
    [SerializeField] private GameObject   tilemap;
    [SerializeField] private BasePart     tileBase;
    [SerializeField] private ObjectPool   pool;
    private                  MapGenerator mapGenerator;

    private void Start()
    {
        mapGenerator = new MapGenerator(0,tileBase);
        mapGenerator.Generate(Square(10, 10), tilemap, transform, pool);
    }

    private static IEnumerable<Vector2Int> Square(int x, int y)
    {
        for (int i = -x; i < x; i++)
        {
            for (int j = -y; j < y; j++)
            {
                 yield return new Vector2Int(i, j);
            }
        }
    }
}