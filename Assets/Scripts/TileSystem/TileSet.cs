using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TileSystem
{
    public class TileSet
    {
        private static readonly Dictionary<string, TileSet> TileSets = new Dictionary<string, TileSet>();

        public readonly HashSet<int> ActiveLayers = new HashSet<int>();

        /// <value>
        ///     A list of all the tile variants contained in this tile set, the variant's ID corresponds to its index in the list
        /// </value>
        public readonly ReadOnlyCollection<BasePart> TileVariants;
        

        /// <value>
        ///     Maps the name of a tile variant given in the JSON file to the variant's ID
        /// </value>
        public readonly ReadOnlyDictionary<string, ushort> VariantNameToID;
        
        public readonly ReadOnlyDictionary<ushort, string> VariantIDToName;

        /// <summary>
        ///     Creates a new TileSet for a given path
        /// </summary>
        /// <param name="path">The path containing the JSON file</param>
        private TileSet(string path)
        {
            var variants = Resources.LoadAll<BasePart>("Tiles/Parts");
            var allVariants = new List<BasePart>();
            var variantNameDict = new Dictionary<string, ushort>();
            var idsDict = new Dictionary<ushort, string>();

            foreach (BasePart variant in variants)
            {
                if (variantNameDict.ContainsKey(variant.partID))
                {
                    Debug.LogError("Found duplicate of " + variant.partID);
                    continue;
                }
                variant.id = (ushort)allVariants.Count;
                allVariants.Add(variant);
                ActiveLayers.Add(variant.layer);
                variantNameDict[variant.partID] = variant.id;
                idsDict[variant.id]             = variant.partID;
            }
            TileVariants = new ReadOnlyCollection<BasePart>(allVariants);
            VariantNameToID = new ReadOnlyDictionary<string, ushort>(variantNameDict);
            VariantIDToName = new ReadOnlyDictionary<ushort, string>(idsDict);
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
    }
}
