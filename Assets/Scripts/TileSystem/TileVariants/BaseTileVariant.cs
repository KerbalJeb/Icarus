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
        public          int      Layer;
        public          ushort   MaxHealth;
        public          float    DamageResistance;

        public BaseTileVariant(string jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            TileBase         = Resources.Load<TileBase>(json.TilePath);
            ID               = IdIDx;
            Name             = json.Name;
            Layer            = json.Layer;
            MaxHealth        = json.MaxHealth;
            DamageResistance = json.DamageResistance;
            IdIDx++;
        }

        public static ushort IdIDx { get; set; }

        [Serializable]
        private class Json
        {
            public string TilePath         = null;
            public string Name             = null;
            public int    Layer            = 0;
            public ushort MaxHealth        = 0;
            public float  DamageResistance = 1f;
        }
    }
}
