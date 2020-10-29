using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public enum TileStatus
        {
            Functional,
            Structural,
            Empty,
        }

        private static readonly ReadOnlyCollection<Vector3Int> ConnectionRules = new ReadOnlyCollection<Vector3Int>(
         new List<Vector3Int>
         {
             new Vector3Int(1,  0,  0),
             new Vector3Int(0,  1,  0),
             new Vector3Int(-1, 0,  0),
             new Vector3Int(0,  -1, 0),
         });

        [SerializeField] private string     tilePath;
        [SerializeField] private GameObject template;

        private readonly Dictionary<Vector3Int, FunctionalTileData> functionalTileData =
            new Dictionary<Vector3Int, FunctionalTileData>();

        private readonly Dictionary<Vector3Int, StructuralTileData> structuralTileData =
            new Dictionary<Vector3Int, StructuralTileData>();

        private Tilemap     functionalTilemap;
        private bool        physics;
        private Rigidbody2D rb2D;

        private Tilemap   structuralTilemap;
        private float     tileSize;
        public  TileSet   TileSet { get; private set; }
        private BoundsInt Bounds  => structuralTilemap.cellBounds;

        /// <value>
        ///     Will enable or disable physics for this tilemap (Splits tile map on enable)
        /// </value>
        public bool PhysicsEnabled
        {
            set
            {
                physics          = value;
                rb2D.isKinematic = !value;
                if (value) Split();
            }
            get => physics;
        }

        public void Awake()
        {
            var tileMaps = GetComponentsInChildren<Tilemap>();
            foreach (Tilemap map in tileMaps)
            {
                switch (map.gameObject.name)
                {
                    case "FuncTiles":
                        functionalTilemap = map;
                        break;
                    case "StructTiles":
                        structuralTilemap = map;
                        break;
                }
            }

            var grid = GetComponent<Grid>();
            tileSize         = grid.cellSize.x;
            TileSet          = TileSet.GetTileSet(tilePath);
            rb2D             = GetComponent<Rigidbody2D>();
            rb2D.isKinematic = true;
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

            int x = Mathf.FloorToInt(localPos.x / tileSize);
            int y = Mathf.FloorToInt(localPos.y / tileSize);
            return new Vector3Int(x, y, 0);
        }

        /// <summary>
        ///     Trys to get the structural tile at the given cords
        /// </summary>
        /// <param name="cords">The coordinates of the position to check</param>
        /// <param name="tileData">Sets to the value of the tile data (default if none)</param>
        /// <returns>True if there is a structural tile at cords, False otherwise</returns>
        public bool GetTile(Vector3Int cords, out StructuralTileData tileData)
        {
            if (TilesAt(cords) != TileStatus.Empty)
            {
                tileData = structuralTileData[cords];
                return true;
            }

            tileData = new StructuralTileData();
            return false;
        }

        /// <summary>
        ///     Trys to get the structural tile at a world position
        /// </summary>
        /// <param name="pos">The world position to check</param>
        /// <param name="tileData">Sets to the value of the tile data (default if none)</param>
        /// <returns>True if there is a structural tile at cords, False otherwise</returns>
        public bool GetTile(Vector3 pos, out StructuralTileData tileData)
        {
            Vector3Int cords = PositionToCords(pos);
            return GetTile(cords, out tileData);
        }

        /// <summary>
        ///     Trys to get the functional tile at the given cords
        /// </summary>
        /// <param name="cords">The coordinates of the position to check</param>
        /// <param name="tileData">Sets to the value of the tile data (default if none)</param>
        /// <returns>True if there is a functional tile at cords, False otherwise</returns>
        public bool GetTile(Vector3Int cords, out FunctionalTileData tileData)
        {
            if (TilesAt(cords) == TileStatus.Functional)
            {
                tileData = functionalTileData[cords];
                return true;
            }

            tileData = new FunctionalTileData();
            return false;
        }

        /// <summary>
        ///     Trys to get the functional tile at a world position
        /// </summary>
        /// <param name="pos">The world position to check</param>
        /// <param name="tileData">Sets to the value of the tile data (default if none)</param>
        /// <returns>True if there is a functional tile at cords, False otherwise</returns>
        public bool GetTile(Vector3 pos, out FunctionalTileData tileData)
        {
            Vector3Int cords = PositionToCords(pos);
            return GetTile(cords, out tileData);
        }

        /// <summary>
        ///     Set a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates of the tile to set</param>
        /// <param name="tileVariantID">The tile variant to use</param>
        public void SetTile(Vector3Int cords, ushort tileVariantID)
        {
            BaseTileVariant tileVariant = TileSet.TileVariants[tileVariantID];
            switch (tileVariant)
            {
                case FunctionalTileVariant functionalTile:
                    functionalTileData[cords] = new FunctionalTileData(functionalTile);
                    functionalTilemap.SetTile(cords, functionalTile.TileBase);
                    break;
                case StructuralTileVariant structuralTile:
                    structuralTileData[cords] = new StructuralTileData(structuralTile);
                    structuralTilemap.SetTile(cords, structuralTile.TileBase);
                    break;
            }
        }

        /// <summary>
        ///     Sets a tile at a position in world space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        /// <param name="structuralTiles">The tile variant to be placed</param>
        public void SetStructuralTiles(Vector3Int[] pos, StructuralTileVariant[] structuralTiles)
        {
            var newTiles = structuralTiles.Select(tile => tile.TileBase).ToArray();

            for (var i = 0; i < pos.Length; i++)
            {
                StructuralTileVariant tileVariant = structuralTiles[i];
                Vector3Int            cords       = pos[i];
                structuralTileData[cords] = new StructuralTileData(tileVariant);
            }

            structuralTilemap.SetTiles(pos, newTiles);
        }

        /// <summary>
        ///     Set an array of tiles at the given coordinates
        /// </summary>
        /// <param name="cords">The list of coordinates in the tilemap</param>
        /// <param name="functionalTile">The list of tile variants to be placed</param>
        public void SetFunctionalTiles(Vector3Int[] cords, FunctionalTileVariant[] functionalTile)
        {
            var newTiles = functionalTile.Select(tile => tile.TileBase).ToArray();

            for (var i = 0; i < cords.Length; i++)
            {
                FunctionalTileVariant tileVariant = functionalTile[i];
                Vector3Int            cord        = cords[i];
                functionalTileData[cord] = new FunctionalTileData(tileVariant);
            }

            functionalTilemap.SetTiles(cords, newTiles);
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
        /// <param name="structural">Only removes functional tile if false, removes structural and functional otherwise</param>
        public void RemoveTile(Vector3Int cords, bool structural = true)
        {
            if (structural && TilesAt(cords) == TileStatus.Structural)
            {
                structuralTileData.Remove(cords);
                structuralTilemap.SetTile(cords, null);
            }

            if (TilesAt(cords) == TileStatus.Functional)
            {
                functionalTilemap.SetTile(cords, null);
                functionalTileData.Remove(cords);
            }
        }

        /// <summary>
        ///     Remove the tile at a position in world space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        /// <param name="structural">Only removes functional tile if false, removes structural and functional otherwise</param>
        public void RemoveTile(Vector3 pos, bool structural = true)
        {
            RemoveTile(PositionToCords(pos), structural);
        }

        /// <summary>
        ///     Checks what tiles are present at a given coordinate
        /// </summary>
        /// <param name="cords">The coordinate to check</param>
        /// <returns>The type(s) of tiles present</returns>
        public TileStatus TilesAt(Vector3Int cords)
        {
            if (functionalTileData.ContainsKey(cords)) return TileStatus.Functional;

            return structuralTileData.ContainsKey(cords) ? TileStatus.Structural : TileStatus.Empty;
        }

        /// <summary>
        ///     Checks what tiles are present at a given position in world space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        /// <returns>The type(s) of tiles present</returns>
        public TileStatus TilesAt(Vector3 pos) => TilesAt(PositionToCords(pos));

        /// <summary>
        ///     Updates the internal tile data to match the tiles in Unity's tilemaps
        /// </summary>
        private void SyncFromTilemap()
        {
            BoundsInt cellBounds = structuralTilemap.cellBounds;

            for (int x = cellBounds.xMin; x <= cellBounds.xMax; x++)
            for (int y = cellBounds.yMin; y <= cellBounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (!structuralTilemap.HasTile(pos)) continue;

                TileBase tile = structuralTilemap.GetTile(pos);
                ushort   id   = TileSet.TilemapNameToID[tile.name];
                structuralTileData[pos] = new StructuralTileData(TileSet.TileVariants[id] as StructuralTileVariant);
                if (!functionalTilemap.HasTile(pos)) continue;

                tile                    = functionalTilemap.GetTile(pos);
                id                      = TileSet.TilemapNameToID[tile.name];
                functionalTileData[pos] = new FunctionalTileData(TileSet.TileVariants[id] as FunctionalTileVariant);
            }
        }

        /// <summary>
        ///     Applies damage to the tilemap
        /// </summary>
        /// <param name="dmg">A description of the damage to apply</param>
        public void ApplyDamage(Damage dmg)
        {
            if (!physics) return;

            Vector3Int start = PositionToCords(dmg.StartPos);
            Vector3Int end   = PositionToCords(dmg.EndPos);

            ApplyInLine(start, end, cords => DamageTile(cords, dmg.BaseDamage));
            Split();
        }

        /// <summary>
        ///     Damages a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates to damage the tile at</param>
        /// <param name="baseDamage">The base damage to use</param>
        private void DamageTile(Vector3Int cords, float baseDamage)
        {
            if (TilesAt(cords) == TileStatus.Empty) return;

            StructuralTileData tile            = structuralTileData[cords];
            var                tileVariantType = TileSet.TileVariants[tile.ID] as StructuralTileVariant;
            var                damage          = (ushort) (tileVariantType.DamageResistance * baseDamage);
            if (tile.Health <= damage)
            {
                RemoveTile(cords);
                if (structuralTileData.Count == 0) Destroy(transform.gameObject);

                return;
            }

            tile.Health               -= damage;
            structuralTileData[cords] =  tile;
        }

        /// <summary>
        ///     Clears all stored data and resets the tilemaps
        /// </summary>
        public void ResetTiles()
        {
            structuralTilemap.ClearAllTiles();
            functionalTilemap.ClearAllTiles();
            structuralTileData.Clear();
            functionalTileData.Clear();
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
        private List<List<Vector3Int>> FindIslands()
        {
            int width  = Bounds.xMax - Bounds.xMin;
            int height = Bounds.yMax - Bounds.yMin;

            var checkedCells = new bool[width, height];

            var islands = new List<List<Vector3Int>>();

            for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            {
                var cords = new Vector3Int(x, y, 0);
                if (!CheckTile(cords)) continue;

                var posToCheck = new Queue<Vector3Int>();
                posToCheck.Enqueue(cords);
                islands.Add(new List<Vector3Int>());
                var island = islands[islands.Count - 1];

                island.Add(new Vector3Int(x, y, 0));

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
                return TilesAt(cords) != TileStatus.Empty;
            }
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

                var structuralTiles = new List<StructuralTileVariant>();
                var functionalTiles = new List<FunctionalTileVariant>();
                var functionalCords = new List<Vector3Int>();

                foreach (Vector3Int cords in island)
                {
                    if (!GetTile(cords, out StructuralTileData stcTile)) continue;
                    structuralTiles.Add(TileSet.TileVariants[stcTile.ID] as StructuralTileVariant);

                    if (!GetTile(cords, out FunctionalTileData funTile)) continue;
                    functionalTiles.Add(TileSet.TileVariants[funTile.ID] as FunctionalTileVariant);
                    functionalCords.Add(cords);
                }

                newTileManager.SetFunctionalTiles(functionalCords.ToArray(), functionalTiles.ToArray());
                newTileManager.SetStructuralTiles(island.ToArray(), structuralTiles.ToArray());
                if (!physics) continue;

                newTileManager.PhysicsEnabled = true;
                newRb2D.velocity              = rb2D.velocity;
                newRb2D.angularVelocity       = rb2D.angularVelocity;
            }

            Destroy(gameObject);
        }
    }
}
