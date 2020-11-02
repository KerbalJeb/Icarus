using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem.TileVariants
{
    /// <summary>
    ///     The base for all tile variant classes. Contains the common TileBase, ID and Name fields.
    /// </summary>
    public class BaseTileVariant
    {
        public readonly TileBase TileBase;
        public          ushort   ID;
        public          string   Name;

        public BaseTileVariant(string jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            TileBase = Resources.Load<TileBase>(json.TilePath);
            ID       = IdIDx;
            Name     = json.Name;
            IdIDx++;
        }

        public static ushort IdIDx { get; set; }

        [Serializable]
        [SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
        private class Json
        {
            public string TilePath = null;
            public string Name     = null;
        }
    }
}
