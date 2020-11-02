using System;
using UnityEngine;

namespace TileSystem.TileVariants
{
    /// <summary>
    ///     The base for all functional or non structural tiles (tiles that consume power and don't have health)
    /// </summary>
    public class FunctionalTileVariant : BaseTileVariant
    {
        public readonly float PowerGeneration;

        public FunctionalTileVariant(string jsonText) : base(jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            PowerGeneration = json.PowerGeneration;
        }

        [Serializable]
        private class Json
        {
            public float PowerGeneration=0f;
        }
    }
}
