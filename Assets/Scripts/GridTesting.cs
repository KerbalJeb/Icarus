using UnityEngine;
using UnityEngine.InputSystem;

public class GridTesting : MonoBehaviour, ITransform
{
    [SerializeField] private int            gridDims = 5;
    [SerializeField] private float          gridSize = 1f;
    private                  bool           blockFilled;
    private                  Vector2[]      blockUV;
    private                  Vector2[]      blueBlock;
    private                  Camera         cam;
    private                  Vector2[]      emptyBlock;
    private                  Vector2[]      grayBlock;
    private                  (int x, int y) lastPos;
    private                  Mesh           mesh;
    private                  TileGrid       tileGrid;

    private void Start()
    {
        var texture   = GetComponent<MeshRenderer>().material.mainTexture;
        var texWidth  = texture.width;
        var texHeight = texture.height;

        grayBlock = new[]
        {
            new Vector2(0,              0),
            new Vector2(64f / texWidth, 0),
            new Vector2(64f / texWidth, 64f / texHeight),
            new Vector2(0,              64f / texHeight),
        };

        blueBlock = new[]
        {
            new Vector2(68f  / texWidth, 0),
            new Vector2(132f / texWidth, 0),
            new Vector2(132f / texWidth, 64f / texHeight),
            new Vector2(68f  / texWidth, 64f / texHeight),
        };

        emptyBlock = new[]
        {
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 0),
        };

        tileGrid = new TileGrid(gridDims, gridDims, gridSize, this, new TileData(emptyBlock, false));
        cam      = Camera.main;

        GetComponent<MeshFilter>().mesh = tileGrid.RenderMesh;
    }

    private void Update()
    {
        var mousePos = Mouse.current.position;
        var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));

        if (Mouse.current.leftButton.isPressed)
        {
            blockUV     = grayBlock;
            blockFilled = true;
        }
        else if (Mouse.current.middleButton.isPressed)
        {
            blockUV     = emptyBlock;
            blockFilled = false;
        }
        else
        {
            var (x, y) = tileGrid.Get_XY(worldPos);
            if (x == lastPos.x && y == lastPos.y)
            {
                return;
            }

            if (!tileGrid.InGridBounds(x, y))
            {
                tileGrid.RefreshTile(lastPos.x, lastPos.y);
                return;
            }

            tileGrid.Update_UV(x, y, blueBlock);
            tileGrid.RefreshTile(lastPos.x, lastPos.y);
            lastPos = (x, y);
            return;
        }


        tileGrid.UpdateTile(worldPos, new TileData(blockUV, blockFilled));
    }

    public Quaternion Rotation => transform.rotation;
    public Vector3    Position => transform.position;

    public void GenerateCollider(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }

        var collider2Ds = GetComponents<BoxCollider2D>();
        foreach (var boxCollider in collider2Ds)
        {
            Destroy(boxCollider);
        }

        var boxes = tileGrid.FilledBoxes;
        var size  = new Vector2(tileGrid.TileSize, tileGrid.TileSize);
        foreach (var box in boxes)
        {
            var newBoxCollider2D = gameObject.AddComponent<BoxCollider2D>();
            var offset           = box * tileGrid.TileSize + new Vector2(1, 1) * 0.5f * tileGrid.TileSize;
            newBoxCollider2D.offset          = offset;
            newBoxCollider2D.size            = size;
            newBoxCollider2D.usedByComposite = true;
        }
    }
}
