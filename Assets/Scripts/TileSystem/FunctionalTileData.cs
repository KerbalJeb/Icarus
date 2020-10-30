using TileSystem.TileVariants;

namespace TileSystem
{
    /// <summary>
    ///     This struct is used to store data about 'instances' of functional tiles
    /// </summary>
    public struct FunctionalTileData
    {
        public FunctionalTileData(FunctionalTileVariant baseTileVariant)
        {
            ID        = baseTileVariant.ID;
        }

        /// <value>
        ///     The ID of the functional tile
        /// </value>
        public ushort ID { get; }
    }
}
