using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    private TileGrid tileGrid;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGrid(TileGrid grid)
    {
        tileGrid = grid;
        GetComponent<MeshFilter>().mesh = grid.GenerateMesh();
    }
}
