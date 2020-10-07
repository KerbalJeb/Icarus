using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     The basic data structure used for all tiles
/// </summary>
public struct TileData
{
    // Health
    public byte Health;

    // Deform
    public sbyte DeformX;

    public sbyte DeformY;

    // Temp
    public short Temp;

    // Type
    public TileSprites Sprite;
    public TileTypes   Type;

    public TileData(byte        health  = default,
                    sbyte       deformX = default,
                    sbyte       deformY = default,
                    short       temp    = default,
                    TileSprites sprite  = TileSprites.Empty,
                    TileTypes   type    = default)
    {
        Health  = health;
        DeformX = deformX;
        DeformY = deformY;
        Temp    = temp;
        Sprite  = sprite;
        Type    = type;
    }

    public enum TileSprites : short
    {
        Empty,
        GrayHull,
        BlueHull,
    }

    public enum TileTypes : byte
    {
        Empty,
        Hull,
    }

    public bool Equals(TileData other) => Health == other.Health && DeformX == other.DeformX && DeformY == other.DeformY && Temp == other.Temp && Sprite == other.Sprite && Type == other.Type;

    public override bool Equals(object obj) => obj is TileData other && Equals(other);

    public override int GetHashCode() => (Health, Sprite, Temp, Type, DeformX, DeformY).GetHashCode();
}

public static class TileDataMapping
{
    private const int texWidth  = 2048;
    private const int texHeight = 2048;

    public static readonly Dictionary<TileData.TileSprites, Vector2[]> SpriteMapping =
        new Dictionary<TileData.TileSprites, Vector2[]>
        {
            {TileData.TileSprites.GrayHull, PixelsToUV(0,  0, 64, 64)},
            {TileData.TileSprites.Empty, PixelsToUV(0,     0, 0,  0)},
            {TileData.TileSprites.BlueHull, PixelsToUV(64, 0, 64, 64)},
        };

    private static Vector2[] PixelsToUV(int x, int y, int w, int h)
    {
        return new[]
        {
            new Vector2((float) (x + 0) / texWidth, (float) (y + h) / texHeight),
            new Vector2((float) (x + w) / texWidth, (float) (y + h) / texHeight),
            new Vector2((float) (x + w) / texWidth, (float) (y + 0) / texHeight),
            new Vector2((float) (x + 0) / texWidth, (float) (y + 0) / texHeight),
        };
    }
}
