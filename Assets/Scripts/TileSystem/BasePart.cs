using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    /// <summary>
    ///     The base tile variant, all other tile variant types should inherit from this
    /// </summary>
    [CreateAssetMenu(fileName = "BasePart", menuName = "TileParts/Hull", order = 0)]
    public class BasePart : ScriptableObject
    {
        public string   partName;
        public string   partID;
        public string   category;
        public TileBase tile;
        public float    damageResistance;
        public ushort   maxHealth;
        public int      layer;
        public Sprite   previewImg;
        public float    mass;

        [HideInInspector] public ushort id;

        public virtual void Instantiate(Vector3Int cord, Tilemap tilemap, Directions direction)
        {
            tilemap.SetTile(cord, tile);
            tilemap.SetTransformMatrix(cord, TileInfo.TransformMatrix[direction]);
        }

        public virtual void SetTiles(Vector3Int[] cords, Tilemap tilemap, Directions[] directions)
        {
            var tiles = new TileBase[cords.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = tile;
            }
            tilemap.SetTiles(cords, tiles);
            
            for (int i = 0; i < tiles.Length; i++)
            {
                var dir = directions[i];
                if (dir == Directions.Up)
                {
                    continue;
                }
                tilemap.SetTransformMatrix(cords[i], TileInfo.TransformMatrix[dir]);
            }
        }

        public virtual void RemoveTiles(Vector3Int[] cords, Tilemap tilemap)
        {
            var tiles = new TileBase[cords.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = null;
            }
            tilemap.SetTiles(cords, tiles);
        }

        public virtual void Remove(Vector3Int cords, Tilemap tilemap)
        {
            tilemap.SetTile(cords, null);
        }
    }
}
