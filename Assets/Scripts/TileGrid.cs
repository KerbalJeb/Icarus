﻿using Unity.Mathematics;
using UnityEngine;

public class TileGrid
{
    [SerializeField] private readonly float gridSize;

    private Transform parentTransform;
    private Vector2[] uvMap;

    private int width;
    private int height;
    private GridData[,] gridArray;

    private int textureWidth;
    private int textureHeight;

    public TileGrid(int xMax, int yMax, float gridSize, Transform transform)
    {
        this.width = 2*xMax;
        this.height = 2*yMax;
        this.gridSize = gridSize;
        this.parentTransform = transform;

        gridArray = new GridData[width, height];

        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                gridArray[x, y].Empty = true;
            }
        }
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        var rotation = parentTransform.rotation;
        var globalPos = parentTransform.position;
        var localPos = new float3(x, y, 0f) * gridSize;
        return (rotation * localPos) + globalPos;
    }

    private (int x, int y) GetXY(Vector3 position)
    {
        var invRotation = Quaternion.Inverse(parentTransform.rotation);
        var objectPos = parentTransform.position;
        var localPos = invRotation * (position - objectPos);

        var x = Mathf.FloorToInt(localPos.x / gridSize);
        var y = Mathf.FloorToInt(localPos.y / gridSize);
        return (x, y);
    }

    public ref GridData GetValue(Vector3 position)
    {
        var (x, y) = GetXY(position);
        return ref GetValue(x, y);
    }

    public ref GridData GetValue(int x, int y)
    {
        (x, y) = CartesianToIdx(x, y);
        return ref gridArray[x, y];
    }

    public bool InBounds(Vector3 position)
    {
        var (x, y) = GetXY(position);
        (x, y) = CartesianToIdx(x, y);
        return x >= 0 && y >= 0 && x < width && y < width;
    }

    private TextMesh CreateText(string text, Vector3 offset, int fontSize, Color color)
    {
        var gameObject = new GameObject("TextObject", typeof(TextMesh));
        var transform = gameObject.transform;
        transform.localPosition = offset;
        var textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = fontSize;
        return textMesh;
    }

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[4 * width * height];

        for (int x = -width/2; x < width/2; x++)
        {
            for (int y = -width/2; y < height/2; y++)
            {
                var i = 4 * CordsToIdx(x, y);
                verts[i+0] = new Vector3((x+0) * gridSize, (y+0) * gridSize);
                verts[i+1] = new Vector3((x+1) * gridSize, (y+0) * gridSize);
                verts[i+2] = new Vector3((x+1) * gridSize, (y+1) * gridSize);
                verts[i+3] = new Vector3((x+0) * gridSize, (y+1) * gridSize);
            }
        }

        return verts;
    }

    private int[] GenerateTris()
    {
        var tris = new int[6 * width * height];
        for (int x = -width/2; x < width/2; x++)
        {
            for (int y = -width/2; y < height/2; y++)
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

    private Vector2[] GenerateUV()
    {
        var uvs = new Vector2[4 * width * height];

        for (int x = -width/2; x < width/2; x++)
        {
            for (int y = -width/2; y < height/2; y++)
            {
                var i = 4 * CordsToIdx(x, y);
                uvs[i+0] = new Vector2(0,0);
                uvs[i+1] = new Vector2(64f/135f,0);
                uvs[i+2] = new Vector2(64f/135f,1);
                uvs[i+3] = new Vector2(0,1);
            }
        }
        
        return uvs;
    }

    public Mesh GenerateMesh()
    {
        uvMap = GenerateUV();
        var mesh = new Mesh {vertices = GenerateVerts(), uv = uvMap, triangles = GenerateTris()};

        return mesh;
    }

    private int CordsToIdx(int x, int y) => x + width / 2 + (y + width / 2) * width;

    private (int, int) CartesianToIdx(int x, int y) => (x + width / 2, y + width / 2);

    public void UpdateUV(Vector3 pos, Vector2[] uvMaps, ref Mesh mesh)
    {
        var (x, y) = GetXY(pos);
        UpdateUV(x, y, uvMaps, ref mesh);
    }

    private void UpdateUV(int x, int y, Vector2[] uvMaps, ref Mesh mesh)
    {
        var idx = 4 * CordsToIdx(x, y);

        for (var i = 0; i < 4; i++)
        {
            uvMap[idx + i] = uvMaps[i];
        }

        mesh.uv = uvMap;
    }
}

public struct GridData
{
    public bool Empty;
}