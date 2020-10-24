using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSet
{
    private static readonly Dictionary<string, TileSet> TileSets = new Dictionary<string, TileSet>();
    public readonly ReadOnlyCollection<TileType> TileTypes;
    public readonly ReadOnlyDictionary<string, TileType> TileTypesDict;

    private TileSet(string path)
    {
        var jsonData = Resources.LoadAll<TextAsset>(path);
        var tileTypes = new List<TileType>();
        var tileTypesDict = new Dictionary<string, TileType>();
        foreach (var file in jsonData)
        {
            var jsonTile = JsonUtility.FromJson<JsonTile>(file.text);
            int id = tileTypes.Count;
            var tileType = new TileType(id, jsonTile.DamageResistance, jsonTile.MaxHealth, jsonTile.TilePath);
            tileTypesDict[tileType.TileBase.name] = tileType;
            tileTypes.Add(tileType);
        }

        TileTypes = new ReadOnlyCollection<TileType>(tileTypes);
        TileTypesDict = new ReadOnlyDictionary<string, TileType>(tileTypesDict);
        TileSets[path] = this;
    }

    public static TileSet GetTileSet(string path)
    {
        if (!TileSets.ContainsKey(path))
        {
            TileSets[path] = new TileSet(path);
        }

        return TileSets[path];
    }

    [Serializable]
    private struct JsonTile
    {
        public float DamageResistance;
        public short MaxHealth;
        public string TilePath;

        private JsonTile(float damageResistance, short maxHealth, string tilePath)
        {
            DamageResistance = damageResistance;
            MaxHealth = maxHealth;
            TilePath = tilePath;
        }
    }

    public class TileType
    {
        public float DamageResistance;
        public ushort ID;
        public short MaxHealth;
        public TileBase TileBase;

        public TileType(int id, float damageResistance, short maxHealth, string tilePath)
        {
            DamageResistance = damageResistance;
            MaxHealth = maxHealth;
            TileBase = Resources.Load<TileBase>(tilePath);
            ID = (ushort) id;
        }
    }
}