using TileClasses;

public struct FunctionalTileData
{
    public FunctionalTileData(FunctionalTile baseTile)
    {
        ID = baseTile.ID;
        TileClass = baseTile.TileClass;
    }

    public ushort ID;
    public TileClass TileClass;
}