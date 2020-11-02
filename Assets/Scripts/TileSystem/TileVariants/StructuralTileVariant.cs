using System;
using UnityEngine;

namespace TileSystem.TileVariants
{
    /// <summary>
    ///     The base class for all structural tiles (Have health, don't consume power)
    /// </summary>
    public class StructuralTileVariant : BaseTileVariant
    {
        public readonly float DamageResistance;
        public readonly int   MaxHealth;

        public StructuralTileVariant(string jsonText) : base(jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            MaxHealth        = json.MaxHealth;
            DamageResistance = json.DamageResistance;
        }

        [Serializable]
        private class Json
        {
            public int   MaxHealth        = 0;
            public float DamageResistance = 0;
        }
    }
}
