using System;
using UnityEngine;

namespace TileSystem.TileClasses
{
    public class FunctionalTile : BaseTile
    {
        private static  ushort idIdx = 0;
        public readonly float  PowerGeneration;

        public FunctionalTile(string jsonText) : base(jsonText, idIdx)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            PowerGeneration = json.PowerGeneration;
            idIdx++;
        }

        [Serializable]
        private class Json
        {
            public float PowerGeneration = 0f;
        }
    }
}
