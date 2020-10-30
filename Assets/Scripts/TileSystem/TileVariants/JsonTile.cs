using System;
using UnityEngine;

namespace TileSystem.TileVariants
{
    /// <summary>
    /// Used to parse JSON files and determine what variant was parsed
    /// </summary>
    public class JsonTile
    {
        private readonly TileVariants tileVariant;

        public JsonTile(string jsonText)
        {
            var  json       = JsonUtility.FromJson<Json>(jsonText);
            bool validClass = Enum.TryParse(json.TileType, out tileVariant);

            if (!validClass) Debug.LogError("Invalid Tile Type: " + json.TileType);
        }

        public TileVariants TileVariant => tileVariant;

        [Serializable]
        private class Json
        {
            public string TileType;
        }
    }
}
