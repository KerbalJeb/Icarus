using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    /// <summary>
    ///     Used to manage and update Unity tile maps and store tile data
    /// </summary>
    /// <remarks>
    ///     Can store multiple layers of tilemaps, these are gotten from the children of the grid. The sorting order of the
    ///     tile renders must be sequential (ie. 1,2,3 is ok but 1,3,4 is not). Tiles are accessed using a Vector3Int, the X
    ///     and Y cords correspond to the X and Y position of the tile and Z is used for the layer.
    /// </remarks>
    public class TileManager : MonoBehaviour
    {
        // public        GameObject     comSprite;
        private const int minIslandSize = 3;

        private static readonly ReadOnlyCollection<Vector3Int> ConnectionRules = new ReadOnlyCollection<Vector3Int>(
         new List<Vector3Int>
         {
             new Vector3Int(1,  0,  0),
             new Vector3Int(0,  1,  0),
             new Vector3Int(-1, 0,  0),
             new Vector3Int(0,  -1, 0),
         });

        public ObjectPool pool;

        [SerializeField] private bool physics;
        public                   bool onTileMap;

        private readonly Dictionary<Vector3Int, TileInstanceData> tileData =
            new Dictionary<Vector3Int, TileInstanceData>();

        private Vector2 com;
        private bool    comUpdated;

        private Grid  grid;
        private float mass;

        private Tilemap[]   tilemapLayers;
        public  Rigidbody2D Rigidbody2D { get; private set; }

        public float TileSize => grid.cellSize.x;

        public  TileSet   TileSet             { get; private set; }
        private BoundsInt Bounds              => tilemapLayers[0].cellBounds;
        public  bool      PhysicsModelChanged { get; set; }

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
            TileSet     = TileSet.Instance;
            Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (PhysicsModelChanged)
            {
                RefreshPhysics();
                PhysicsModelChanged = false;
            }

            if (comUpdated)
            {
                Rigidbody2D.mass         = mass;
                Rigidbody2D.centerOfMass = com;
                comUpdated               = false;
            }
        }

        public void OnEnable()
        {
            if (onTileMap)
            {
                var tileMap = GetComponent<Tilemap>();
                tilemapLayers = new[] {tileMap};
            }
            else
            {
                var tilemaps       = GetComponentsInChildren<Tilemap>();
                var tilemapRenders = GetComponentsInChildren<TilemapRenderer>();
                var pairs = (from map in tilemaps
                             from tilemapRenderer in tilemapRenders
                             where map.gameObject == tilemapRenderer.gameObject
                             select (map, tilemapRenderer)).ToList();

                if (pairs.Count <= 0)
                {
                    gameObject.SetActive(false);
                    return;
                }

                tilemapLayers = new Tilemap[pairs.Count];
                foreach ((Tilemap map, TilemapRenderer tilemapRenderer) in pairs)
                    tilemapLayers[tilemapRenderer.sortingOrder] = map;
            }

            grid = tilemapLayers[0].layoutGrid;
            if (grid == null) gameObject.SetActive(false);
        }

        /// <value>
        ///     Called everytime the physics model is updated (tile is destroyed)
        /// </value>
        public event Action UpdatePhysics;

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
        public void SetTile(Vector3Int cords, BasePart tileVariant, Direction direction = Direction.Up)
        {
            if (tileVariant is null)
            {
                RemoveTile(cords);
                return;
            }

            cords.z = tileVariant.layer;
            if (tileData.ContainsKey(cords) && tileData[cords].ID == tileVariant.id) return;
            tileVariant.Instantiate(cords, tilemapLayers[tileVariant.layer], direction);
            tileData[cords] = new TileInstanceData(tileVariant, direction);
            Vector2 localPos = CordsToPosition(cords);
            UpdateCom(localPos, tileVariant.mass, true);
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
            Vector2 localPos = CordsToPosition(cords);
            if (allTiles)
            {
                for (var i = 0; i < tilemapLayers.Length; i++)
                {
                    cords.z = i;
                    if (!tileData.ContainsKey(cords)) continue;
                    BasePart variant = TileSet.TileVariants[tileData[cords].ID];
                    UpdateCom(localPos, variant.mass, false);
                    variant.Remove(cords, tilemapLayers[i]);
                    tileData.Remove(cords);
                }
            }
            else
            {
                BasePart variant = TileSet.TileVariants[tileData[cords].ID];
                UpdateCom(localPos, variant.mass, false);
                variant.Remove(cords, tilemapLayers[cords.z]);
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
        private static Direction GetTileRotation(Tilemap tilemap, Vector3Int cords)
        {
            float rot = tilemap.GetTransformMatrix(cords).rotation.eulerAngles.z;
            switch (rot)
            {
                case 90f:
                    return Direction.Left;
                case 180f:
                    return Direction.Down;
                case 270f:
                    return Direction.Right;
                default:
                    return Direction.Up;
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
                if (tileData.Count == 0) transform.gameObject.SetActive(false);
                damageUsed          = tile.Health / partType.damageResistance;
                PhysicsModelChanged = true;
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
                foreach (Transform child in tilemap.transform) Destroy(child.gameObject);
            }

            tileData.Clear();
            mass       = 0;
            com        = Vector2.zero;
            comUpdated = true;
        }

        /// <summary>
        ///     Finds Islands of unconnected tiles
        /// </summary>
        /// <returns>A list of the islands, each containing a list of the coordinate positions that make up the island</returns>
        public List<List<Vector3Int>> FindIslands(int layer = 0)
        {
            BoundsInt cachedBounds = Bounds;

            int xMax = cachedBounds.xMax + 1;
            int yMax = cachedBounds.yMax + 1;
            int xMin = cachedBounds.xMin - 1;
            int yMin = cachedBounds.yMin - 1;

            int width  = xMax - xMin;
            int height = yMax - yMin;

            var checkedCells = new bool[width * height];

            var islands = new List<List<Vector3Int>>();

            for (int x = xMin; x < xMax; x++)
            for (int y = yMin; y < yMax; y++)
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
                int x    = cords.x;
                int y    = cords.y;
                int xIdx = x - xMin;
                int yIdx = y - yMin;

                int idx = xIdx + yIdx * width;


                if (checkedCells[idx] || x > xMax || x < xMin || y > yMax || y < yMin) return false;

                checkedCells[idx] = true;
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
            var islands = FindIslands();
            if (islands.Count <= 1) return;

            int biggest     = islands.Max(i => i.Count);
            var updatedThis = false;
            tilemapLayers[0].color = Color.white;

            foreach (var island in islands)
            {
                if (island.Count == biggest && !updatedThis)
                    updatedThis = true;
                else
                {
                    if (island.Count <= minIslandSize)
                    {
                        foreach (Vector3Int cord in island)
                            RemoveTile(cord);
                    }

                    Transform  gridTransform = grid.transform;
                    GameObject obj           = pool.GetObject();
                    obj.transform.parent = gridTransform;

                    var         newTileManager = obj.GetComponent<TileManager>();
                    Rigidbody2D newRb2D        = newTileManager.Rigidbody2D;
                    newTileManager.pool = pool;
                    newTileManager.grid = grid;
                    newTileManager.ResetTiles();

                    Transform thisTransform = transform;
                    Transform tileTransform = newTileManager.transform;

                    tileTransform.position = thisTransform.position;
                    tileTransform.rotation = thisTransform.rotation;

                    var instanceData = new TileInstanceData();
                    obj.SetActive(true);

                    var tileGroups =
                        new List<(List<Vector3Int> cords, List<Direction> dirs, List<TileInstanceData> data)>();

                    for (var i = 0; i < TileSet.TileVariants.Count; i++)
                        tileGroups.Add((new List<Vector3Int>(), new List<Direction>(), new List<TileInstanceData>()));

                    foreach (Vector3Int cords in island)
                    {
                        foreach (int i in TileSet.ActiveLayers)
                        {
                            Vector3Int c = cords;
                            c.z = i;
                            if (!GetTile(c, ref instanceData)) continue;
                            tileGroups[instanceData.ID].cords.Add(c);
                            tileGroups[instanceData.ID].dirs.Add(instanceData.Rotation);
                            tileGroups[instanceData.ID].data.Add(instanceData);
                        }
                    }

                    for (var i = 1; i < TileSet.TileVariants.Count; i++)
                    {
                        BasePart variant = TileSet.TileVariants[i];
                        if (tileGroups[i].cords.Count <= 0) continue;
                        variant.SetTiles(tileGroups[i].cords.ToArray(), newTileManager.tilemapLayers[variant.layer],
                                         tileGroups[i].dirs.ToArray());
                        variant.RemoveTiles(tileGroups[i].cords.ToArray(), tilemapLayers[variant.layer]);

                        for (var j = 0; j < tileGroups[i].cords.Count; j++)
                        {
                            Vector3Int cord = tileGroups[i].cords[j];
                            newTileManager.tileData[cord] = tileGroups[i].data[j];
                            tileData.Remove(cord);
                        }
                    }

                    newTileManager.RecalculateCom();

                    if (!PhysicsEnabled) continue;
                    newTileManager.physics  = true;
                    newRb2D.isKinematic     = false;
                    newRb2D.angularVelocity = Rigidbody2D.angularVelocity;
                }
            }

            RecalculateCom();
        }

        /// <summary>
        ///     Splits the mesh if physics is enabled and invokes the UpdatePhysics event
        /// </summary>
        private void RefreshPhysics()
        {
            if (!physics) return;
            UpdatePhysics?.Invoke();
            Split();
        }

        private void UpdateCom(Vector2 localPos, float tileMass, bool adding)
        {
            if (adding)
            {
                com  =  (mass * com + localPos * tileMass) / (tileMass + mass);
                mass += tileMass;
            }
            else
            {
                if (mass - tileMass < Mathf.Epsilon) return;
                com  -= tileMass * localPos / mass;
                mass -= tileMass;
                com  *= (mass + tileMass) / mass;
            }

            // comSprite.transform.position = com;
            comUpdated = true;
        }

        private void RecalculateCom()
        {
            mass = 0;
            com  = Vector2.zero;
            foreach (var data in tileData)
            {
                Vector2 pos      = CordsToPosition(data.Key);
                float   tileMass = TileSet.TileVariants[data.Value.ID].mass;
                com  =  (mass * com + tileMass * pos) / (mass + tileMass);
                mass += tileMass;
            }

            comUpdated = true;
        }


        /// <summary>
        ///     Will serialize the design data (only tile ID and rot, no HP)
        /// </summary>
        /// <returns>The JSON string</returns>
        public string DesignToJson()
        {
            var data = tileData.Select(instanceData => new SerializableTileData
            {
                pos    = instanceData.Key,
                tileID = TileSet.VariantIDToName[instanceData.Value.ID],
                dir    = instanceData.Value.Rotation,
            }).ToList();
            return JsonUtility.ToJson(new SerializableGridData {grid = data}, true);
        }

        public void LoadFromJson(string path)
        {
            string jsonText = File.ReadAllText(path);
            var    grid     = JsonUtility.FromJson<SerializableGridData>(jsonText).grid;
            ResetTiles();
            foreach (SerializableTileData data in grid)
            {
                BasePart basePart = TileSet.Instance.VariantNameToID[data.tileID];
                SetTile(data.pos, basePart, data.dir);
            }

            PhysicsModelChanged = true;
        }

        [Serializable]
        private class SerializableTileData
        {
            public string     tileID;
            public Direction  dir;
            public Vector3Int pos;
        }

        [Serializable]
        private class SerializableGridData
        {
            public List<SerializableTileData> grid;
        }
    }
}
