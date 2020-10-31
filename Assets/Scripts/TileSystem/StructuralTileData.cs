using TileSystem.TileVariants;

namespace TileSystem
{
    /// <summary>
    ///     This struct is used store data about 'instances' of structural tiles
    /// </summary>
    public struct StructuralTileData
    {
        public StructuralTileData(StructuralTileVariant tileVariantType, TileRotation rotation=TileRotation.Up)
        {
            Health   = (ushort) tileVariantType.MaxHealth;
            ID       = tileVariantType.ID;
            Rotation = rotation;
        }

        /// <value>
        ///     The health of the tile
        /// </value>
        public ushort Health;

        /// <value>
        ///     The ID of the structural tile
        /// </value>
        public ushort ID { get; }
        
        /// <value>
        /// The Rotation of this tile
        /// </value>
        public TileRotation Rotation;
    }
}
