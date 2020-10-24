using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TileManager : MonoBehaviour
{
    [SerializeField] private string tilePath = null;
    private Dictionary<Vector3Int, TileData> data = new Dictionary<Vector3Int, TileData>();
    private Tilemap tilemap;
    private TileSet tileSet;
    private float tileSize;

    public BoundsInt bounds => tilemap.cellBounds;


    public void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        var grid = GetComponentInParent<Grid>();
        tileSize = grid.cellSize.x;
        tileSet = TileSet.GetTileSet(tilePath);
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

    public TileData GetTile(Vector3Int cords)
    {
        return data[cords];
    }

    public TileData GetTile(Vector3 vec)
    {
        var cords = PositionToCords(vec);
        return GetTile(cords);
    }

    public void SetTile(Vector3Int cords, ushort id)
    {
        var tileData = tileSet.TileTypes[id];
        data[cords] = new TileData(tileData);
        tilemap.SetTile(new Vector3Int(cords.x, cords.y, 0), tileData.TileBase);
    }

    public void SetTiles(Vector3Int[] pos, ushort[] ids)
    {
        var newTiles = new TileBase[pos.Length];

        for (int i = 0; i < pos.Length; i++)
        {
            var tileData = tileSet.TileTypes[ids[i]];
            data[pos[i]] = new TileData(tileData);
            newTiles[i] = tileData.TileBase;
        }

        tilemap.SetTiles(pos, newTiles);
    }

    public void SetTile(Vector3 pos, ushort id)
    {
        SetTile(PositionToCords(pos), id);
    }

    public void RemoveTile(Vector3Int cords)
    {
        data.Remove(cords);
        tilemap.SetTile(cords, null);
    }

    public void RemoveTile(Vector3 pos)
    {
        RemoveTile(PositionToCords(pos));
    }

    public bool HasTile(Vector3Int cords)
    {
        return data.ContainsKey(cords);
    }

    public bool HasTile(Vector3 pos)
    {
        return HasTile(PositionToCords(pos));
    }

    private void SyncFromTilemap()
    {
        var cellBounds = tilemap.cellBounds;

        for (int x = cellBounds.xMin; x <= cellBounds.xMax; x++)
        {
            for (int y = cellBounds.yMin; y <= cellBounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(pos))
                {
                    var tile = tilemap.GetTile(pos);
                    var cords = new Vector3Int(x, y, 0);
                    data[cords] = new TileData(tileSet.TileTypesDict[tile.name]);
                }
            }
        }
    }

    public void ApplyDamage(Damage dmg)
    {
        var start = PositionToCords(dmg.StartPos);
        var end = PositionToCords(dmg.EndPos);

        var effectedTiles = MakeLine(start, end);

        foreach (var cords in effectedTiles)
        {
            DamageTile(cords, dmg.BaseDamage);
        }
    }

    private void DamageTile(Vector3Int cords, float baseDamage)
    {
        if (!HasTile(cords))
        {
            return;
        }

        var tile = data[cords];
        var tileType = tileSet.TileTypes[tile.ID];
        var damage = (short) (tileType.DamageResistance * baseDamage);
        tile.Health -= damage;
        if (tile.Health <= 0)
        {
            RemoveTile(cords);
            if (data.Count == 0)
            {
                Destroy(transform.parent.gameObject);
            }

            return;
        }

        data[cords] = tile;
    }

    public void ResetTiles()
    {
        tilemap.ClearAllTiles();
    }

    private static IEnumerable<Vector3Int> MakeLine(Vector3Int p0, Vector3Int p1)
    {
        int dx = Math.Abs(p1.x - p0.x);
        int sx = p0.x < p1.x ? 1 : -1;
        int dy = -Math.Abs(p1.y - p0.y);
        int sy = p0.y < p1.y ? 1 : -1;
        int err = dx + dy;
        var line = new List<Vector3Int>();

        int x = p0.x;
        int y = p0.y;

        while (true)
        {
            line.Add(new Vector3Int(x, y, 0));
            if (x == p1.x && y == p1.y)
            {
                break;
            }

            int e2 = 2 * err;

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

        return line;
    }
}