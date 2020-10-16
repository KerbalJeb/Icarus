using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEditor;

public class TileAutomation : MonoBehaviour
{
    [Range(0,100)] //percentage range
    public int iniChance;
    [Range(1,8)]
    public int birthLimit;
    [Range(1,8)]
    public int deathLimit;
    [Range(1,10)]
    public int numRepitions;
    private int count = 0; // # files count

    private int[,] terrainMap;
    public Vector3Int tileMapSize;
    public Tilemap topMap;
    public Tilemap bottomMap;

    public TileBase topTile;
    public TileBase bottomTile;

    int width;
    int height;

    public void simulation(int numRep)
    {
        clearMap(false);
        width = tileMapSize.x;
        height = tileMapSize.y;

        if (terrainMap == null){
            terrainMap = new int[width,height];
            initPos();
        }

        for (int i = 0; i < numRep; i++){
            terrainMap = genTilePos(terrainMap);
        }

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            if(terrainMap[x,y] == 1) {
                topMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), topTile);
                bottomMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), bottomTile);
            }
        }
    }

    }

    public int[,] genTilePos(int[,] oldMap){ //generate terrain method
        int[,] newMap = new int[width, height];
        int neighbour;
        BoundsInt myBound = new BoundsInt(-1,-1,0,3,3,1);

        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++)
            {
                neighbour = 0;
                foreach (var b in myBound.allPositionsWithin)
                {
                    if(b.x == 0 && b.y == 0) continue;
                    if(x+b.x >= 0 && x+b.x < width & y+b.y >=0 && y+b.y < height){
                        neighbour += oldMap[x + b.x, y + b.y];
                    } else {
                        neighbour++;
                    }
                }
                if (oldMap[x,y] == 1) {
                    if (neighbour < deathLimit) newMap [x,y] = 0;
                    else {
                        newMap[x,y] = 1;
                    }
                }

                if (oldMap[x,y] == 0) {
                    if (neighbour > birthLimit) newMap [x,y] = 1;
                    else {
                       newMap[x,y] = 0;
                    }
                }
            }
        }
        return newMap;
    }
    public void initPos() {
        for(int x = 0; x < width; x++){
            for(int y = 0; y < height; y++){
                terrainMap[x,y] = Random.Range(1,101) < iniChance ? 1 : 0;
            }
        }

        
    }


   
    // Update is called once per frame
    void Update()
    {
        if(Mouse.current.leftButton.isPressed) {
            simulation(numRepitions);
            Debug.Log("Mouse Clicked");
        }
        if(Mouse.current.middleButton.isPressed) {
            clearMap(true);
            Debug.Log("Mouse Clicked2");
        }
    }

    public void clearMap(bool complete)
    {
        topMap.ClearAllTiles();
        bottomMap.ClearAllTiles();

        if(complete){
            terrainMap = null;
        }
    }
}
