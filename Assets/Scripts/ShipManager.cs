using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Grid))]
public class ShipManager : MonoBehaviour
{
    private static ReadOnlyCollection<Vector3Int> rules = new ReadOnlyCollection<Vector3Int>(new List<Vector3Int>
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, -1, 0),
    });

    [SerializeField] private GameObject template = null;
    private Camera cam;
    private bool physics = false;
    private Rigidbody2D rb2D;
    private TileManager tileManager;

    public bool PhysicsEnabled
    {
        set
        {
            physics = value;
            rb2D.isKinematic = !value;
        }
    }


    void Awake()
    {
        cam = Camera.main;
        rb2D = GetComponentInChildren<Rigidbody2D>();
        tileManager = GetComponentInChildren<TileManager>();
        rb2D.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!physics)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                var mousePos = Mouse.current.position;
                var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
                tileManager.SetTile(worldPos, 0);
            }
            else if (Mouse.current.middleButton.isPressed)
            {
                var mousePos = Mouse.current.position;
                var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
                tileManager.RemoveTile(worldPos);
            }
        }
    }

    public void ApplyDamage(Damage dmg)
    {
        if (!physics)
        {
            return;
        }

        tileManager.ApplyDamage(dmg);
        Split();
    }

    public void Test(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            EnablePhysic();
        }
    }

    private void EnablePhysic()
    {
        physics = true;
        Split();
        rb2D.isKinematic = false;
    }

    private List<List<Vector3Int>> FindIslands()
    {
        var bounds = tileManager.bounds;
        int width = bounds.xMax - bounds.xMin;
        int height = bounds.yMax - bounds.yMin;

        var checkedCells = new bool[width, height];

        var islands = new List<List<Vector3Int>>();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cords = new Vector3Int(x, y, 0);
                if (!CheckTile(cords)) continue;

                var posToCheck = new Queue<Vector3Int> { };
                posToCheck.Enqueue(cords);
                islands.Add(new List<Vector3Int>());
                var island = islands[islands.Count - 1];

                island.Add(new Vector3Int(x, y, 0));

                while (posToCheck.Count > 0)
                {
                    var cordToCheck = posToCheck.Dequeue();

                    foreach (var offset in rules)
                    {
                        var neighbor = cordToCheck + offset;
                        if (!CheckTile(neighbor)) continue;

                        island.Add(neighbor);
                        posToCheck.Enqueue(neighbor);
                    }
                }
            }
        }

        return islands;

        bool CheckTile(Vector3Int cords)
        {
            int xIdx = cords.x - bounds.xMin;
            int yIdx = cords.y - bounds.yMin;
            if (!bounds.Contains(cords) || checkedCells[xIdx, yIdx])
            {
                return false;
            }

            checkedCells[xIdx, yIdx] = true;
            return tileManager.HasTile(cords);
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
            var obj = Instantiate(template);
            var newShipManager = obj.GetComponentInChildren<ShipManager>();
            var newTileManager = newShipManager.tileManager;

            newTileManager.ResetTiles();

            var thisTransform = tileManager.transform;
            var tileTransform = newTileManager.transform;

            tileTransform.position = thisTransform.position;
            tileTransform.rotation = thisTransform.rotation;

            var tiles = new ushort[island.Count];

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = tileManager.GetTile(island[i]).ID;
            }

            newTileManager.SetTiles(island.ToArray(), tiles);
            if (physics)
            {
                newShipManager.PhysicsEnabled = true;
            }
        }

        Destroy(gameObject);
    }
}