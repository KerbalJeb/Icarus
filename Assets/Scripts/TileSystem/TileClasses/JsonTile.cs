using System;
using UnityEngine;

namespace TileSystem.TileClasses
{
    public class JsonTile
    {
        private readonly TileClass tileClass;

        public JsonTile(string jsonText)
        {
            var json       = JsonUtility.FromJson<Json>(jsonText);
            var validClass = Enum.TryParse(json.TileType, out tileClass);

            if (!validClass)
            {
                Debug.LogError("Invalid Tile Type: " + json.TileType);
            }
        }

        public virtual TileClass TileClass => tileClass;

        [Serializable]
        private class Json
        {
            public string TileType = null;
        }
    }
}
