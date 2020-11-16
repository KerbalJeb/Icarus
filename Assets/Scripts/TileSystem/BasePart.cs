using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    /// <summary>
    /// The base tile variant, all other tile variant types should inherit from this
    /// </summary>
    [CreateAssetMenu(fileName = "BasePart", menuName = "TileParts/Hull", order = 0)]
    public class BasePart : ScriptableObject
    {
        public                   string   partName;
        public                   string   partID;
        public                   string   category;
        public                   TileBase tile;
        public                   float    damageResistance;
        public                   ushort   maxHealth;
        public                   int      layer;
        public                   Sprite   previewImg;
        [HideInInspector] public ushort   id;

        public virtual void Instantiate(Vector3Int cord, Tilemap tilemap, Directions direction)
        {
            tilemap.SetTile(cord, tile);
            tilemap.SetTransformMatrix(cord, TileInfo.TransformMatrix[direction]);
        }

        public virtual void Remove(Vector3Int cords, Tilemap tilemap)
        {
            tilemap.SetTile(cords, null);
        }
        
    }
}
