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
        public float      thrust;
        public GameObject exhaust;
        public Vector2    exhaustPos;

        public override void Instantiate(Vector3Int cord, Tilemap tilemap, Directions direction)
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

        public override void SetTiles(Vector3Int[] cords, Tilemap tilemap, Directions[] directions)
        {
            for (var i = 0; i < cords.Length; i++) Instantiate(cords[i], tilemap, directions[i]);
        }

        public override void RemoveTiles(Vector3Int[] cords, Tilemap tilemap)
        {
            foreach (Vector3Int cord in cords) Remove(cord, tilemap);
        }

        public override void Remove(Vector3Int cords, Tilemap tilemap)
        {
            base.Remove(cords, tilemap);
            var shipManager = tilemap.transform.parent.GetComponent<ShipManager>();
            shipManager.MovementManager.RemoveEngine(cords);
        }
    }
}
