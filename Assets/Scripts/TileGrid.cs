using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TileGrid<TObject>
{
    private readonly float _gridSize;

    private Transform _parentTransform;
    
    private int _width;
    private int _height;
    
    private TObject[,] _gridArray;
    
    public TileGrid(int width, int height, float gridSize, Transform transform)
    {
        this._width  = width;
        this._height = height;
        this._gridSize = gridSize;
        this._parentTransform = transform;
        
        _gridArray = new TObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y+1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x+1, y), Color.white, 100f);
            }
        }
        
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0),  GetWorldPosition(width, height), Color.white, 100f);
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        var rotation = _parentTransform.rotation;
        var globalPos = _parentTransform.position;
        var localPos = new float3(x, y, 0f) *_gridSize;
        return (rotation * localPos) + globalPos;
    }

    private (int x, int y) GetXY(Vector3 position)
    {
        var invRotation  = Quaternion.Inverse(_parentTransform.rotation);
        var objectPos = _parentTransform.position;
        var localPos = invRotation * (position - objectPos);

        var x = Mathf.FloorToInt(localPos.x / _gridSize);
        var y = Mathf.FloorToInt(localPos.y / _gridSize);
        return (x, y);
    }
}
