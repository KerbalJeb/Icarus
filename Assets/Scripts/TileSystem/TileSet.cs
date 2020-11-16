using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace TileSystem
{
    /// <summary>
    ///     A singleton that will load all tile variants present in Resources/Tiles/Parts
    /// </summary>
    public class TileSet
    {
        /// <value>
        ///     The singleton instace
        /// </value>
        private static TileSet instance;

        /// <summary>
        ///     All the layers used by the tile variants loaded
        /// </summary>
        public readonly HashSet<int> ActiveLayers = new HashSet<int>();

        /// <value>
        ///     A list of all the tile variants contained in this tile set, the variant's ID corresponds to its index in the list
        /// </value>
        public readonly ReadOnlyCollection<BasePart> TileVariants;

        /// <value>
        ///     Maps the index of a part in the TileVariants list to it's partID string
        /// </value>
        public readonly ReadOnlyDictionary<ushort, string> VariantIDToName;


        /// <value>
        ///     Maps the name partID string to the index of a part in the TileVariants List
        /// </value>
        public readonly ReadOnlyDictionary<string, BasePart> VariantNameToID;

        private TileSet()
        {
            var variants        = Resources.LoadAll<BasePart>("Tiles/Parts");
            var allVariants     = new List<BasePart>();
            var variantNameDict = new Dictionary<string, BasePart>();
            var idsDict         = new Dictionary<ushort, string>();

            foreach (BasePart variant in variants)
            {
                if (variantNameDict.ContainsKey(variant.partID))
                {
                    Debug.LogError("Found duplicate of " + variant.partID);
                    continue;
                }

                variant.id = (ushort) allVariants.Count;
                allVariants.Add(variant);
                ActiveLayers.Add(variant.layer);
                variantNameDict[variant.partID] = variant;
                idsDict[variant.id]             = variant.partID;
            }

            TileVariants    = new ReadOnlyCollection<BasePart>(allVariants);
            VariantNameToID = new ReadOnlyDictionary<string, BasePart>(variantNameDict);
            VariantIDToName = new ReadOnlyDictionary<ushort, string>(idsDict);
        }

        public static TileSet Instance => instance ?? (instance = new TileSet());
    }
}
