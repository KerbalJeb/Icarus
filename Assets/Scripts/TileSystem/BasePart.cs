using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
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
    }
}

