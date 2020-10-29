using TileSystem.TileClasses;

namespace TileSystem
{
    public struct StructuralTileData
    {
        public StructuralTileData(StructuralTile tileType)
        {
            Health    = (ushort) tileType.MaxHealth;
            ID        = tileType.ID;
            TileClass = tileType.TileClass;
        }

        public ushort    Health;
        public ushort    ID;
        public TileClass TileClass;
    }
}
