using TileSystem.TileVariants;

namespace TileSystem
{
    /// <summary>
    ///     This struct is used store data about 'instances' of tiles
    /// </summary>
    public struct TileInstanceData
    {
        public TileInstanceData(BaseTileVariant tileVariantType, Directions rotation = Directions.Up)
        {
            Health   = tileVariantType.MaxHealth;
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
        ///     The Rotation of this tile
        /// </value>
        public Directions Rotation;
    }
}
