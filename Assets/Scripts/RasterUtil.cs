using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A helper class for raster drawing utilities (ie. drawing lines or boxes)
/// </summary>
public static class RasterUtil
{
    /// <summary>
    ///     Used to apply an action to every tile in a line between two points
    /// </summary>
    /// <param name="p0">The starting point</param>
    /// <param name="p1">The end point</param>
    public static IEnumerable<Vector3Int> Line(Vector3Int p0, Vector3Int p1)
    {
        // Bresenham's line algorithm (https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm#All_cases)
        int dx  = Math.Abs(p1.x - p0.x);
        int sx  = p0.x < p1.x ? 1 : -1;
        int dy  = -Math.Abs(p1.y - p0.y);
        int sy  = p0.y < p1.y ? 1 : -1;
        int err = dx + dy;

        int x = p0.x;
        int y = p0.y;

        while (true)
        {
            yield return new Vector3Int(x, y, 0);
            if (x == p1.x && y == p1.y) yield break;

            int e2 = 2 * err;

            if (e2 >= dy)
            {
                err += dy;
                x   += sx;
            }

            if (e2 <= dx)
            {
                err += dx;
                y   += sy;
            }
        }
    }
}
