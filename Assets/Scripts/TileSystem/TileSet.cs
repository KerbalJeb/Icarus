using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TileSystem.TileVariants;
using UnityEngine;

namespace TileSystem
{
    public class TileSet
    {
        private static readonly Dictionary<string, TileSet> TileSets = new Dictionary<string, TileSet>();

        public readonly HashSet<int> ActiveLayers = new HashSet<int>();

        /// <value>
        ///     Maps the name of a TileBase object to the ID of the tile variant that uses it
        /// </value>
        public readonly ReadOnlyDictionary<string, ushort> TilemapNameToID;

        /// <value>
        ///     A list of all the tile variants contained in this tile set, the variant's ID corresponds to its index in the list
        /// </value>
        public readonly ReadOnlyCollection<BaseTileVariant> TileVariants;

        private readonly ReadOnlyCollection<Type> tileVariantTypes =
            new ReadOnlyCollection<Type>(GetAllDerived(typeof(BaseTileVariant)).ToList());

        /// <value>
        ///     Maps the name of a tile variant given in the JSON file to the variant's ID
        /// </value>
        public readonly ReadOnlyDictionary<string, ushort> VariantNameToID;

        /// <summary>
        ///     Creates a new TileSet for a given path
        /// </summary>
        /// <param name="path">The path containing the JSON file</param>
        private TileSet(string path)
        {
            BaseTileVariant.IdIDx = 0;
            var jsonData = Resources.LoadAll<TextAsset>(path);

            var tileVariants                = new List<BaseTileVariant>(jsonData.Length);
            var tileVariantsTileNameDict    = new Dictionary<string, ushort>();
            var tileVariantsVariantNameDict = new Dictionary<string, ushort>();

            foreach (TextAsset file in jsonData)
            {
                var jsonTile     = JsonUtility.FromJson<JsonTile>(file.text);
                var variantTypes = tileVariantTypes.Where(type => type.Name == jsonTile.TileType).ToArray();

                if (variantTypes.Length != 1)
                {
                    Debug.LogWarning("Found " + variantTypes.Length + " Matches for " + jsonTile.TileType);
                    continue;
                }

                Type variantType = variantTypes[0];
                var  variant     = Activator.CreateInstance(variantType, file.text) as BaseTileVariant;
                if (variant == null) continue;

                tileVariants.Add(variant);
                ActiveLayers.Add(variant.Layer);
                tileVariantsTileNameDict[variant.TileBase.name] = variant.ID;
                tileVariantsVariantNameDict[variant.Name]       = variant.ID;
            }

            TileSets[path]  = this;
            TileVariants    = new ReadOnlyCollection<BaseTileVariant>(tileVariants);
            TilemapNameToID = new ReadOnlyDictionary<string, ushort>(tileVariantsTileNameDict);
            VariantNameToID = new ReadOnlyDictionary<string, ushort>(tileVariantsVariantNameDict);
        }

        /// <summary>
        ///     Gets (or creates) based on all the json files present in the given path. A TileSet will only be created once per
        ///     path, all subsequent calls will return a reference to the original instance
        /// </summary>
        /// <param name="path">A path containing json files</param>
        /// <returns>A TileSet that contains all tile variants created from the json files in path</returns>
        public static TileSet GetTileSet(string path)
        {
            if (!TileSets.ContainsKey(path)) TileSets[path] = new TileSet(path);

            return TileSets[path];
        }

        /// <summary>
        ///     Gets all the classes that inherit from a class
        /// </summary>
        /// <param name="baseClass">The base class</param>
        /// <returns>The Type for each class that inherits from baseClass</returns>
        private static IEnumerable<Type> GetAllDerived(Type baseClass)
        {
            return Assembly.GetAssembly(baseClass).GetTypes()
                           .Where(theType => theType.IsClass && !theType.IsAbstract && theType.IsSubclassOf(baseClass));
        }

        [Serializable]
        private class JsonTile
        {
            public string TileType = null;
        }
    }
}
