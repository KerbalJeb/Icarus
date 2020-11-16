using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    public delegate void Notify();

    /// <summary>
    ///     Used to manage and update Unity tile maps and store tile data
    /// </summary>
    /// <remarks>
    ///     Can store multiple layers of tilemaps, these are gotten from the children of the grid. The sorting order of the
    ///     tile renders must be sequential (ie. 1,2,3 is ok but 1,3,4 is not). Tiles are accessed using a Vector3Int, the X
    ///     and Y cords correspond to the X and Y position of the tile and Z is used for the layer.
    /// </remarks>
    [RequireComponent(typeof(Grid))]
    public class TileManager : MonoBehaviour
    {
        private static readonly ReadOnlyCollection<Vector3Int> ConnectionRules = new ReadOnlyCollection<Vector3Int>(
         new List<Vector3Int>
         {
             new Vector3Int(1,  0,  0),
             new Vector3Int(0,  1,  0),
             new Vector3Int(-1, 0,  0),
             new Vector3Int(0,  -1, 0),
         });

        [SerializeField] private GameObject template = null;

        private readonly Dictionary<Vector3Int, TileInstanceData> tileData =
            new Dictionary<Vector3Int, TileInstanceData>();

        private bool doneSplitting = false;

        private bool physics = false;

        private Tilemap[]   tilemapLayers;
        public  Rigidbody2D Rigidbody2D { get; private set; }

        public float TileSize { get; private set; }

        public  TileSet   TileSet { get; private set; }
        private BoundsInt Bounds  => tilemapLayers[0].cellBounds;

        public bool PhysicsEnabled
        {
            set
            {
                physics                 = value;
                Rigidbody2D.isKinematic = !value;
            }
            get => physics;
        }

        public void Awake()
        {
            var tilemaps       = GetComponentsInChildren<Tilemap>();
            var tilemapRenders = GetComponentsInChildren<TilemapRenderer>();
            var pairs = (from map in tilemaps
                         from tilemapRenderer in tilemapRenders
                         where map.gameObject == tilemapRenderer.gameObject
                         select (map, tilemapRenderer)).ToList();

            tilemapLayers = new Tilemap[pairs.Count];
            foreach ((Tilemap map, TilemapRenderer tilemapRenderer) in pairs)
                tilemapLayers[tilemapRenderer.sortingOrder] = map;
            var grid = GetComponent<Grid>();
            TileSize    = grid.cellSize.x;
            TileSet     = TileSet.Instance;
            Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        /// <value>
        /// Called everytime the physics model is updated (tile is destroyed)
        /// </value>
        public event Notify UpdatePhysics;

        /// <summary>
        ///     Converts a world position to coordinates in the tilemap
        /// </summary>
        /// <param name="vec">The world position vector</param>
        /// <returns>The coordinate position in the tilemap</returns>
        public Vector3Int PositionToCords(Vector3 vec)
        {
            Quaternion invRotation = Quaternion.Inverse(transform.rotation);
            Vector3    objectPos   = transform.position;
            Vector3    localPos    = invRotation * (vec - objectPos);

            int x = Mathf.FloorToInt(localPos.x / TileSize);
            int y = Mathf.FloorToInt(localPos.y / TileSize);
            return new Vector3Int(x, y, 0);
        }

        public Vector2 CordsToPosition(Vector3Int cords) =>
            new Vector2((cords.x + 0.5f) * TileSize, (cords.y + 0.5f) * TileSize);

        /// <summary>
        ///     Trys to get the tile at the given cords
        /// </summary>
        /// <param name="cords">The coordinates of the position to check</param>
        /// <param name="foundTile">True if there is a structural tile at cords, False otherwise</param>
        /// <returns>Returns to the value of the tile data (default if none)</returns>
        public TileInstanceData GetTile(Vector3Int cords, out bool foundTile)
        {
            if (HasTile(cords))
            {
                foundTile = true;
                return tileData[cords];
            }

            foundTile = false;
            return new TileInstanceData();
        }

        public bool GetTile(Vector3Int cords, ref TileInstanceData instanceData)
        {
            if (!HasTile(cords)) return false;
            instanceData = tileData[cords];
            return true;
        }

        /// <summary>
        ///     Set a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates of the tile to set</param>
        /// <param name="tileVariant">The tile variant to use</param>
        /// <param name="direction">The orientation of the tile</param>
        public void SetTile(Vector3Int cords, BasePart tileVariant, Directions direction=Directions.Up)
        {
            cords.z = tileVariant.layer;
            if (tileData.ContainsKey(cords) && tileData[cords].ID == tileVariant.id)
            {
                return;
            }
            tileVariant.Instantiate(cords, tilemapLayers[tileVariant.layer], direction);
            tileData[cords] = new TileInstanceData(tileVariant);
        }

        /// <summary>
        ///     Set an array of tiles at the given coordinates
        /// </summary>
        /// <param name="cords">The list of coordinates in the tilemap</param>
        /// <param name="tiles">The list of tile variants to be placed</param>
        private void CopyTileInstancesData(Vector3Int[] cords, TileInstanceData[] tiles)
        {
            if (cords.Length < 1) return;
            Debug.Assert(cords.All(i => i.z == cords[0].z));
            
            int     layerID  = cords[0].z;
            var     newTiles = tiles.Select(tile => TileSet.TileVariants[tile.ID].tile).ToArray();
            Tilemap tilemap  = tilemapLayers[layerID];
            tilemap.SetTiles(cords, newTiles);

            for (var i = 0; i < cords.Length; i++)
            {
                Vector3Int       cord = cords[i];
                TileInstanceData data = tiles[i];
                tileData[cord] = data;
                if (data.Rotation != Directions.Up)
                    tilemap.SetTransformMatrix(cord, TileInfo.TransformMatrix[data.Rotation]);
            }
        }

        /// <summary>
        ///     Sets a tile at the given position in world space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        /// <param name="tileVariant">The tile variant to be placed</param>
        public void SetTile(Vector3 pos, BasePart tileVariant)
        {
            SetTile(PositionToCords(pos), tileVariant);
        }

        /// <summary>
        ///     Remove the tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates to remove the tile at</param>
        /// <param name="allTiles">Deletes all tiles at xy cords if true (default)</param>
        public void RemoveTile(Vector3Int cords, bool allTiles = true)
        {
            if (allTiles)
            {
                for (var i = 0; i < tilemapLayers.Length; i++)
                {
                    cords.z = i;
                    if (!tileData.ContainsKey(cords))
                    {
                        continue;
                    }
                    TileSet.TileVariants[tileData[cords].ID].Remove(cords, tilemapLayers[i]);
                    tileData.Remove(cords);
                }
            }
            else
            {
                TileSet.TileVariants[tileData[cords].ID].Remove(cords, tilemapLayers[cords.z]);
                tileData.Remove(cords);
            }
        }

        /// <summary>
        ///     Remove the tile at a position in world space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        public void RemoveTile(Vector3 pos)
        {
            RemoveTile(PositionToCords(pos));
        }

        /// <summary>
        ///     Checks what tiles are present at a given coordinate
        /// </summary>
        /// <param name="cords">The coordinate to check</param>
        /// <returns>The type(s) of tiles present</returns>
        public bool HasTile(Vector3Int cords) => tileData.ContainsKey(cords);

        /// <summary>
        ///     Checks what tiles are present at a given position in world space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        /// <returns>The type(s) of tiles present</returns>
        public bool HasTile(Vector3 pos) => HasTile(PositionToCords(pos));

        /// <summary>
        ///     Get the rotation of a tile at the given cords
        /// </summary>
        /// <param name="tilemap">The tilemap to use</param>
        /// <param name="cords">The XYZ Coordinates of the tile</param>
        /// <returns>The direction the tile is facing</returns>
        private static Directions GetTileRotation(Tilemap tilemap, Vector3Int cords)
        {
            float rot = tilemap.GetTransformMatrix(cords).rotation.eulerAngles.z;
            switch (rot)
            {
                case 90f:
                    return Directions.Left;
                case 180f:
                    return Directions.Down;
                case 270f:
                    return Directions.Right;
                default:
                    return Directions.Up;
            }
        }

        /// <summary>
        ///     Damages a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates to damage the tile at</param>
        /// <param name="baseDamage">The base damage to use</param>
        /// <param name="damageUsed">How much of the base damage was used to destroy the tile</param>
        public void DamageTile(Vector3Int cords, float baseDamage, out float damageUsed)
        {
            if (!HasTile(cords))
            {
                damageUsed = 0;
                return;
            }

            TileInstanceData tile     = tileData[cords];
            BasePart         partType = TileSet.TileVariants[tile.ID];
            var              damage   = (ushort) (partType.damageResistance * baseDamage);
            if (tile.Health <= damage)
            {
                RemoveTile(cords);
                if (tileData.Count == 0) Destroy(transform.gameObject);
                damageUsed = tile.Health / partType.damageResistance;
                return;
            }

            tile.Health     -= damage;
            tileData[cords] =  tile;
            damageUsed      =  baseDamage;
        }

        /// <summary>
        ///     Clears all stored data and resets the tilemaps
        /// </summary>
        public void ResetTiles()
        {
            foreach (Tilemap tilemap in tilemapLayers)
            {
                tilemap.ClearAllTiles();
                foreach (Transform child in tilemap.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            tileData.Clear();
        }

        /// <summary>
        ///     Finds Islands of unconnected tiles
        /// </summary>
        /// <returns>A list of the islands, each containing a list of the coordinate positions that make up the island</returns>
        public List<List<Vector3Int>> FindIslands(int layer = 0)
        {
            int width  = Bounds.xMax - Bounds.xMin;
            int height = Bounds.yMax - Bounds.yMin;

            var checkedCells = new bool[width, height];

            var islands = new List<List<Vector3Int>>();

            for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            {
                var cords = new Vector3Int(x, y, layer);
                if (!CheckTile(cords)) continue;

                var posToCheck = new Queue<Vector3Int>();
                posToCheck.Enqueue(cords);
                islands.Add(new List<Vector3Int>());
                var island = islands[islands.Count - 1];

                island.Add(new Vector3Int(x, y, layer));

                while (posToCheck.Count > 0)
                {
                    Vector3Int cordToCheck = posToCheck.Dequeue();

                    foreach (Vector3Int offset in ConnectionRules)
                    {
                        Vector3Int neighbor = cordToCheck + offset;
                        if (!CheckTile(neighbor)) continue;

                        island.Add(neighbor);
                        posToCheck.Enqueue(neighbor);
                    }
                }
            }

            return islands;

            bool CheckTile(Vector3Int cords)
            {
                int xIdx = cords.x - Bounds.xMin;
                int yIdx = cords.y - Bounds.yMin;
                if (!Bounds.Contains(cords) || checkedCells[xIdx, yIdx]) return false;

                checkedCells[xIdx, yIdx] = true;
                return HasTile(cords);
            }
        }

        /// <summary>
        ///     Gets all the tiles of a particular variant type
        /// </summary>
        /// <param name="tiles">A list of all the tiles of that variant type</param>
        /// <typeparam name="T">The variant type (Must inherit from BaseTileVariant or will throw null reference)</typeparam>
        public void GetTilesByVariant<T>(out List<(Vector3Int cords, TileInstanceData data)> tiles)
        {
            var ids = new HashSet<ushort>(TileSet.TileVariants.OfType<T>().Select(x => (x as BasePart).id));
            tiles = tileData.Where(x => ids.Contains(x.Value.ID)).Select(x => (x.Key, x.Value)).ToList();
        }

        /// <summary>
        ///     Splits the tilemap into multiple objects if there are unconnected regions
        /// </summary>
        private void Split()
        {
            if (doneSplitting) return;
            var islands = FindIslands();
            if (islands.Count <= 1) return;

            foreach (var island in islands)
            {
                GameObject obj            = Instantiate(template);
                var        newTileManager = obj.GetComponent<TileManager>();
                var        newRb2D        = obj.GetComponent<Rigidbody2D>();

                newTileManager.ResetTiles();

                Transform thisTransform = transform;
                Transform tileTransform = newTileManager.transform;

                tileTransform.position = thisTransform.position;
                tileTransform.rotation = thisTransform.rotation;

                var tiles        = new List<TileInstanceData>();
                var cordsList    = new List<Vector3Int>();
                var instanceData = new TileInstanceData();

                foreach (int i in TileSet.ActiveLayers)
                {
                    tiles.Clear();
                    cordsList.Clear();
                    foreach (Vector3Int cords in island)
                    {
                        Vector3Int c = cords;
                        c.z = i;
                        if (!GetTile(c, ref instanceData)) continue;
                        tiles.Add(instanceData);
                        cordsList.Add(c);
                    }

                    newTileManager.CopyTileInstancesData(cordsList.ToArray(), tiles.ToArray());
                }

                if (!PhysicsEnabled) continue;

                newTileManager.physics  = true;
                newRb2D.isKinematic     = false;
                newRb2D.velocity        = Rigidbody2D.velocity;
                newRb2D.angularVelocity = Rigidbody2D.angularVelocity;
            }

            doneSplitting = true;
            Destroy(gameObject);
        }

        /// <summary>
        /// Splits the mesh if physics is enabled and invokes the UpdatePhysics event
        /// </summary>
        public void RefreshPhysics()
        {
            if (!physics) return;
            UpdatePhysics?.Invoke();
            Split();
        }

        /// <summary>
        /// Will serialize the design data (only tile ID and rot, no HP)
        /// </summary>
        /// <returns>The JSON string</returns>
        public string DesignToJson()
        {
            var data = tileData.Select(instanceData => new SerializableTileData
            {
                pos = instanceData.Key, 
                tileID = TileSet.VariantIDToName[instanceData.Value.ID],
                dir = instanceData.Value.Rotation,
            }).ToList();
            return JsonUtility.ToJson(new SerializableGridData {grid = data}, true);
        }

        public void LoadFromJson(string path)
        {
            var jsonText = File.ReadAllText(path);
            var grid     = JsonUtility.FromJson<SerializableGridData>(jsonText).grid;
            ResetTiles();
            foreach (SerializableTileData data in grid)
            {
                var basePart = TileSet.Instance.VariantNameToID[data.tileID];
                SetTile(data.pos, basePart, data.dir);
            }
        }

        [Serializable]
        private class SerializableTileData
        {
            public string     tileID;
            public Directions dir;
            public Vector3Int pos;
        }

        [Serializable]
        private class SerializableGridData
        {
            public List<SerializableTileData> grid;
        }
    }
}
