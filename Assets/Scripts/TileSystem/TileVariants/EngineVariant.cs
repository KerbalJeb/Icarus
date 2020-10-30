using System;
using UnityEngine;

namespace TileSystem.TileVariants
{
    public class EngineVariant : FunctionalTileVariant
    {
        public readonly float Thrust;

        public EngineVariant(string jsonText) : base(jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            Thrust = json.Thrust;
        }

        [Serializable]
        private class Json
        {
            public float Thrust;
        }
    }
}
