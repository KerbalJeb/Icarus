using System.Collections.Generic;
using TileSystem;
using UnityEngine;

// TODO add floating origin
// TODO use objectPool

/// <summary>
/// A class to generate maps
/// </summary>
public class MapTester : MonoBehaviour
{
    /// <value>
    /// The template to use for the tilemap
    /// </value>
    [SerializeField] private GameObject   tilemap;
    /// <value>
    /// The part to use for asteroids
    /// </value>
    [SerializeField] private BasePart     tileBase;
    /// <value>
    /// An object pool to use
    /// </value>
    [SerializeField] private ObjectPool   pool;
    private                  MapGenerator mapGenerator;

    private void Start()
    {
        mapGenerator = new MapGenerator(0,tileBase);
        mapGenerator.Generate(Square(10, 10), tilemap, transform, pool);
    }

    /// <summary>
    /// Gets all points within a square from -x to x-1 and -y to y-1
    /// </summary>
    /// <param name="x">The max absolute x cord</param>
    /// <param name="y">The max absolute y cord</param>
    /// <returns>An iterator for all points with the square</returns>
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