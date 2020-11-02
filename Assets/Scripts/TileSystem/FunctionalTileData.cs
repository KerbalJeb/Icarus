using TileSystem.TileVariants;

namespace TileSystem
{
    /// <summary>
    ///     This struct is used to store data about 'instances' of functional tiles
    /// </summary>
    public struct FunctionalTileData
    {
        public FunctionalTileData(FunctionalTileVariant baseTileVariant, TileRotation rotation = TileRotation.Up)
        {
            ID       = baseTileVariant.ID;
            Rotation = rotation;
        }

        /// <value>
        ///     The ID of the functional tile
        /// </value>
        public ushort ID { get; }

        /// <value>
        ///     The Rotation of this tile
        /// </value>
        public TileRotation Rotation;
    }
}
