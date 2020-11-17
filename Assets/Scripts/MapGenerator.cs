using System.Collections.Generic;
using TileSystem;
using UnityEngine;

/// <summary>
///     Used to generate a map from a seed
/// </summary>
public class MapGenerator
{
    private readonly float    density    = 0.25f;
    private readonly float    noiseScale = 0.1f;
    private readonly float    scale      = 100f;
    private readonly float    threshold  = 0.5f;
    private readonly BasePart tileBase;
    private readonly int      xOffset;
    private readonly int      yOffset;

    /// <summary>
    ///     Map gen constructor
    /// </summary>
    /// <param name="seed">The seed to use for generation</param>
    /// <param name="tileBase">The part type to use for the asteroid material</param>
    public MapGenerator(int seed, BasePart tileBase)
    {
        Random.InitState(seed);
        xOffset       = (int) (Random.value * 1e6);
        yOffset       = (int) (Random.value * 1e6);
        this.tileBase = tileBase;
    }

    /// <summary>
    ///     Generates a map
    /// </summary>
    /// <param name="pos">The portion of the map to generate as the coordinates of 100x100 squares centred on the origin</param>
    /// <param name="tilemap">The tilemap template to use for the asteroids</param>
    /// <param name="grid">The grid objects to attach the tilemaps to</param>
    /// <param name="pool">The object pool to use</param>
    public void Generate(IEnumerable<Vector2Int> pos, GameObject tilemap, Transform grid, ObjectPool pool)
    {
        foreach (Vector2Int vector2Int in pos)
        {
            if (!CheckPoint(vector2Int) || vector2Int == Vector2Int.zero) continue;
            var asteroid = new AsteroidsGenerator(Random.Range(25, 35), Random.Range(25, 35), Random.Range(10, 20),
                                                  (vector2Int.y + yOffset) * (vector2Int.x + xOffset));
            var        posWorld      = new Vector2(vector2Int.x * scale, vector2Int.y * scale);
            GameObject mapGameObject = Object.Instantiate(tilemap, grid);
            mapGameObject.transform.position = posWorld + Random.insideUnitCircle * (scale - 50);
            int rot = Random.Range(0, 365);
            mapGameObject.transform.rotation                          = Quaternion.Euler(0, 0, rot);
            mapGameObject.GetComponent<Rigidbody2D>().angularVelocity = Random.Range(0, 5f);
            var map = mapGameObject.GetComponent<TileManager>();
            map.pool = pool;
            asteroid.SetTilemap(ref map, tileBase);
        }
    }

    private bool CheckPoint(Vector2Int pos)
    {
        float value = Mathf.PerlinNoise((pos.x + xOffset) * noiseScale, (pos.y + yOffset) * noiseScale);
        return value >= threshold && Random.value <= density;
    }
}
