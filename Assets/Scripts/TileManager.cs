using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TileClasses;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    public enum TileStatus
    {
        Functional,
        Structural,
        Empty
    }

    private static readonly ReadOnlyCollection<Vector3Int> ConnectionRules = new ReadOnlyCollection<Vector3Int>(
        new List<Vector3Int>
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, -1, 0)
        });

    [SerializeField] private string tilePath = null;
    [SerializeField] private GameObject template = null;

    private readonly Dictionary<Vector3Int, FunctionalTileData> functionalTileData =
        new Dictionary<Vector3Int, FunctionalTileData>();

    private readonly Dictionary<Vector3Int, StructuralTileData> structuralTileData =
        new Dictionary<Vector3Int, StructuralTileData>();

    private Tilemap functionalTilemap;
    private bool physics;
    private Rigidbody2D rb2D;

    private Tilemap structuralTilemap;
    private float tileSize;
    public TileSet TileSet { get; private set; }
    private BoundsInt Bounds => structuralTilemap.cellBounds;

    public bool PhysicsEnabled
    {
        set
        {
            physics = value;
            rb2D.isKinematic = !value;
            if (value)
            {
                Split();
            }
        }
        get => physics;
    }

    public void Awake()
    {
        var tileMaps = GetComponentsInChildren<Tilemap>();
        foreach (var map in tileMaps)
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
        tileSize = grid.cellSize.x;
        TileSet = TileSet.GetTileSet(tilePath);
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.isKinematic = true;
    }

    public void Start()
    {
        SyncFromTilemap();
    }

    public Vector3Int PositionToCords(Vector3 vec)
    {
        var invRotation = Quaternion.Inverse(transform.rotation);
        var objectPos = transform.position;
        var localPos = invRotation * (vec - objectPos);

        var x = Mathf.FloorToInt(localPos.x / tileSize);
        var y = Mathf.FloorToInt(localPos.y / tileSize);
        return new Vector3Int(x, y, 0);
    }

    public StructuralTileData GetStructuralTile(Vector3Int cords)
    {
        return structuralTileData[cords];
    }

    public StructuralTileData GetStructuralTile(Vector3 pos)
    {
        var cords = PositionToCords(pos);
        return GetStructuralTile(cords);
    }

    public FunctionalTileData GetFunctionalTile(Vector3Int cords)
    {
        return functionalTileData[cords];
    }

    public FunctionalTileData GetFunctionalTile(Vector3 pos)
    {
        var cords = PositionToCords(pos);
        return GetFunctionalTile(cords);
    }

    public void SetTile(Vector3Int cords, BaseTile tileTemplate)
    {
        switch (tileTemplate)
        {
            case FunctionalTile functionalTile:
                functionalTileData[cords] = new FunctionalTileData(functionalTile);
                functionalTilemap.SetTile(cords, functionalTile.TileBase);
                break;
            case StructuralTile structuralTile:
                structuralTileData[cords] = new StructuralTileData(structuralTile);
                structuralTilemap.SetTile(cords, structuralTile.TileBase);
                break;
        }
    }

    public void SetTiles(Vector3Int[] pos, BaseTile[] tileTemplates)
    {
        var newTiles = new TileBase[pos.Length];

        for (var i = 0; i < pos.Length; i++)
        {
            var tileTemplate = tileTemplates[i];
            var cords = pos[i];
            switch (tileTemplate)
            {
                case FunctionalTile functionalTile:
                    functionalTileData[cords] = new FunctionalTileData(functionalTile);
                    break;
                case StructuralTile structuralTile:
                    structuralTileData[cords] = new StructuralTileData(structuralTile);
                    break;
            }

            newTiles[i] = tileTemplate.TileBase;
        }

        structuralTilemap.SetTiles(pos, newTiles);
    }

    public void SetTile(Vector3 pos, BaseTile tileTemplate)
    {
        SetTile(PositionToCords(pos), tileTemplate);
    }

    public void RemoveTile(Vector3Int cords, bool structural = true)
    {
        if (structural)
        {
            structuralTileData.Remove(cords);
            structuralTilemap.SetTile(cords, null);
        }

        functionalTilemap.SetTile(cords, null);
        functionalTileData.Remove(cords);
    }

    public void RemoveTile(Vector3 pos, bool structural = true)
    {
        RemoveTile(PositionToCords(pos), structural);
    }

    public TileStatus HasTile(Vector3Int cords)
    {
        if (functionalTileData.ContainsKey(cords))
        {
            return TileStatus.Functional;
        }

        return structuralTileData.ContainsKey(cords) ? TileStatus.Structural : TileStatus.Empty;
    }

    public TileStatus HasTile(Vector3 pos)
    {
        return HasTile(PositionToCords(pos));
    }

    private void SyncFromTilemap()
    {
        var cellBounds = structuralTilemap.cellBounds;

        for (var x = cellBounds.xMin; x <= cellBounds.xMax; x++)
        for (var y = cellBounds.yMin; y <= cellBounds.yMax; y++)
        {
            var pos = new Vector3Int(x, y, 0);
            if (!structuralTilemap.HasTile(pos)) continue;
            var tile = structuralTilemap.GetTile(pos);
            structuralTileData[pos] = new StructuralTileData(TileSet.StructuralTilesDict[tile.name]);
            if (!functionalTilemap.HasTile(pos)) continue;
            var funcTile = functionalTilemap.GetTile(pos);
            functionalTileData[pos] = new FunctionalTileData(TileSet.FunctionalTilesDict[funcTile.name]);
        }
    }

    public void ApplyDamage(Damage dmg)
    {
        if (!physics)
        {
            return;
        }

        var start = PositionToCords(dmg.StartPos);
        var end = PositionToCords(dmg.EndPos);

        ApplyInLine(start, end, cords => DamageTile(cords, dmg.BaseDamage));
        Split();
    }

    private void DamageTile(Vector3Int cords, float baseDamage)
    {
        if (HasTile(cords) == TileStatus.Empty) return;

        var tile = structuralTileData[cords];
        var tileType = TileSet.StructuralTiles[tile.ID];
        var damage = (ushort) (tileType.DamageResistance * baseDamage);
        if (tile.Health <= damage)
        {
            RemoveTile(cords);
            if (structuralTileData.Count == 0)
            {
                Destroy(transform.gameObject);
            }

            return;
        }

        tile.Health -= damage;
        structuralTileData[cords] = tile;
    }

    public void ResetTiles()
    {
        structuralTilemap.ClearAllTiles();
    }

    private static void ApplyInLine(Vector3Int p0, Vector3Int p1, Action<Vector3Int> callback)
    {
        // Bresenham's line algorithm (https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm#All_cases)
        var dx = Math.Abs(p1.x - p0.x);
        var sx = p0.x < p1.x ? 1 : -1;
        var dy = -Math.Abs(p1.y - p0.y);
        var sy = p0.y < p1.y ? 1 : -1;
        var err = dx + dy;

        var x = p0.x;
        var y = p0.y;

        while (true)
        {
            callback(new Vector3Int(x, y, 0));
            if (x == p1.x && y == p1.y) break;

            var e2 = 2 * err;

            if (e2 >= dy)
            {
                err += dy;
                x += sx;
            }

            if (e2 <= dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    private List<List<Vector3Int>> FindIslands()
    {
        var width = Bounds.xMax - Bounds.xMin;
        var height = Bounds.yMax - Bounds.yMin;

        var checkedCells = new bool[width, height];

        var islands = new List<List<Vector3Int>>();

        for (var x = Bounds.xMin; x < Bounds.xMax; x++)
        for (var y = Bounds.yMin; y < Bounds.yMax; y++)
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
                var cordToCheck = posToCheck.Dequeue();

                foreach (var offset in ConnectionRules)
                {
                    var neighbor = cordToCheck + offset;
                    if (!CheckTile(neighbor)) continue;

                    island.Add(neighbor);
                    posToCheck.Enqueue(neighbor);
                }
            }
        }

        return islands;

        bool CheckTile(Vector3Int cords)
        {
            var xIdx = cords.x - Bounds.xMin;
            var yIdx = cords.y - Bounds.yMin;
            if (!Bounds.Contains(cords) || checkedCells[xIdx, yIdx]) return false;

            checkedCells[xIdx, yIdx] = true;
            return HasTile(cords) != TileStatus.Empty;
        }
    }

    private void Split()
    {
        var islands = FindIslands();
        if (islands.Count <= 1) return;

        foreach (var island in islands)
        {
            var obj = Instantiate(template);
            var newTileManager = obj.GetComponent<TileManager>();
            var newRb2D = obj.GetComponent<Rigidbody2D>();

            newTileManager.ResetTiles();

            var thisTransform = transform;
            var tileTransform = newTileManager.transform;

            tileTransform.position = thisTransform.position;
            tileTransform.rotation = thisTransform.rotation;

            var tiles = new List<BaseTile>();

            foreach (var cords in island)
            {
                switch (HasTile(cords))
                {
                    case TileStatus.Functional:
                        var funTile = GetFunctionalTile(cords);
                        tiles.Add(TileSet.FunctionalTiles[funTile.ID]);
                        goto case TileStatus.Structural;
                    case TileStatus.Structural:
                        var stcTile = GetStructuralTile(cords);
                        tiles.Add(TileSet.StructuralTiles[stcTile.ID]);
                        break;
                    case TileStatus.Empty:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            newTileManager.SetTiles(island.ToArray(), tiles.ToArray());
            if (!physics) continue;
            newTileManager.PhysicsEnabled = true;
            newRb2D.velocity = rb2D.velocity;
            newRb2D.angularVelocity = rb2D.angularVelocity;
        }

        Destroy(gameObject);
    }
}