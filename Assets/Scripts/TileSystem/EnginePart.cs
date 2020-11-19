using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    /// <summary>
    ///     The engine tile variant type
    /// </summary>
    [CreateAssetMenu(fileName = "Engine", menuName = "TileParts/Engine", order = 1)]
    public class EnginePart : BasePart
    {
        /// <summary>
        ///     How much thrust this engine produces
        /// </summary>
        public float thrust;

        /// <summary>
        ///     The gameobject template to use for the exhaust fx, must have a particle system
        /// </summary>
        public GameObject exhaust;

        /// <summary>
        ///     The offset of the exhaust gameobject
        /// </summary>
        public Vector2 exhaustPos;

        /// <summary>
        ///     Creates a tile at the given coordinates
        /// </summary>
        /// <param name="cord">The position of the engine</param>
        /// <param name="tilemap">The tilemap to use</param>
        /// <param name="direction">The direction it is facing</param>
        public override void Instantiate(Vector3Int cord, Tilemap tilemap, Direction direction)
        {
            base.Instantiate(cord, tilemap, direction);
            Quaternion rot = TileInfo.TransformMatrix[direction].rotation;
            GameObject gameObject = Instantiate(exhaust,
                                                tilemap.CellToLocal(cord) +
                                                new Vector3(0.5f * tilemap.cellSize.x, 0.5f * tilemap.cellSize.y),
                                                rot * Quaternion.Euler(90, 0, 0),
                                                tilemap.transform);

            gameObject.transform.position += rot * exhaustPos + 2 * Vector3.forward;
            tilemap.transform.parent.GetComponent<ShipManager>().MovementManager
                   .AddEngine(cord, this, gameObject, direction);
        }

        /// <summary>
        ///     Sets multiple tiles
        /// </summary>
        /// <param name="cords">The positions of the tile</param>
        /// <param name="tilemap">The tilemap to use</param>
        /// <param name="directions">The directions it is facing</param>
        public override void SetTiles(Vector3Int[] cords, Tilemap tilemap, Direction[] directions)
        {
            for (var i = 0; i < cords.Length; i++) Instantiate(cords[i], tilemap, directions[i]);
        }

        /// <summary>
        ///     Removes tiles at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates to remove tiles from</param>
        /// <param name="tilemap">The tilemap to use</param>
        public override void RemoveTiles(Vector3Int[] cords, Tilemap tilemap)
        {
            foreach (Vector3Int cord in cords) Remove(cord, tilemap);
        }

        /// <summary>
        ///     Removes a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinate to remove tiles from</param>
        /// <param name="tilemap">The tilemap to use</param>
        public override void Remove(Vector3Int cords, Tilemap tilemap)
        {
            base.Remove(cords, tilemap);
            var shipManager = tilemap.transform.parent.GetComponent<ShipManager>();
            shipManager.MovementManager.RemoveEngine(cords);
        }
    }
}
