public struct TileData
{
    public TileData(short health, ushort id)
    {
        Health = health;
        ID = id;
    }

    public TileData(TileSet.TileType tileType)
    {
        Health = tileType.MaxHealth;
        ID = tileType.ID;
    }

    public short Health;
    public ushort ID;
}