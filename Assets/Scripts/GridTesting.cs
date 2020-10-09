using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridTesting : MonoBehaviour, ITransform
{
    [SerializeField] private int gridDims = 5;
    [SerializeField] private float gridSize = 1f;
    private Camera cam;
    private (int x, int y) lastPos;
    private TileGrid tileGrid;
    private ushort typeID;

    private void Start()
    {
        var texture = GetComponent<MeshRenderer>().material.mainTexture;
        var texWidth = texture.width;
        var texHeight = texture.height;

        tileGrid = new TileGrid(gridDims, gridDims, gridSize, this, new StructuralTileSet("Tiles"),
            new TileData(typeID: 0));
        cam = Camera.main;

        GetComponent<MeshFilter>().mesh = tileGrid.RenderMesh;
    }

    private void Update()
    {
        var mousePos = Mouse.current.position;
        var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
        var (x, y) = tileGrid.Get_XY(worldPos);

        if (Mouse.current.leftButton.isPressed)
        {
            typeID = tileGrid.TileSet.NameMapping["Basic Armor"];
        }
        else if (Mouse.current.middleButton.isPressed)
        {
            typeID = tileGrid.TileSet.NameMapping["Empty"];
        }
        else
        {
            if (x == lastPos.x && y == lastPos.y) return;

            if (!tileGrid.InGridBounds(x, y))
            {
                tileGrid.RefreshTile(lastPos.x, lastPos.y);
                return;
            }

            var id = tileGrid.TileSet.NameMapping["Blue Armor"];
            tileGrid.Update_UV(x, y, tileGrid.TileSet.IDMapping[id].UVMapping);
            tileGrid.RefreshTile(lastPos.x, lastPos.y);
            lastPos = (x, y);
            return;
        }

        if (tileGrid.InGridBounds(x, y))
        {
            ref var tile = ref tileGrid.GetTile(x, y);
            tile.TypeID = typeID;
            tileGrid.RefreshTile(x, y);
        }
    }

    public Quaternion Rotation => transform.rotation;
    public Vector3 Position => transform.position;

    public void GenerateCollider(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        Debug.Log("Generating Collider");

        var polygons = tileGrid.Polygons;

        var colliders = GetComponents<PolygonCollider2D>();

        foreach (var polygonCollider2D in colliders) Destroy(polygonCollider2D);

        foreach (var polygon in polygons)
        {
            var polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
            polygonCollider2D.points = polygon.Select(p => new Vector2(p.x, p.y) * tileGrid.TileSize).ToArray();
            polygonCollider2D.usedByComposite = true;
        }
    }
}