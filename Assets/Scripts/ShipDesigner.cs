using System.IO;
using TileSystem;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
///     A class used to manage drawing tiles in ship design mode
/// </summary>
public class ShipDesigner : MonoBehaviour
{
    private const string DefaultText = "Enter Ship Name..";

    // todo Add preview of blocks to be placed
    // todo Add saving method
    // todo Add line and box drawing tools
    // todo Change deleting blocks to hotkey instead of middle mouse
    [SerializeField] private TileManager    tileManager = null;
    [SerializeField] private Camera         cam         = null;
    [SerializeField] private PopUp          nameConflictPopUp;
    [SerializeField] private PopUp          savePopUp;
    [SerializeField] private TextList       shipSelector;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Tilemap        previewMap;
    [SerializeField] private Color          addingColor;
    [SerializeField] private Color          removingColor;
    [SerializeField] private TileBase       blankTile;
    private                  Vector3Int     lastPos;
    private                  bool           placingBlocks;
    private                  string         shipSavePath;
    private                  TileSet        tileSet;
    private                  Directions     direction = Directions.Up;


    private string currentTileID = "default_hull";
    private string activeTileID  = "default_hull";

    public string CurrentTileID
    {
        get => currentTileID;
        set
        { 
            ChangeMode(value != "null");
            currentTileID = value;
        }
    }

    private void Awake()
    {
        tileSet      = TileSet.Instance;
        shipSavePath = Application.persistentDataPath + "/ships";
    }

    private void Update()
    {
        Vector2Control mousePos = Mouse.current.position;
        Vector3        worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
        Vector3Int cords = previewMap.WorldToCell(worldPos);
        
        if (placingBlocks)
        {
            tileManager.SetTile(cords, tileSet.VariantNameToID[CurrentTileID], direction);
        }

        if (!InputManager.IsMouseOverClickableUI())
        {
            TileBase tile;
            tile = tileSet.VariantNameToID[CurrentTileID] == null ? blankTile : tileSet.VariantNameToID[CurrentTileID].tile;
            previewMap.SetTile(cords, tile);
            previewMap.SetTransformMatrix(cords, TileInfo.TransformMatrix[direction]);
            if (cords == lastPos) return;
            previewMap.SetTile(lastPos, null);
            lastPos = cords;
        }
        else
        {
            previewMap.SetTile(lastPos, null);
        }


        
    }

    private void ChangeMode(bool adding)
    {
        previewMap.color = adding ? addingColor : removingColor;
        if (!adding)
        {
            activeTileID  = currentTileID;
            currentTileID = "null";
        }
        else
        {
            currentTileID = activeTileID;
        }
    }

    private void OnEnable()
    {
        InputManager.PlayerActions.PlaceBlock.performed      += StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed     += DeleteBlocks;
        InputManager.PlayerActions.PlaceBlock.canceled       += StopPlacingBlocks;
        InputManager.PlayerActions.RotateTileLeft.performed  += RotateLeft;
        InputManager.PlayerActions.RotateTileRight.performed += RotateRight;
    }

    private void OnDisable()
    {
        InputManager.PlayerActions.PlaceBlock.performed      -= StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed     -= DeleteBlocks;
        InputManager.PlayerActions.PlaceBlock.canceled       -= StopPlacingBlocks;
        InputManager.PlayerActions.RotateTileLeft.performed  -= RotateLeft;
        InputManager.PlayerActions.RotateTileRight.performed -= RotateRight;
    }

    private void StartPlacingBlocks(InputAction.CallbackContext context)
    {
        // Stop 'click through' on UI
        if (InputManager.IsMouseOverClickableUI()) return;
        placingBlocks = true;
    }

    private void StopPlacingBlocks(InputAction.CallbackContext context)
    {
        placingBlocks = false;
    }

    public void StartSave()
    {
        inputField.text = DefaultText;
    }

    public void TrySave(bool overwrite = false)
    {
        string designName = inputField.text;
        string filePath   = shipSavePath + "/" + designName + ".json";
        if (!Directory.Exists(shipSavePath)) Directory.CreateDirectory(shipSavePath);
        if (File.Exists(filePath) && !overwrite)
        {
            nameConflictPopUp.Open();
            return;
        }

        string jsonData = tileManager.DesignToJson();
        ShipData.ShipName = filePath;
        File.WriteAllText(filePath, jsonData);
        savePopUp.Close();
        nameConflictPopUp.Close();
    }

    public void ShowSavedShips()
    {
        var ships = Directory.GetFiles(shipSavePath, "*.json");
        shipSelector.Empty();
        foreach (string ship in ships)
        {
            string shipName = Path.GetFileName(ship);
            shipSelector.AddElement(shipName.Remove(shipName.Length - 5), this);
        }
    }

    public void LoadDesign(string shipName)
    {
        string filePath = shipSavePath + "/" + shipName + ".json";
        tileManager.LoadFromJson(filePath);
    }

    public void LoadShipTester()
    {
        TrySave(true);
        SceneManager.LoadScene("Scenes/ShipTest");
    }

    private void RotateLeft(InputAction.CallbackContext context)
    {
        if (direction==Directions.Right)
        {
            direction = Directions.Up;
        }
        else
        {
            direction++;
        }
    }

    private void RotateRight(InputAction.CallbackContext context)
    {
        if (direction==Directions.Up)
        {
            direction = Directions.Right;
        }
        else
        {
            direction--;
        }
    }

    private void DeleteBlocks(InputAction.CallbackContext context)
    {
        ChangeMode(currentTileID=="null");
    }
}
