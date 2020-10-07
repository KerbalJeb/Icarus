using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridTesting : MonoBehaviour, ITransform
{
    [SerializeField] private int                  gridDims = 5;
    [SerializeField] private float                gridSize = 1f;
    private                  bool                 blockFilled;
    private                  Camera               cam;
    private                  (int x, int y)       lastPos;
    private                  Mesh                 mesh;
    private                  TileGrid             tileGrid;
    private                  TileData.TileSprites tileSprite;
    private                  TileData.TileTypes   tileType;

    private void Start()
    {
        var texture   = GetComponent<MeshRenderer>().material.mainTexture;
        var texWidth  = texture.width;
        var texHeight = texture.height;

        tileGrid = new TileGrid(gridDims, gridDims, gridSize, this,
                                new TileData(sprite: TileData.TileSprites.GrayHull, type: TileData.TileTypes.Hull));
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
            tileSprite = TileData.TileSprites.GrayHull;
            tileType   = TileData.TileTypes.Hull;
        }
        else if (Mouse.current.middleButton.isPressed)
        {
            tileSprite = TileData.TileSprites.Empty;
            tileType   = TileData.TileTypes.Empty;
        }
        else
        {
            if (x == lastPos.x && y == lastPos.y)
            {
                return;
            }

            if (!tileGrid.InGridBounds(x, y))
            {
                tileGrid.RefreshTile(lastPos.x, lastPos.y);
                return;
            }

            tileGrid.Update_UV(x, y, TileData.TileSprites.BlueHull);
            tileGrid.RefreshTile(lastPos.x, lastPos.y);
            lastPos = (x, y);
            return;
        }

        if (tileGrid.InGridBounds(x, y))
        {
            ref var tile = ref tileGrid.GetTile(x, y);
            tile.Sprite = tileSprite;
            tile.Type   = tileType;
            tileGrid.RefreshTile(x, y);
        }
    }

    public Quaternion Rotation => transform.rotation;
    public Vector3    Position => transform.position;

    public void GenerateCollider(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }

        Debug.Log("Generating Collider");

        var polygons = tileGrid.Polygons;

        var colliders = GetComponents<PolygonCollider2D>();

        foreach (var polygonCollider2D in colliders)
        {
            Destroy(polygonCollider2D);
        }

        foreach (var polygon in polygons)
        {
            var collider = gameObject.AddComponent<PolygonCollider2D>();
            collider.points          = polygon.Select(p => new Vector2(p.x, p.y) * tileGrid.TileSize).ToArray();
            collider.usedByComposite = true;
        }
    }
}
