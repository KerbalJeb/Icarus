using System;
using System.Collections.Generic;
using UnityEngine;

public class StructuralTileSet
{
    public Dictionary<ushort, TileData> IDMapping;
    public Dictionary<string, ushort> NameMapping;

    public StructuralTileSet(string path)
    {
        IDMapping = new Dictionary<ushort, TileData>();
        NameMapping = new Dictionary<string, ushort>();

        var loadedJson = Resources.LoadAll<TextAsset>(path);
        IDMapping[0] = new TileData("Empty");
        NameMapping["Empty"] = 0;
        ushort tileID = 1;

        foreach (var file in loadedJson)
        {
            if (tileID == 0) throw new OverflowException("Too many block types loaded!");

            IDMapping[tileID] = new TileData(JsonUtility.FromJson<JsonTile>(file.text));
            NameMapping[IDMapping[tileID].Name] = tileID;
            tileID++;
        }
    }

    public struct TileData
    {
        public TileData(JsonTile jsonTile)
        {
            Name = jsonTile.Name;
            Category = jsonTile.Category;
            PhysicalDamageResistance = jsonTile.PhysicalDamageResistance;
            MaxTemp = jsonTile.MaxTemp;
            ThermalDamageResistance = jsonTile.ThermalDamageResistance;
            HeatCapacity = jsonTile.HeatCapacity;
            ThermalConductivity = jsonTile.ThermalConductivity;
            Density = jsonTile.Density;
            Flexibility = jsonTile.Flexibility;

            UVMapping = TileDataHelper.PixelsToUV(jsonTile.TextureX, jsonTile.TextureY, jsonTile.TextureW,
                jsonTile.TextureH);
        }

        public TileData(string name = null,
            string category = null,
            float physicalDamageResistance = default,
            ushort maxTemp = default,
            float thermalDamageResistance = default,
            float heatCapacity = default,
            float thermalConductivity = default,
            float density = default,
            float flexibility = default,
            Vector2[] uvMapping = null)
        {
            Name = name;
            Category = category;
            PhysicalDamageResistance = physicalDamageResistance;
            MaxTemp = maxTemp;
            ThermalDamageResistance = thermalDamageResistance;
            HeatCapacity = heatCapacity;
            ThermalConductivity = thermalConductivity;
            Density = density;
            Flexibility = flexibility;
            UVMapping = uvMapping ?? new[] {new Vector2(), new Vector2(), new Vector2(), new Vector2()};
        }

        public string Name;
        public string Category;
        public float PhysicalDamageResistance;
        public ushort MaxTemp;
        public float ThermalDamageResistance;
        public float HeatCapacity;
        public float ThermalConductivity;
        public float Density;
        public float Flexibility;

        public Vector2[] UVMapping;
    }

    [Serializable]
    public struct JsonTile
    {
        public string Name;
        public string Category;
        public float PhysicalDamageResistance;
        public ushort MaxTemp;
        public float ThermalDamageResistance;
        public float HeatCapacity;
        public float ThermalConductivity;
        public float Density;
        public float Flexibility;
        public uint TextureX;
        public uint TextureY;
        public uint TextureW;
        public uint TextureH;

        public JsonTile(string name, string category, float physicalDamageResistance, ushort maxTemp,
            float thermalDamageResistance, float heatCapacity, float thermalConductivity, float density,
            float flexibility, uint textureX, uint textureY, uint textureW, uint textureH)
        {
            Name = name;
            Category = category;
            PhysicalDamageResistance = physicalDamageResistance;
            MaxTemp = maxTemp;
            ThermalDamageResistance = thermalDamageResistance;
            HeatCapacity = heatCapacity;
            ThermalConductivity = thermalConductivity;
            Density = density;
            Flexibility = flexibility;
            TextureX = textureX;
            TextureY = textureY;
            TextureW = textureW;
            TextureH = textureH;
        }
    }
}