using System;
using UnityEngine;

namespace TileSystem.TileVariants
{
    public class StructuralTileVariant : BaseTileVariant
    {
        public readonly float  DamageResistance;
        public readonly int    MaxHealth;

        public StructuralTileVariant(string jsonText) : base(jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            MaxHealth        = json.MaxHealth;
            DamageResistance = json.DamageResistance;
        }

        [Serializable]
        private class Json
        {
            public int   MaxHealth;
            public float DamageResistance;
        }
    }
}
