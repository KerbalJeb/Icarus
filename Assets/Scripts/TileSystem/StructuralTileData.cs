using TileSystem.TileVariants;

namespace TileSystem
{
    /// <summary>
    ///     This struct is used store data about 'instances' of structural tiles
    /// </summary>
    public struct StructuralTileData
    {
        public StructuralTileData(StructuralTileVariant tileVariantType)
        {
            Health    = (ushort) tileVariantType.MaxHealth;
            ID        = tileVariantType.ID;
        }

        /// <value>
        ///     The health of the tile
        /// </value>
        public ushort Health;

        /// <value>
        ///     The ID of the structural tile
        /// </value>
        public ushort ID { get;}
    }
}
