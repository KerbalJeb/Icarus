using UnityEngine;

/// <summary>
///     The basic data structure used for all tiles
/// </summary>
public struct TileData
{
    // Health
    public ushort Health;

    // Temp
    public ushort Temp;

    // Type
    public ushort TypeID;

    public TileData(ushort health = default,
        ushort temp = default,
        ushort typeID = default)
    {
        Health = health;
        Temp = temp;
        TypeID = typeID;
    }

    public bool Equals(TileData other)
    {
        return Health == other.Health && Temp == other.Temp && TypeID == other.TypeID;
    }

    public override bool Equals(object obj)
    {
        return obj is TileData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (Health, Temp, Type: TypeID).GetHashCode();
    }
}

public static class TileDataHelper
{
    private const int texWidth = 2048;
    private const int texHeight = 2048;

    public static Vector2[] PixelsToUV(uint x, uint y, uint w, uint h)
    {
        return new[]
        {
            new Vector2((float) (x + 0) / texWidth, (float) (y + h) / texHeight),
            new Vector2((float) (x + w) / texWidth, (float) (y + h) / texHeight),
            new Vector2((float) (x + w) / texWidth, (float) (y + 0) / texHeight),
            new Vector2((float) (x + 0) / texWidth, (float) (y + 0) / texHeight)
        };
    }
}