using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    /// <summary>
    ///     The base tile variant, all other tile variant types should inherit from this
    /// </summary>
    [CreateAssetMenu(fileName = "BasePart", menuName = "TileParts/Hull", order = 0)]
    public class BasePart : ScriptableObject
    {
        /// <value>
        ///     The name of this part to use in game
        /// </value>
        public string partName;

        /// <value>
        ///     A unique name for this part to use in code
        /// </value>
        public string partID;

        /// <value>
        ///     The category this part should be placed in the tile selector UI
        /// </value>
        public string category;

        /// <value>
        ///     The tile to use
        /// </value>
        public TileBase tile;

        /// <value>
        ///     How much to multiply the damage applied to this part by (1 is full damage, 0 is no damage)
        /// </value>
        public float damageResistance;

        /// <summary>
        ///     The health of this part
        /// </summary>
        public ushort maxHealth;

        /// <summary>
        ///     What layer in the tilemanager this part is drawn on
        /// </summary>
        public int layer;

        /// <summary>
        ///     An image to use in the tile selector
        /// </summary>
        public Sprite previewImg;

        /// <summary>
        ///     The mass of this part
        /// </summary>
        public float mass;

        /// <summary>
        ///     If this part will be shown in the part selector UI
        /// </summary>
        public bool showInPartSelector = true;

        /// <summary>
        ///     A unique ID, created at runtime
        /// </summary>
        [HideInInspector] public ushort id;

        /// <summary>
        ///     Creates a tile at the given coordinates
        /// </summary>
        /// <param name="cord">The position of the engine</param>
        /// <param name="tilemap">The tilemap to use</param>
        /// <param name="direction">The direction it is facing</param>
        public virtual void Instantiate(Vector3Int cord, Tilemap tilemap, Direction direction)
        {
            tilemap.SetTile(cord, tile);
            tilemap.SetTransformMatrix(cord, TileInfo.TransformMatrix[direction]);
        }

        /// <summary>
        ///     Sets multiple tiles
        /// </summary>
        /// <param name="cords">The positions of the tile</param>
        /// <param name="tilemap">The tilemap to use</param>
        /// <param name="directions">The directions it is facing</param>
        public virtual void SetTiles(Vector3Int[] cords, Tilemap tilemap, Direction[] directions)
        {
            var tiles                                       = new TileBase[cords.Length];
            for (var i = 0; i < tiles.Length; i++) tiles[i] = tile;
            tilemap.SetTiles(cords, tiles);

            for (var i = 0; i < tiles.Length; i++)
            {
                Direction dir = directions[i];
                if (dir == Direction.Up) continue;
                tilemap.SetTransformMatrix(cords[i], TileInfo.TransformMatrix[dir]);
            }
        }

        /// <summary>
        ///     Removes tiles at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates to remove tiles from</param>
        /// <param name="tilemap">The tilemap to use</param>
        public virtual void RemoveTiles(Vector3Int[] cords, Tilemap tilemap)
        {
            var tiles                                       = new TileBase[cords.Length];
            for (var i = 0; i < tiles.Length; i++) tiles[i] = null;
            tilemap.SetTiles(cords, tiles);
        }

        /// <summary>
        ///     Removes a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinate to remove tiles from</param>
        /// <param name="tilemap">The tilemap to use</param>
        public virtual void Remove(Vector3Int cords, Tilemap tilemap)
        {
            tilemap.SetTile(cords, null);
        }
    }
}
