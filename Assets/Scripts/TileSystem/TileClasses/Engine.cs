using System;
using UnityEngine;

namespace TileSystem.TileClasses
{
    public class Engine : FunctionalTile
    {
        public readonly float Thrust;

        public Engine(string jsonText) : base(jsonText)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            Thrust = json.Thrust;
        }

        public override TileClass TileClass => TileClass.Engine;

        [Serializable]
        private class Json
        {
            public float Thrust = 0f;
        }
    }
}
