using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class TileGrid
{
    private readonly GridData[,] gridArray;
    private readonly int         height;
    private readonly Transform   parentTransform;

    private readonly Vector2[] uvMap;
    private readonly int       width;
    private          int       textureHeight;
    private          int       textureWidth;

    public TileGrid(int xMax, int yMax, float gridSize, Transform transform, Vector2[] defaultUV, bool defaultFilled)
    {
        width           = 2 * xMax;
        height          = 2 * yMax;
        GridSize        = gridSize;
        parentTransform = transform;

        gridArray = new GridData[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                gridArray[x, y] = new GridData(defaultUV, defaultFilled);
            }
        }

        uvMap      = GenerateUV(defaultUV);
        RenderMesh = new Mesh {vertices = GenerateVerts(), uv = uvMap, triangles = GenerateTris()};
    }

    public Vector2[] FilledBoxes
    {
        get
        {
            var boxesList = new List<Vector2>();
            for (var x = -width / 2; x < width / 2; x++)
            {
                for (var y = -height / 2; y < height / 2; y++)
                {
                    var (xIdx, yIdx) = CartesianToIdx(x, y);
                    var gridData = gridArray[xIdx, yIdx];
                    if (gridData.Filled)
                    {
                        boxesList.Add(new Vector2(x, y));
                    }
                }
            }

            return boxesList.ToArray();
        }
    }

    public Mesh RenderMesh { get; }

    public float GridSize { get; }

    private Vector3 GetWorldPosition(int x, int y)
    {
        var rotation  = parentTransform.rotation;
        var globalPos = parentTransform.position;
        var localPos  = new float3(x, y, 0f) * GridSize;
        return rotation * localPos + globalPos;
    }

    public (int x, int y) GetXY(Vector3 position)
    {
        var invRotation = Quaternion.Inverse(parentTransform.rotation);
        var objectPos   = parentTransform.position;
        var localPos    = invRotation * (position - objectPos);

        var x = Mathf.FloorToInt(localPos.x / GridSize);
        var y = Mathf.FloorToInt(localPos.y / GridSize);
        return (x, y);
    }

    public GridData GetValue(Vector3 position)
    {
        var (x, y) = GetXY(position);
        return GetValue(x, y);
    }

    public GridData GetValue(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return default;
        }

        (x, y) = CartesianToIdx(x, y);
        return gridArray[x, y];
    }

    public bool InBounds(Vector3 position)
    {
        var (x, y) = GetXY(position);
        (x, y)     = CartesianToIdx(x, y);
        return InBounds(x, y);
    }

    public bool InBounds(int x, int y) => x >= -width / 2 && x < width / 2 && y >= -height / 2 && y < height / 2;

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[4 * width * height];

        for (var x = -width / 2; x < width / 2; x++)
        {
            for (var y = -width / 2; y < height / 2; y++)
            {
                var i = 4 * CordsToIdx(x, y);
                verts[i + 0] = new Vector3((x + 0) * GridSize, (y + 0) * GridSize);
                verts[i + 1] = new Vector3((x + 1) * GridSize, (y + 0) * GridSize);
                verts[i + 2] = new Vector3((x + 1) * GridSize, (y + 1) * GridSize);
                verts[i + 3] = new Vector3((x + 0) * GridSize, (y + 1) * GridSize);
            }
        }

        return verts;
    }

    private int[] GenerateTris()
    {
        var tris = new int[6 * width * height];
        for (var x = -width / 2; x < width / 2; x++)
        {
            for (var y = -width / 2; y < height / 2; y++)
            {
                var i = 6 * CordsToIdx(x, y);
                var j = 4 * CordsToIdx(x, y);

                tris[i + 0] = j + 0;
                tris[i + 1] = j + 3;
                tris[i + 2] = j + 1;

                tris[i + 3] = j + 1;
                tris[i + 4] = j + 3;
                tris[i + 5] = j + 2;
            }
        }

        return tris;
    }

    private Vector2[] GenerateUV(Vector2[] defaltUV)
    {
        var uvs = new Vector2[4 * width * height];
        Assert.AreEqual(defaltUV.Length, 4);

        for (var i = 0; i < uvs.Length; i++)
        {
            uvs[i] = defaltUV[i % 4];
        }

        return uvs;
    }

    private int CordsToIdx(int x, int y) => x + width / 2 + (y + width / 2) * width;

    private (int, int) CartesianToIdx(int x, int y) => (x + width / 2, y + width / 2);

    public void UpdateUV(Vector3 pos, Vector2[] uvMaps)
    {
        var (x, y) = GetXY(pos);
        UpdateUV(x, y, uvMaps);
    }

    public void UpdateUV(int x, int y, Vector2[] uvMaps)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        var idx = 4 * CordsToIdx(x, y);
        for (var i = 0; i < 4; i++)
        {
            uvMap[idx + i] = uvMaps[i];
        }

        RenderMesh.uv = uvMap;
    }

    public void UpdateBlock(Vector3 pos, GridData gridData)
    {
        var (x, y) = GetXY(pos);
        UpdateBlock(x, y, gridData);
    }

    public void UpdateBlock(int x, int y, GridData gridData)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        var (xIdx, yIdx)      = CartesianToIdx(x, y);
        gridArray[xIdx, yIdx] = gridData;
        UpdateUV(x, y, gridData.UVCords);
    }

    public void UpdateBlock(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        var (xIdx, yIdx) = CartesianToIdx(x, y);
        UpdateUV(x, y, gridArray[xIdx, yIdx].UVCords);
    }
}

public struct GridData
{
    public bool      Filled;
    public Vector2[] UVCords;

    public GridData(Vector2[] uvCords, bool filled)
    {
        Filled  = filled;
        UVCords = uvCords;
    }
}
