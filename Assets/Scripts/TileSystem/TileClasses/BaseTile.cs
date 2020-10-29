using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem.TileClasses
{
    public class BaseTile : JsonTile
    {
        public readonly TileBase TileBase;
        public          ushort   ID;
        public          string   Name;

        public BaseTile(string jsonText, ushort id) : base(jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            TileBase = Resources.Load<TileBase>(json.TilePath);
            ID       = id;
            Name     = json.Name;
        }

        [Serializable]
        private class Json
        {
            public string TilePath = null;
            public string Name     = null;
        }
    }
}
