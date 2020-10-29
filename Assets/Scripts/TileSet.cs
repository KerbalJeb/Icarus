using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TileClasses;
using UnityEngine;

public class TileSet
{
    private static readonly Dictionary<string, TileSet> TileSets = new Dictionary<string, TileSet>();

    public readonly ReadOnlyDictionary<ushort, FunctionalTile> FunctionalTiles;

    public readonly ReadOnlyDictionary<string, FunctionalTile> FunctionalTilesDict;
    public readonly ReadOnlyDictionary<ushort, StructuralTile> StructuralTiles;
    public readonly ReadOnlyDictionary<string, StructuralTile> StructuralTilesDict;

    private TileSet(string path)
    {
        var jsonData = Resources.LoadAll<TextAsset>(path);

        var functionalTiles = new Dictionary<ushort, FunctionalTile>();
        var structuralTiles = new Dictionary<ushort, StructuralTile>();

        var functionalTilesDict = new Dictionary<string, FunctionalTile>();
        var structuralTilesDict = new Dictionary<string, StructuralTile>();

        foreach (var file in jsonData)
        {
            var jsonTile = new JsonTile(file.text);
            BaseTile tile;

            switch (jsonTile.TileClass)
            {
                case TileClass.Armor:
                    tile = new Armor(file.text);
                    break;
                case TileClass.Engine:
                    tile = new Engine(file.text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (tile)
            {
                case FunctionalTile functionalTile:
                    functionalTiles[tile.ID] = functionalTile;
                    functionalTilesDict[functionalTile.TileBase.name] = functionalTile;
                    break;
                case StructuralTile structuralTile:
                    structuralTiles[tile.ID] = structuralTile;
                    structuralTilesDict[structuralTile.TileBase.name] = structuralTile;
                    break;
            }
        }

        TileSets[path] = this;
        FunctionalTiles = new ReadOnlyDictionary<ushort, FunctionalTile>(functionalTiles);
        StructuralTiles = new ReadOnlyDictionary<ushort, StructuralTile>(structuralTiles);
        FunctionalTilesDict = new ReadOnlyDictionary<string, FunctionalTile>(functionalTilesDict);
        StructuralTilesDict = new ReadOnlyDictionary<string, StructuralTile>(structuralTilesDict);
    }

    public static TileSet GetTileSet(string path)
    {
        if (!TileSets.ContainsKey(path)) TileSets[path] = new TileSet(path);

        return TileSets[path];
    }
}