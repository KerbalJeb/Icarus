using System.IO;
using TileSystem;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

/// <summary>
///     A class used to manage drawing tiles in ship design mode
/// </summary>
public class ShipDesigner : MonoBehaviour
{
    // todo Add line and box drawing tools
    [SerializeField] private TileManager    tileManager;
    [SerializeField] private Camera         cam;
    [SerializeField] private PopUp          nameConflictPopUp;
    [SerializeField] private PopUp          savePopUp;
    [SerializeField] private PopUp          shipError;
    [SerializeField] private TextList       shipSelector;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private SpriteRenderer previewImg;
    [SerializeField] private Color          addingColor   = Color.white;
    [SerializeField] private Color          removingColor = Color.white;
    [SerializeField] private Sprite         blankTile;
    [SerializeField] private TileSelector   tileSelector;

    private string    activeTileID  = "default_hull";
    private string    currentTileID = "default_hull";
    private string    designName    = "Unnamed Ship";
    private Direction direction     = Direction.Up;
    private bool      placingBlocks;
    private Transform previewImgTransform;
    private string    shipSavePath;
    private TileSet   tileSet;

    private bool ActivelyPlacingBlocks { get; set; }


    /// <value>
    ///     The string id of the tile currently being placed
    /// </value>
    public string CurrentTileID
    {
        get => currentTileID;
        set
        {
            ChangeMode(value != "null");
            currentTileID         = value;
            ActivelyPlacingBlocks = true;
        }
    }

    private void Awake()
    {
        tileSet             = TileSet.Instance;
        shipSavePath        = Application.persistentDataPath + "/ships";
        previewImgTransform = previewImg.transform;
        previewImg.color    = addingColor;
        if (!Directory.Exists(shipSavePath)) Directory.CreateDirectory(shipSavePath);
    }

    private void Start()
    {
        if (ShipData.ShipPath != null)
        {
            tileManager.LoadFromJson(ShipData.ShipPath);
            designName = Path.GetFileName(ShipData.ShipPath);
            designName = designName.Remove(designName.Length - 5);
        }
    }

    private void Update()
    {
        if (!ActivelyPlacingBlocks) return;
        Vector2Control mousePos = Mouse.current.position;
        Vector3        worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
        Vector3Int     cords    = tileManager.PositionToCords(worldPos);


        if (placingBlocks) tileManager.SetTile(cords, tileSet.VariantNameToID[CurrentTileID], direction);

        if (!InputManager.IsMouseOverClickableUI())
        {
            Sprite tile = tileSet.VariantNameToID[CurrentTileID] == null
                ? blankTile
                : tileSet.VariantNameToID[CurrentTileID].previewImg;
            Vector3 pos = tileManager.CordsToPosition(cords);
            previewImg.sprite = tile;

            previewImgTransform.position = pos;
            previewImgTransform.rotation = TileInfo.TransformMatrix[direction].rotation;
        }
        else
            previewImg.sprite = null;
    }

    private void OnEnable()
    {
        InputManager.PlayerActions.PlaceBlock.performed      += StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed     += DeleteBlocks;
        InputManager.PlayerActions.PlaceBlock.canceled       += StopPlacingBlocks;
        InputManager.PlayerActions.RotateTileLeft.performed  += RotateLeft;
        InputManager.PlayerActions.RotateTileRight.performed += RotateRight;
        InputManager.PlayerActions.Escape.performed          += DisablePlacing;
        InputManager.PlayerActions.CancelPlace.performed     += DisablePlacing;
        tileSelector.ResetButtons();
    }

    private void OnDisable()
    {
        InputManager.PlayerActions.PlaceBlock.performed      -= StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed     -= DeleteBlocks;
        InputManager.PlayerActions.PlaceBlock.canceled       -= StopPlacingBlocks;
        InputManager.PlayerActions.RotateTileLeft.performed  -= RotateLeft;
        InputManager.PlayerActions.RotateTileRight.performed -= RotateRight;
        InputManager.PlayerActions.Escape.performed          -= DisablePlacing;
        InputManager.PlayerActions.CancelPlace.performed     -= DisablePlacing;
    }

    private void ChangeMode(bool adding)
    {
        previewImg.color = adding ? addingColor : removingColor;
        if (!adding)
        {
            activeTileID  = currentTileID;
            currentTileID = "null";
        }
        else
            currentTileID = activeTileID;
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

    /// <summary>
    ///     Updates the save popup with the correct name
    /// </summary>
    public void StartSave()
    {
        inputField.text = designName;
    }

    /// <summary>
    ///     Updates the name of the current ship
    /// </summary>
    /// <param name="newName">The new name of the ship</param>
    public void UpdateName(string newName)
    {
        designName = newName;
    }

    /// <summary>
    ///     Saves the ship design and warns about overwriting name conflicts
    /// </summary>
    /// <param name="overwrite">will overwrite without prompt if true</param>
    public void TrySave(bool overwrite = false)
    {
        string filePath = shipSavePath + "/" + designName + ".json";
        if (File.Exists(filePath) && !overwrite)
        {
            nameConflictPopUp.Open();
            return;
        }

        string jsonData = tileManager.DesignToJson();
        ShipData.ShipPath = filePath;
        File.WriteAllText(filePath, jsonData);
        savePopUp.Close();
        nameConflictPopUp.Close();
    }

    /// <summary>
    ///     Displays the pop up that shows saved shps
    /// </summary>
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

    /// <summary>
    ///     Loads a ship design by name
    /// </summary>
    /// <param name="shipName">The name of the ship to loasd</param>
    public void LoadDesign(string shipName)
    {
        if (shipName == null) return;

        designName = shipName;
        string filePath = shipSavePath + "/" + shipName + ".json";
        ShipData.ShipPath = filePath;
        tileManager.LoadFromJson(filePath);
    }

    /// <summary>
    ///     Enter the flight test mode
    /// </summary>
    public void LoadShipTester()
    {
        //to do: add pop-up prompting to save an unsaved ship to allow testing of ship
        TrySave(true);
        if (ShipData.ShipPath is null) return;
        if (tileManager.FindIslands().Count > 1)
        {
            shipError.Open();
            return;
        }

        SceneManager.LoadScene("Scenes/ShipTest");
    }

    private void RotateLeft(InputAction.CallbackContext context)
    {
        if (direction == Direction.Right)
            direction = Direction.Up;
        else
            direction++;
    }

    private void RotateRight(InputAction.CallbackContext context)
    {
        if (direction == Direction.Up)
            direction = Direction.Right;
        else
            direction--;
    }

    private void DeleteBlocks(InputAction.CallbackContext context)
    {
        if (ActivelyPlacingBlocks)
            ChangeMode(currentTileID == "null");
        else
            ChangeMode(false);
        ActivelyPlacingBlocks = true;
    }

    private void DisablePlacing(InputAction.CallbackContext context)
    {
        ActivelyPlacingBlocks = false;
        previewImg.sprite     = null;
        tileSelector.ResetButtons();
    }
}
