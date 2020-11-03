using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using TileSystem.TileVariants;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    /// <summary>
    ///     Used to manage and update Unity tile maps and store tile data
    /// </summary>
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

        // ReSharper disable once RedundantDefaultMemberInitializer
        [SerializeField] private string     tilePath = null;
        [SerializeField] private GameObject template =null;

        private                  Rigidbody2D rb2D;


        private readonly Dictionary<Vector3Int, TileInstanceData> tileData =
            new Dictionary<Vector3Int, TileInstanceData>();

        private Tilemap[] tilemapLayers;

        public float TileSize { get; private set; }

        public  TileSet   TileSet { get; private set; }
        private BoundsInt Bounds  => tilemapLayers[0].cellBounds;
        private bool      physics = false;
        public bool PhysicsEnabled
        {
            set
            {
                physics          = value;
                rb2D.isKinematic = !value;
                if (!value) return;
                Split();           
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
            {
                tilemapLayers[tilemapRenderer.sortingOrder] = map;
            }
            var grid = GetComponent<Grid>();
            TileSize = grid.cellSize.x;
            TileSet  = TileSet.GetTileSet(tilePath);
            rb2D     = GetComponent<Rigidbody2D>();
        }

        public void Start()
        {
            SyncFromTilemap();
        }

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

        public Vector2 CordsToPosition(Vector3Int cords)
        {
            return new Vector2((cords.x+0.5f) * TileSize, (cords.y+0.5f) * TileSize);
        }

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
        ///     Trys to get the tile at a world position
        /// </summary>
        /// <param name="pos">The world position to check</param>
        /// <param name="foundTile">True if there is a functional tile at cords, False otherwise</param>
        /// <returns>Returns the value of the tile data (default if none)</returns>
        public TileInstanceData GetTile(Vector3 pos, out bool foundTile)
        {
            Vector3Int cords = PositionToCords(pos);
            return GetTile(cords, out foundTile);
        }

        /// <summary>
        ///     Set a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates of the tile to set</param>
        /// <param name="tileVariantID">The tile variant to use</param>
        public void SetTile(Vector3Int cords, ushort tileVariantID)
        {
            BaseTileVariant tileVariant = TileSet.TileVariants[tileVariantID];
            tilemapLayers[tileVariant.Layer].SetTile(cords, tileVariant.TileBase);
            cords.z         = tileVariant.Layer;
            tileData[cords] = new TileInstanceData(tileVariant);
        }

        /// <summary>
        ///     Set an array of tiles at the given coordinates
        /// </summary>
        /// <param name="cords">The list of coordinates in the tilemap</param>
        /// <param name="tiles">The list of tile variants to be placed</param>
        public void SetTiles(Vector3Int[] cords, TileInstanceData[] tiles)
        {
            Debug.Assert(cords.All(i=>i.z==cords[0].z));
            int layerID  = cords[0].z;
            var newTiles = tiles.Select(tile => TileSet.TileVariants[tile.ID].TileBase).ToArray();
            var tilemap  = tilemapLayers[layerID];
            tilemap.SetTiles(cords, newTiles);

            for (var i = 0; i < cords.Length; i++)
            {
                Vector3Int cord     = cords[i];
                var        data = tiles[i];
                tileData[cord] = data;
                if (data.Rotation != Directions.Up)
                {
                    tilemap.SetTransformMatrix(cord, TileInfo.TransformMatrix[data.Rotation]);
                }
            }

        }

        /// <summary>
        ///     Sets a tile at the given position in world space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        /// <param name="tileVariantID">The tile variant to be placed</param>
        public void SetTile(Vector3 pos, ushort tileVariantID)
        {
            SetTile(PositionToCords(pos), tileVariantID);
        }

        /// <summary>
        ///     Remove the tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates to remove the tile at</param>
        /// <param name="allTiles">Deletes all tiles at xy cords if true (default)</param>
        public void RemoveTile(Vector3Int cords, bool allTiles=true)
        {
            if (allTiles)
            {
                for (int i = 0; i < tilemapLayers.Length; i++)
                {
                    cords.z = i;
                    tilemapLayers[i].SetTile(cords, null);
                    tileData.Remove(cords);
                }
            }
            tileData.Remove(cords);
            tilemapLayers[cords.z].SetTile(cords, null);

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
        public bool HasTile(Vector3Int cords)
        {
            return tileData.ContainsKey(cords);
        }

        /// <summary>
        ///     Checks what tiles are present at a given position in world space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        /// <returns>The type(s) of tiles present</returns>
        public bool HasTile(Vector3 pos) => HasTile(PositionToCords(pos));

        /// <summary>
        ///     Updates the internal tile data to match the tiles in Unity's tilemaps
        /// </summary>
        private void SyncFromTilemap()
        {
            for(int i=0;i<tilemapLayers.Length;i++)
            {
                var tilemapLayer = tilemapLayers[i];
                foreach (Vector3Int cords in tilemapLayer.cellBounds.allPositionsWithin)
                {
                    if (!tilemapLayer.HasTile(cords))
                    {
                        continue;
                    }

                    var c2 = cords;
                    c2.z = i;
                    TileBase tile = tilemapLayer.GetTile(cords);
                    var      rot  = GetTileRotation(tilemapLayer, cords);

                    tilemapLayer.SetTile(c2, tile);
                    ushort id  = TileSet.TilemapNameToID[tile.name];
                    tileData[c2] = new TileInstanceData(TileSet.TileVariants[id], rot);
                    if (rot != Directions.Up)
                    {
                        tilemapLayer.SetTransformMatrix(c2, TileInfo.TransformMatrix[rot]);
                    }
                }
            }

        }

        private Directions GetTileRotation(Tilemap tilemap, Vector3Int pos)
        {
            var rot = tilemap.GetTransformMatrix(pos).rotation.eulerAngles.z;
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
        ///     Applies damage to the tilemap
        /// </summary>
        /// <param name="dmg">A description of the damage to apply</param>
        public bool ApplyDamage(Damage dmg)
        {
            Vector3Int start = PositionToCords(dmg.StartPos);
            Vector3Int end   = PositionToCords(dmg.EndPos);
            bool        destroyedTile =false;

            ApplyInLine(start, end, cords => DamageTile(cords, dmg.BaseDamage, ref destroyedTile));
            if (destroyedTile)
            {
                Split();
            }
            return destroyedTile;
        }

        /// <summary>
        ///     Damages a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates to damage the tile at</param>
        /// <param name="baseDamage">The base damage to use</param>
        /// <param name="destroyed">Will be set to true if a tile is destroyed (unchanged otherwise)</param>
        private void DamageTile(Vector3Int cords, float baseDamage, ref bool destroyed)
        {
            if (!HasTile(cords)) return;

            TileInstanceData tile            = tileData[cords];
            var      tileVariantType = TileSet.TileVariants[tile.ID];
            var      damage          = (ushort) (tileVariantType.DamageResistance * baseDamage);
            if (tile.Health <= damage)
            {
                RemoveTile(cords);
                if (tileData.Count == 0) Destroy(transform.gameObject);
                destroyed = true;
                return;
            }

            tile.Health     -= damage;
            tileData[cords] =  tile;
        }

        /// <summary>
        ///     Clears all stored data and resets the tilemaps
        /// </summary>
        public void ResetTiles()
        {
            foreach (Tilemap tilemap in tilemapLayers)
            {
                tilemap.ClearAllTiles();
            }
            tileData.Clear();
        }

        /// <summary>
        ///     Used to apply an action to every tile in a line between two points
        /// </summary>
        /// <param name="p0">The starting point</param>
        /// <param name="p1">The end point</param>
        /// <param name="callback">A delegate action that takes a Vector3Int that will be calle for every point on the line</param>
        private static void ApplyInLine(Vector3Int p0, Vector3Int p1, Action<Vector3Int> callback)
        {
            // Bresenham's line algorithm (https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm#All_cases)
            int dx  = Math.Abs(p1.x - p0.x);
            int sx  = p0.x < p1.x ? 1 : -1;
            int dy  = -Math.Abs(p1.y - p0.y);
            int sy  = p0.y < p1.y ? 1 : -1;
            int err = dx + dy;

            int x = p0.x;
            int y = p0.y;

            while (true)
            {
                callback(new Vector3Int(x, y, 0));
                if (x == p1.x && y == p1.y) break;

                int e2 = 2 * err;

                if (e2 >= dy)
                {
                    err += dy;
                    x   += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y   += sy;
                }
            }
        }

        /// <summary>
        ///     Finds Islands of unconnected tiles
        /// </summary>
        /// <returns>A list of the islands, each containing a list of the coordinate positions that make up the island</returns>
        public List<List<Vector3Int>> FindIslands(int layer=0)
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
        /// Gets all the tiles of a particular variant type
        /// </summary>
        /// <param name="tiles">A list of all the tiles of that variant type</param>
        /// <typeparam name="T">The variant type (Must inherit from BaseTileVariant or will throw null reference)</typeparam>
        public void GetTilesByVariant<T>(out List<(Vector3Int cords, TileInstanceData data)> tiles)
        {
            var ids = new HashSet<ushort>(TileSet.TileVariants.OfType<T>().Select(x => (x as BaseTileVariant).ID));
            tiles = tileData.Where(x => ids.Contains(x.Value.ID)).Select(x => (x.Key, x.Value)).ToList();
        }
        
        /// <summary>
        ///     Splits the tilemap into multiple objects if there are unconnected reagons
        /// </summary>
        private void Split()
        {
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
                    newTileManager.SetTiles(cordsList.ToArray(), tiles.ToArray());
                }

                if (!PhysicsEnabled) continue;

                newTileManager.PhysicsEnabled = true;
                newRb2D.velocity              = rb2D.velocity;
                newRb2D.angularVelocity       = rb2D.angularVelocity;
            }
            Destroy(gameObject);
        }
    }
}
