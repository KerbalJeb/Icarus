using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
///     A Class for storing and managing grids of tiles
/// </summary>
public class TileGrid
{
    private readonly int         gridHeight;
    private readonly int         gridWidth;
    private readonly Transform   parentTransform;
    private readonly TileData[,] tileArray;

    private readonly Vector2[] uvMap;
    private          int       textureHeight;
    private          int       textureWidth;

    /// <summary>
    ///     Constructor for creating a cartesian grid of tiles
    /// </summary>
    /// <param name="xMax">The width of positive half of the cartesian plane (total width of 2*xMax)</param>
    /// <param name="yMax">The height of positive half of the cartesian plane (total width of 2*yMax)</param>
    /// <param name="tileSize">The width and height of a single square tile</param>
    /// <param name="transform">The transform of the game object that is generating the grid</param>
    /// <param name="defaultData"> The default data to populate the tiles with </param>
    public TileGrid(int xMax, int yMax, float tileSize, Transform transform, TileData defaultData)
    {
        Assert.AreEqual(defaultData.UVCords.Length, 4);
        gridWidth       = 2 * xMax;
        gridHeight      = 2 * yMax;
        TileSize        = tileSize;
        parentTransform = transform;

        tileArray = new TileData[gridWidth, gridHeight];

        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                tileArray[x, y] = defaultData;
            }
        }

        uvMap      = Generate_UV(defaultData.UVCords);
        RenderMesh = new Mesh {vertices = GenerateVertices(), uv = uvMap, triangles = GenerateTris()};
    }

    /// <summary>
    ///     Returns a list of Vector2s containing the cartesian coordinates of tiles that are occupied (tile.Filled is True)
    /// </summary>
    public Vector2[] FilledBoxes
    {
        get
        {
            var tileList = new List<Vector2>();
            for (var x = -gridWidth / 2; x < gridWidth / 2; x++)
            {
                for (var y = -gridHeight / 2; y < gridHeight / 2; y++)
                {
                    var (xIdx, yIdx) = CartesianToIdx(x, y);
                    var tileData = tileArray[xIdx, yIdx];
                    if (tileData.Filled)
                    {
                        tileList.Add(new Vector2(x, y));
                    }
                }
            }

            return tileList.ToArray();
        }
    }

    /// <summary>
    ///     Returns the mesh used to render the tiles
    /// </summary>
    public Mesh RenderMesh { get; }

    /// <summary>
    ///     The height/width of individual tiles
    /// </summary>
    public float TileSize { get; }

    /// <summary>
    ///     Gets the world position of the lower left corner of a tile
    /// </summary>
    /// <param name="x">The cartesian x index of the tile</param>
    /// <param name="y">The cartesian y index of the tile</param>
    /// <returns>The world position as a Vector3</returns>
    private Vector3 GetWorldPosition(int x, int y)
    {
        var rotation  = parentTransform.rotation;
        var globalPos = parentTransform.position;
        var localPos  = new float3(x, y, 0f) * TileSize;
        return rotation * localPos + globalPos;
    }

    /// <summary>
    ///     Get the cartesian index of the block at a position in world coordinates (does not check bounds)
    /// </summary>
    /// <param name="position">The world postion</param>
    /// <returns>The cartesian index as a tuple</returns>
    public (int x, int y) Get_XY(Vector3 position)
    {
        var invRotation = Quaternion.Inverse(parentTransform.rotation);
        var objectPos   = parentTransform.position;
        var localPos    = invRotation * (position - objectPos);

        var x = Mathf.FloorToInt(localPos.x / TileSize);
        var y = Mathf.FloorToInt(localPos.y / TileSize);
        return (x, y);
    }

    /// <summary>
    ///     Gets the value of a tile at a position in the world
    /// </summary>
    /// <param name="position">The position in world cordinates</param>
    /// <returns>The TileData of the tile at the given position, returns a default value if out of range</returns>
    public TileData GetValue(Vector3 position)
    {
        var (x, y) = Get_XY(position);
        return GetValue(x, y);
    }

    /// <summary>
    ///     Gets teh value of a tile from its cartesian indexes
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <returns>The TileData of the tile at the given position, returns a default value if out of range</returns>
    public TileData GetValue(int x, int y)
    {
        if (!InGridBounds(x, y))
        {
            return default;
        }

        (x, y) = CartesianToIdx(x, y);
        return tileArray[x, y];
    }

    /// <summary>
    ///     Checks if a world position is inside the grid
    /// </summary>
    /// <param name="position">The world position to check</param>
    /// <returns>true if in the position is inside the grid, false otherwise</returns>
    public bool InGridBounds(Vector3 position)
    {
        var (x, y) = Get_XY(position);
        (x, y)     = CartesianToIdx(x, y);
        return InGridBounds(x, y);
    }

    /// <summary>
    ///     Checks if a cartesian index is inside teh grid
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">y</param>
    /// <returns>true if in the position is inside the grid, false otherwise</returns>
    public bool InGridBounds(int x, int y) =>
        x >= -gridWidth / 2 && x < gridWidth / 2 && y >= -gridHeight / 2 && y < gridHeight / 2;

    /// <summary>
    ///     Generates vertices for a quad in each grid position (Used to generate the mesh)
    /// </summary>
    /// <returns>The array of generated vertices</returns>
    private Vector3[] GenerateVertices()
    {
        var verts = new Vector3[4 * gridWidth * gridHeight];

        for (var x = -gridWidth / 2; x < gridWidth / 2; x++)
        {
            for (var y = -gridWidth / 2; y < gridHeight / 2; y++)
            {
                var i = 4 * CordsTo1D_Idx(x, y);
                verts[i + 0] = new Vector3((x + 0) * TileSize, (y + 0) * TileSize);
                verts[i + 1] = new Vector3((x + 1) * TileSize, (y + 0) * TileSize);
                verts[i + 2] = new Vector3((x + 1) * TileSize, (y + 1) * TileSize);
                verts[i + 3] = new Vector3((x + 0) * TileSize, (y + 1) * TileSize);
            }
        }

        return verts;
    }

    /// <summary>
    ///     Generates the triangles for a quad in each grid position (Used to generate the mesh)
    /// </summary>
    /// <returns>
    ///     The array of generated triangles (2 per quad, 3 ints per triangle corresponding to the vertices indexes that
    ///     make them up)
    /// </returns>
    private int[] GenerateTris()
    {
        var tris = new int[6 * gridWidth * gridHeight];
        for (var x = -gridWidth / 2; x < gridWidth / 2; x++)
        {
            for (var y = -gridWidth / 2; y < gridHeight / 2; y++)
            {
                var i = 6 * CordsTo1D_Idx(x, y);
                var j = 4 * CordsTo1D_Idx(x, y);

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

    /// <summary>
    ///     Generates the UV mapping for the vertices of the mesh
    /// </summary>
    /// <param name="defaltUV">The UV mapping to use, Must an array of EXACTLY 4 Vector2s</param>
    /// <returns>The generated UV mappings</returns>
    private Vector2[] Generate_UV(Vector2[] defaltUV)
    {
        var uvs = new Vector2[4 * gridWidth * gridHeight];
        Assert.AreEqual(defaltUV.Length, 4);

        for (var i = 0; i < uvs.Length; i++)
        {
            uvs[i] = defaltUV[i % 4];
        }

        return uvs;
    }

    /// <summary>
    ///     Converts cartesian indexes to an index that can be used on a 1D array (Used for generating the mesh)
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <returns>The 1D index</returns>
    private int CordsTo1D_Idx(int x, int y) => x + gridWidth / 2 + (y + gridWidth / 2) * gridWidth;

    /// <summary>
    ///     Converts cartesian indexes to positive indexes that can be used with the tileArray
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <returns>The positive indexes in a tuple (x, y)</returns>
    private (int x, int y) CartesianToIdx(int x, int y) => (x + gridWidth / 2, y + gridWidth / 2);

    /// <summary>
    ///     Update the UV mapping of a tile at a position in world space, ignores invalid positions
    /// </summary>
    /// <param name="pos">The world postion</param>
    /// <param name="uvMaps">The new UV mapping (Array must have EXACTLY 4 elements)</param>
    public void Update_UV(Vector3 pos, Vector2[] uvMaps)
    {
        var (x, y) = Get_XY(pos);
        Update_UV(x, y, uvMaps);
    }

    /// <summary>
    ///     Update the UV mapping of a tile, ignores invalid positions
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <param name="uvMaps">The new UV mapping (Array must have EXACTLY 4 elements)</param>
    public void Update_UV(int x, int y, Vector2[] uvMaps)
    {
        if (!InGridBounds(x, y))
        {
            return;
        }

        var idx = 4 * CordsTo1D_Idx(x, y);
        for (var i = 0; i < 4; i++)
        {
            uvMap[idx + i] = uvMaps[i];
        }

        RenderMesh.uv = uvMap;
    }

    /// <summary>
    ///     Updates the data of a tile (updates UV mapping as well), ignores invalid positions
    /// </summary>
    /// <param name="pos">The world position</param>
    /// <param name="tileData">The new data for the tile</param>
    public void UpdateTile(Vector3 pos, TileData tileData)
    {
        var (x, y) = Get_XY(pos);
        UpdateTile(x, y, tileData);
    }

    /// <summary>
    ///     Updates the data of a tile (updates UV mapping as well), ignores invalid positions
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <param name="tileData">The new data for the tile</param>
    public void UpdateTile(int x, int y, TileData tileData)
    {
        if (!InGridBounds(x, y))
        {
            return;
        }

        var (xIdx, yIdx)      = CartesianToIdx(x, y);
        tileArray[xIdx, yIdx] = tileData;
        Update_UV(x, y, tileData.UVCords);
    }

    /// <summary>
    ///     Updates the UV mapping of a tile based on the current data for that tile
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    public void RefreshTile(int x, int y)
    {
        if (!InGridBounds(x, y))
        {
            return;
        }

        var (xIdx, yIdx) = CartesianToIdx(x, y);
        Update_UV(x, y, tileArray[xIdx, yIdx].UVCords);
    }
}

/// <summary>
///     The basic data structure used for all tiles
/// </summary>
public struct TileData
{
    public bool      Filled;
    public Vector2[] UVCords;

    public TileData(Vector2[] uvCords, bool filled)
    {
        Filled  = filled;
        UVCords = uvCords;
    }
}
