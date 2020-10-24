using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapTesting : MonoBehaviour
{
    public TileBase tile;

    [SerializeField] private GameObject template = null;

    private readonly int[] col = {0, 0, 1, -1};
    private readonly int[] row = {1, -1, 0, 0};
    private bool spawn = true;
    private Tilemap tilemap;

    // Start is called before the first frame update
    void Awake()
    {
        tilemap = gameObject.GetComponent<Tilemap>();
    }

    private void Update()
    {
        if (spawn)
        {
            Split();
            spawn = false;
        }
    }

    private List<List<Vector3Int>> FindIslands()
    {
        var bounds = tilemap.cellBounds;
        int width = bounds.xMax - bounds.xMin;
        int height = bounds.yMax - bounds.yMin;

        bool[,] checkedCells = new bool[width, height];

        var islands = new List<List<Vector3Int>>();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (!CheckTile(x, y)) continue;

                Queue<(int, int)> posToCheck = new Queue<(int, int)> { };
                posToCheck.Enqueue((x, y));
                islands.Add(new List<Vector3Int>());
                var island = islands[islands.Count - 1];
                island.Add(new Vector3Int(x, y, 0));

                while (posToCheck.Count > 0)
                {
                    var (x1, y1) = posToCheck.Dequeue();

                    for (int i = 0; i < 4; i++)
                    {
                        var neighborX = x1 + row[i];
                        var neighborY = y1 + col[i];

                        if (!CheckTile(neighborX, neighborY)) continue;

                        island.Add(new Vector3Int(neighborX, neighborY, 0));
                        posToCheck.Enqueue((neighborX, neighborY));
                    }
                }
            }
        }

        return islands;

        bool CheckTile(int x, int y)
        {
            int xIdx = x - bounds.xMin;
            int yIdx = y - bounds.yMin;
            var pos = new Vector3Int(x, y, 0);
            if (!bounds.Contains(pos) || checkedCells[xIdx, yIdx])
            {
                return false;
            }

            checkedCells[xIdx, yIdx] = true;
            return tilemap.HasTile(pos);
        }
    }

    private void Split()
    {
        var islands = FindIslands();

        if (islands.Count <= 1)
        {
            return;
        }

        foreach (var island in islands)
        {
            var transform1 = transform;
            var obj = Instantiate(template, transform1.position, transform1.rotation);

            var newTilemap = obj.GetComponentInChildren<Tilemap>();

            var tiles = new TileBase[island.Count];

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = tile;
            }

            newTilemap.SetTiles(island.ToArray(), tiles);
        }

        Destroy(gameObject.transform.parent.gameObject);
    }
}