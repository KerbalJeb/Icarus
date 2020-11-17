using TileSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
///     A basic class to generate asteroids
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class AsteroidTester : MonoBehaviour
{
    [SerializeField] private BasePart    tileBase;
    public                   int         seed;
    public                   int         xMax       = 30;
    public                   int         yMax       = 30;
    public                   int         noiseScale = 15;
    private                  int         lastSeed;
    private                  TileManager tilemap;

    private void Awake()
    {
        tilemap = GetComponent<TileManager>();
    }

    private void Start()
    {
        var asteroid = new AsteroidsGenerator(30, 30, 15);
        asteroid.SetTilemap(ref tilemap, tileBase);
    }

    private void Update()
    {
        if (seed == lastSeed) return;
        tilemap.ResetTiles();
        var asteroid = new AsteroidsGenerator(xMax, yMax, noiseScale, seed);
        asteroid.SetTilemap(ref tilemap, tileBase);
        lastSeed = seed;
    }
}
