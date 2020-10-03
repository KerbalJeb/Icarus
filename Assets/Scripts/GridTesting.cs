using UnityEngine;
using UnityEngine.InputSystem;

public class GridTesting : MonoBehaviour
{
    private                  Vector2[] blockUV;
    private                  Vector2[] blueBlock;
    private                  Camera    cam;
    private                  Vector2[] emptyBlock;
    private                  Vector2[] grayBlock;
    [SerializeField] private float     gridSize = 1f;
    private                  Mesh      mesh;
    private                  TileGrid  tileGrid;

    void Start()
    {
        tileGrid                        = new TileGrid(15, 15, gridSize, transform);
        cam                             = Camera.main;
        mesh                            = tileGrid.GenerateMesh();
        GetComponent<MeshFilter>().mesh = mesh;

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
    }

    private void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            blockUV = blueBlock;
        }
        else if (Mouse.current.rightButton.isPressed)
        {
            blockUV = grayBlock;
        }
        else if (Mouse.current.middleButton.isPressed)
        {
            blockUV = emptyBlock;
        }
        else
        {
            return;
        }

        var mousePos = Mouse.current.position;
        var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(),
                                                          mousePos.y.ReadValue()));

        if (tileGrid.InBounds(worldPos))
        {
            tileGrid.UpdateUV(worldPos, blockUV, ref mesh);
        }
    }
}
