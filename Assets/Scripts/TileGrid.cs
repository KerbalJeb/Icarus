using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

/// <summary>
///     A Class for storing and managing grids of tiles
/// </summary>
public class TileGrid
{
    private readonly int gridHeight;
    private readonly int gridWidth;
    private readonly ITransform parentTransform;
    private readonly TileData[,] tileArray;
    public readonly StructuralTileSet TileSet;
    private readonly Vector2[] uvMap;

    /// <summary>
    ///     Constructor for creating a cartesian grid of tiles
    /// </summary>
    /// <param name="xMax">The width of positive half of the cartesian plane (total width of 2*xMax)</param>
    /// <param name="yMax">The height of positive half of the cartesian plane (total width of 2*yMax)</param>
    /// <param name="tileSize">The width and height of a single square tile</param>
    /// <param name="transform">The transform of the game object that is generating the grid</param>
    /// <param name="tileSet">The Tile set being used</param>
    /// <param name="defaultData"></param>
    public TileGrid(int xMax, int yMax, float tileSize, ITransform transform, StructuralTileSet tileSet,
        TileData defaultData)
    {
        gridWidth = 2 * xMax;
        gridHeight = 2 * yMax;
        TileSize = tileSize;
        parentTransform = transform;
        TileSet = tileSet;

        tileArray = new TileData[gridWidth, gridHeight];

        for (var x = 0; x < gridWidth; x++)
        for (var y = 0; y < gridHeight; y++)
            tileArray[x, y] = defaultData;

        uvMap = Generate_UV(defaultData.TypeID);
        RenderMesh = new Mesh
        {
            indexFormat = IndexFormat.UInt32, vertices = GenerateVertices(), triangles = GenerateTris(), uv = uvMap
        };
    }

    /*
     * todo: Try to clean up this code a bit
     * todo: Return only outer polygon (Doesn't currently work with more than one closed edge)
     * todo: See about improving performance
     */


    /// <summary>
    ///     Returns a list of all the polygons (list of x,y points) that make up the object
    /// </summary>
    public List<List<(int x, int y)>> Polygons
    {
        get
        {
            var edgeList = new List<((int x, int y) p1, (int x, int y) p2)>();
            for (var x = -gridWidth / 2; x < gridWidth / 2; x++)
            for (var y = -gridHeight / 2; y < gridHeight / 2; y++)
            {
                var (xIdx, yIdx) = CartesianToIdx(x, y);
                var tileData = tileArray[xIdx, yIdx];

                if (tileData.TypeID == TileSet.NameMapping["Empty"]) continue;

                if (xIdx > 0)
                {
                    var leftNeighbor = tileArray[xIdx - 1, yIdx];
                    if (leftNeighbor.TypeID == TileSet.NameMapping["Empty"]) edgeList.Add(((x, y), (x, y + 1)));
                }
                else
                {
                    edgeList.Add(((x, y), (x, y + 1)));
                }

                if (xIdx < gridWidth - 1)
                {
                    var rightNeighbor = tileArray[xIdx + 1, yIdx];
                    if (rightNeighbor.TypeID == TileSet.NameMapping["Empty"])
                        edgeList.Add(((x + 1, y), (x + 1, y + 1)));
                }
                else
                {
                    edgeList.Add(((x + 1, y), (x + 1, y + 1)));
                }

                if (yIdx > 0)
                {
                    var leftNeighbor = tileArray[xIdx, yIdx - 1];
                    if (leftNeighbor.TypeID == TileSet.NameMapping["Empty"]) edgeList.Add(((x, y), (x + 1, y)));
                }
                else
                {
                    edgeList.Add(((x, y), (x + 1, y)));
                }

                if (yIdx < gridWidth - 1)
                {
                    var leftNeighbor = tileArray[xIdx, yIdx + 1];
                    if (leftNeighbor.TypeID == TileSet.NameMapping["Empty"]) edgeList.Add(((x, y + 1), (x + 1, y + 1)));
                }
                else
                {
                    edgeList.Add(((x, y + 1), (x + 1, y + 1)));
                }
            }

            SimplifyEdges(ref edgeList);
            return EdgeListToPoints(edgeList);
        }
    }

    /// <value>
    ///     Returns the mesh used to render the tiles
    /// </value>
    public Mesh RenderMesh { get; }

    /// <value>
    ///     The height/width of individual tiles
    /// </value>
    public float TileSize { get; }

    /// <summary>
    ///     Converts a list of edges into a list of polygons (a list of ordered points)
    /// </summary>
    /// <param name="list">The list of edges</param>
    /// <returns>The list of polygons</returns>
    private List<List<(int x, int y)>> EdgeListToPoints(List<((int x, int y) p1, (int x, int y) p2)> list)
    {
        var verts = new Dictionary<(int x, int y), List<((int x, int y) p1, (int x, int y) p2)>>();
        foreach (var edge in list)
        {
            if (!verts.ContainsKey(edge.p1)) verts[edge.p1] = new List<((int x, int y) p1, (int x, int y) p2)>();

            if (!verts.ContainsKey(edge.p2)) verts[edge.p2] = new List<((int x, int y) p1, (int x, int y) p2)>();

            verts[edge.p1].Add(edge);
            verts[edge.p2].Add(edge);
        }

        var polygons = new List<List<(int x, int y)>>();

        while (verts.Count > 0)
        {
            polygons.Add(new List<(int x, int y)>());
            var p = verts.Keys.ToArray()[0];

            var usedVerts = new List<(int x, int y)>();

            while (true)
            {
                if (usedVerts.Contains(p)) break;

                polygons[0].Add(p);
                usedVerts.Add(p);

                foreach (var (p1, p2) in verts[p])
                {
                    if (p1 != p && !usedVerts.Contains(p1))
                    {
                        p = p1;
                        break;
                    }

                    if (p2 != p && !usedVerts.Contains(p2))
                    {
                        p = p2;
                        break;
                    }
                }
            }

            foreach (var vert in usedVerts) verts.Remove(vert);
        }

        return polygons;
    }

    /// <summary>
    ///     Removes redundant vertices from an edge list
    /// </summary>
    /// <param name="list">The list to simplify</param>
    private void SimplifyEdges(ref List<((int x, int y) p1, (int x, int y) p2)> list)
    {
        var xMapping = new Dictionary<int, List<((int x, int y) p1, (int x, int y) p2)>>();
        var yMapping = new Dictionary<int, List<((int x, int y) p1, (int x, int y) p2)>>();

        foreach (var edge in list)
        {
            if (edge.p1.x == edge.p2.x)
            {
                if (!xMapping.ContainsKey(edge.p1.x))
                    xMapping[edge.p1.x] = new List<((int x, int y) p1, (int x, int y) p2)>();

                xMapping[edge.p1.x].Add(edge);
            }

            if (edge.p1.y == edge.p2.y)
            {
                if (!yMapping.ContainsKey(edge.p1.y))
                    yMapping[edge.p1.y] = new List<((int x, int y) p1, (int x, int y) p2)>();

                yMapping[edge.p1.y].Add(edge);
            }
        }

        list = SimplifyDim(xMapping);
        list.AddRange(SimplifyDim(yMapping));
    }

    /// <summary>
    ///     Helper function used to simplify all lines in a single direction
    /// </summary>
    /// <param name="mapping">A vert map</param>
    /// <returns>A list of simplified edges</returns>
    private List<((int x, int y) p1, (int x, int y) p2)> SimplifyDim(
        Dictionary<int, List<((int x, int y) p1, (int x, int y) p2)>> mapping)
    {
        var simplifiedEdges = new List<((int x, int y) p1, (int x, int y) p2)>();

        foreach (var newEdge in mapping.Select(keyValue => keyValue.Value).SelectMany(newEdges => newEdges))
        {
            var updatedEdge = false;
            for (var i = 0; i < simplifiedEdges.Count; i++)
            {
                var (p1, p2) = simplifiedEdges[i];

                if (p1 == newEdge.p1)
                {
                    simplifiedEdges[i] = (p2, newEdge.p2);
                    updatedEdge = true;
                    break;
                }

                if (p2 == newEdge.p2)
                {
                    simplifiedEdges[i] = (p1, newEdge.p1);
                    updatedEdge = true;
                    break;
                }

                if (p1 == newEdge.p2)
                {
                    simplifiedEdges[i] = (p2, newEdge.p1);
                    updatedEdge = true;
                    break;
                }

                if (p2 == newEdge.p1)
                {
                    simplifiedEdges[i] = (p1, newEdge.p2);
                    updatedEdge = true;
                    break;
                }
            }

            if (!updatedEdge) simplifiedEdges.Add(newEdge);
        }

        return simplifiedEdges;
    }

    /// <summary>
    ///     Gets the world position of the lower left corner of a tile
    /// </summary>
    /// <param name="x">The cartesian x index of the tile</param>
    /// <param name="y">The cartesian y index of the tile</param>
    /// <returns>The world position as a Vector3</returns>
    public Vector3 GetWorldPosition(int x, int y)
    {
        var rotation = parentTransform.Rotation;
        var globalPos = parentTransform.Position;
        var localPos = new Vector3(x, y, 0f) * TileSize;
        return rotation * localPos + globalPos;
    }

    /// <summary>
    ///     Get the cartesian index of the block at a position in world coordinates (does not check bounds)
    /// </summary>
    /// <param name="position">The world postion</param>
    /// <returns>The cartesian index as a tuple</returns>
    public (int x, int y) Get_XY(Vector3 position)
    {
        var invRotation = Quaternion.Inverse(parentTransform.Rotation);
        var objectPos = parentTransform.Position;
        var localPos = invRotation * (position - objectPos);

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
        if (!InGridBounds(x, y)) return default;

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
        (x, y) = CartesianToIdx(x, y);
        return InGridBounds(x, y);
    }

    /// <summary>
    ///     Checks if a cartesian index is inside the grid
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <returns>true if in the position is inside the grid, false otherwise</returns>
    public bool InGridBounds(int x, int y)
    {
        return x >= -gridWidth / 2 && x < gridWidth / 2 && y >= -gridHeight / 2 && y < gridHeight / 2;
    }

    /// <summary>
    ///     Generates vertices for a quad in each grid position (Used to generate the mesh)
    /// </summary>
    /// <returns>The array of generated vertices</returns>
    private Vector3[] GenerateVertices()
    {
        var verts = new Vector3[4 * gridWidth * gridHeight];

        for (var x = -gridWidth / 2; x < gridWidth / 2; x++)
        for (var y = -gridWidth / 2; y < gridHeight / 2; y++)
        {
            var i = 4 * CordsTo1D_Idx(x, y);
            verts[i + 0] = new Vector3((x + 0) * TileSize, (y + 0) * TileSize);
            verts[i + 1] = new Vector3((x + 1) * TileSize, (y + 0) * TileSize);
            verts[i + 2] = new Vector3((x + 1) * TileSize, (y + 1) * TileSize);
            verts[i + 3] = new Vector3((x + 0) * TileSize, (y + 1) * TileSize);
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

        return tris;
    }

    /// <summary>
    ///     Generates the UV mapping for the vertices of the mesh
    /// </summary>
    /// <param name="defaultID"></param>
    /// <returns>The generated UV mappings</returns>
    private Vector2[] Generate_UV(ushort defaultID)
    {
        var uvs = new Vector2[4 * gridWidth * gridHeight];
        var defaultUV = TileSet.IDMapping[defaultID].UVMapping;
        Assert.AreEqual(defaultUV.Length, 4);

        for (var i = 0; i < uvs.Length; i++) uvs[i] = defaultUV[i % 4];

        return uvs;
    }

    /// <summary>
    ///     Converts cartesian indexes to an index that can be used on a 1D array (Used for generating the mesh)
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <returns>The 1D index</returns>
    private int CordsTo1D_Idx(int x, int y)
    {
        return x + gridWidth / 2 + (y + gridWidth / 2) * gridWidth;
    }

    /// <summary>
    ///     Converts cartesian indexes to positive indexes that can be used with the tileArray
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <returns>The positive indexes in a tuple (x, y)</returns>
    private (int x, int y) CartesianToIdx(int x, int y)
    {
        return (x + gridWidth / 2, y + gridWidth / 2);
    }

    /// <summary>
    ///     Update the UV mapping of a tile at a position in world space, ignores invalid positions
    /// </summary>
    /// <param name="pos">The world postion</param>
    /// <param name="newUVMap">The UV Mapping to use</param>
    public void Update_UV(Vector3 pos, Vector2[] newUVMap)
    {
        var (x, y) = Get_XY(pos);
        Update_UV(x, y, newUVMap);
    }

    /// <summary>
    ///     Update the UV mapping of a tile, ignores invalid positions
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    /// <param name="newUVMap">The UV Mapping to use</param>
    public void Update_UV(int x, int y, Vector2[] newUVMap)
    {
        if (!InGridBounds(x, y)) return;

        var idx = 4 * CordsTo1D_Idx(x, y);
        for (var i = 0; i < 4; i++) uvMap[idx + i] = newUVMap[i];

        RenderMesh.uv = uvMap;
    }

    public ref TileData GetTile(Vector3 pos)
    {
        var (x, y) = Get_XY(pos);
        return ref GetTile(x, y);
    }

    public ref TileData GetTile(int x, int y)
    {
        (x, y) = CartesianToIdx(x, y);
        return ref tileArray[x, y];
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
        if (!InGridBounds(x, y)) return;

        var (xIdx, yIdx) = CartesianToIdx(x, y);
        tileArray[xIdx, yIdx] = tileData;
        Update_UV(x, y, TileSet.IDMapping[tileData.TypeID].UVMapping);
    }

    /// <summary>
    ///     Updates the UV mapping of a tile based on the current data for that tile
    /// </summary>
    /// <param name="x">The x cartesian index</param>
    /// <param name="y">The y cartesian index</param>
    public void RefreshTile(int x, int y)
    {
        if (!InGridBounds(x, y)) return;

        var (xIdx, yIdx) = CartesianToIdx(x, y);

        Update_UV(x, y, TileSet.IDMapping[tileArray[xIdx, yIdx].TypeID].UVMapping);
    }
}

public interface ITransform
{
    Quaternion Rotation { get; }
    Vector3 Position { get; }
}