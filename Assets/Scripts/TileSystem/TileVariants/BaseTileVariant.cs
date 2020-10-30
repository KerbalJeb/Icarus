using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem.TileVariants
{
    public class BaseTileVariant 
    {
        private static  ushort   idIdx;
        public readonly TileBase TileBase;
        public          ushort   ID;
        public          string   Name;

        public BaseTileVariant(string jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            TileBase = Resources.Load<TileBase>(json.TilePath);
            ID       = idIdx;
            Name     = json.Name;
            idIdx++;
        }

        [Serializable]
        private class Json
        {
            public string TilePath;
            public string Name;
        }
    }
}
