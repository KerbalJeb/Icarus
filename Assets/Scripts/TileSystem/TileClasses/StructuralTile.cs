using System;
using UnityEngine;

namespace TileSystem.TileClasses
{
    public class StructuralTile : BaseTile
    {
        private static  ushort idIdx = 0;
        public readonly float  DamageResistance;
        public readonly int    MaxHealth;

        public StructuralTile(string jsonText) : base(jsonText, idIdx)
        {
            var json = JsonUtility.FromJson<Json>(jsonText);
            MaxHealth        = json.MaxHealth;
            DamageResistance = json.DamageResistance;
            idIdx++;
        }

        public override TileClass TileClass => TileClass.Armor;

        [Serializable]
        private class Json
        {
            public int   MaxHealth        = 0;
            public float DamageResistance = 0;
        }
    }
}
