using System;
using System.Collections.Generic;
using TileSystem;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///     A class to generate procedural asteroids from a seed
/// </summary>
public class AsteroidsGenerator
{
    private readonly float        LargeScaleNoise = 0.1f;
    private readonly Vector3Int[] points;
    private readonly float        SmallScaleNoise = 0.2f;

    /// <summary>
    ///     Constructor for the asteroid generator
    /// </summary>
    /// <param name="xMax">
    ///     x 'radius' of the asteroid ellipse (semi-minor or semi-major axis, depending on if xMax or yMax is
    ///     greater)
    /// </param>
    /// <param name="yMax">
    ///     y 'radius' of the asteroid ellipse (semi-minor or semi-major axis, depending on if xMax or yMax is
    ///     greater)
    /// </param>
    /// <param name="noiseOffsetScale">The scale of the noise offet, larger numbers will make rougher asteroids</param>
    /// <param name="seed"></param>
    public AsteroidsGenerator(int xMax, int yMax, int noiseOffsetScale, int seed = 0)
    {
        Random.InitState(seed);
        noiseOffsetScale = Math.Min(noiseOffsetScale, Math.Min(xMax, yMax));
        int     squaredNoiseScale = noiseOffsetScale        * noiseOffsetScale;
        Vector2 noiseOffset       = Random.insideUnitCircle * 1e3f;

        var boundsInt  = new BoundsInt(-xMax, -yMax, 0, 2 * xMax, 2 * yMax, 1);
        var pointsList = new List<Vector3Int>();

        foreach (Vector3Int vector3Int in boundsInt.allPositionsWithin)
        {
            if (InsideAsteroid(vector3Int)) pointsList.Add(vector3Int);
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

    /// <summary>
    ///     Set a tilemanager with the data for the asteroid
    /// </summary>
    /// <param name="tilemap">The tilemanager to set</param>
    /// <param name="tileBase">The part variant to use for the asteroid material</param>
    public void SetTilemap(ref TileManager tilemap, BasePart tileBase)
    {
        var tiles = new BasePart[points.Length];
        for (var i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tileBase;
            tilemap.SetTile(points[i], tileBase);
        }
    }
}
