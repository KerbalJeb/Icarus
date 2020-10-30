using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TileSystem.TileVariants;
using UnityEngine;

namespace TileSystem
{
    public class TileSet
    {
        private static readonly Dictionary<string, TileSet> TileSets = new Dictionary<string, TileSet>();

        public readonly ReadOnlyDictionary<ushort, FunctionalTileVariant> FunctionalTiles;
        public readonly ReadOnlyDictionary<string, FunctionalTileVariant> FunctionalTilesDict;
        public readonly ReadOnlyDictionary<ushort, StructuralTileVariant> StructuralTiles;
        public readonly ReadOnlyDictionary<string, StructuralTileVariant> StructuralTilesDict;

        private TileSet(string path)
        {
            var jsonData = Resources.LoadAll<TextAsset>(path);

            var functionalTiles = new Dictionary<ushort, FunctionalTileVariant>();
            var structuralTiles = new Dictionary<ushort, StructuralTileVariant>();

            var functionalTilesDict = new Dictionary<string, FunctionalTileVariant>();
            var structuralTilesDict = new Dictionary<string, StructuralTileVariant>();

            foreach (TextAsset file in jsonData)
            {
                var      jsonTile = new JsonTile(file.text);
                BaseTileVariant tileVariant;

                switch (jsonTile.TileVariant)
                {
                    case TileVariants.TileVariants.Armor:
                        tileVariant = new ArmorVariant(file.text);
                        break;
                    case TileVariants.TileVariants.Engine:
                        tileVariant = new EngineVariant(file.text);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (tileVariant)
                {
                    case FunctionalTileVariant functionalTile:
                        functionalTiles[tileVariant.ID]                   = functionalTile;
                        functionalTilesDict[functionalTile.TileBase.name] = functionalTile;
                        break;
                    case StructuralTileVariant structuralTile:
                        structuralTiles[tileVariant.ID]                   = structuralTile;
                        structuralTilesDict[structuralTile.TileBase.name] = structuralTile;
                        break;
                }
            }

            TileSets[path]      = this;
            FunctionalTiles     = new ReadOnlyDictionary<ushort, FunctionalTileVariant>(functionalTiles);
            StructuralTiles     = new ReadOnlyDictionary<ushort, StructuralTileVariant>(structuralTiles);
            FunctionalTilesDict = new ReadOnlyDictionary<string, FunctionalTileVariant>(functionalTilesDict);
            StructuralTilesDict = new ReadOnlyDictionary<string, StructuralTileVariant>(structuralTilesDict);
        }

        public static TileSet GetTileSet(string path)
        {
            if (!TileSets.ContainsKey(path)) TileSets[path] = new TileSet(path);

            return TileSets[path];
        }
    }
}
