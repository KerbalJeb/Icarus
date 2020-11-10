using System;

namespace TileSystem
{
    /// <summary>
    ///     This struct is used store data about 'instances' of tiles
    /// </summary>
    [Serializable]
    public struct TileInstanceData
    {
        /// <value>
        ///     The health of the tile
        /// </value>
        public ushort Health;

        /// <value>
        ///     The Rotation of this tile
        /// </value>
        public Directions Rotation;

        public TileInstanceData(BasePart partType, Directions rotation = Directions.Up)
        {
            Health   = partType.maxHealth;
            ID       = partType.id;
            Rotation = rotation;
        }

        /// <value>
        ///     The ID of the structural tile
        /// </value>
        public ushort ID { get; }
    }
}
