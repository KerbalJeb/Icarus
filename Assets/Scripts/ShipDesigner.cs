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
    [SerializeField] private TextList       shipSelector;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private SpriteRenderer previewImg;
    [SerializeField] private Color          addingColor   = Color.white;
    [SerializeField] private Color          removingColor = Color.white;
    [SerializeField] private Sprite         blankTile;
    private                  bool           activelyPlacing;
    private                  string         activeTileID = "default_hull";


    private string     currentTileID = "default_hull";
    private string     designName    = "Unnamed Ship";
    private Directions direction     = Directions.Up;
    private bool       placingBlocks;
    private Transform  previewImgTransform;
    private string     shipSavePath;
    private TileSet    tileSet;

    public string CurrentTileID
    {
        get => currentTileID;
        set
        {
            ChangeMode(value != "null");
            currentTileID   = value;
            activelyPlacing = true;
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
            designName.Remove(designName.Length - 5);
        }
    }

    private void Update()
    {
        if (!activelyPlacing) return;
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
    }

    private void OnDisable()
    {
        InputManager.PlayerActions.PlaceBlock.performed      -= StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed     -= DeleteBlocks;
        InputManager.PlayerActions.PlaceBlock.canceled       -= StopPlacingBlocks;
        InputManager.PlayerActions.RotateTileLeft.performed  -= RotateLeft;
        InputManager.PlayerActions.RotateTileRight.performed -= RotateRight;
        InputManager.PlayerActions.Escape.performed          -= DisablePlacing;
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

    public void StartSave()
    {
        inputField.text = designName;
    }

    public void UpdateName(string newName)
    {
        designName = newName;
    }

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
        if (shipName == null) return;

        designName = shipName;
        string filePath = shipSavePath + "/" + shipName + ".json";
        ShipData.ShipPath = filePath;
        tileManager.LoadFromJson(filePath);
    }

    public void LoadShipTester()
    {
        if (ShipData.ShipPath is null) return;
        TrySave(true);
        SceneManager.LoadScene("Scenes/ShipTest");
    }

    private void RotateLeft(InputAction.CallbackContext context)
    {
        if (direction == Directions.Right)
            direction = Directions.Up;
        else
            direction++;
    }

    private void RotateRight(InputAction.CallbackContext context)
    {
        if (direction == Directions.Up)
            direction = Directions.Right;
        else
            direction--;
    }

    private void DeleteBlocks(InputAction.CallbackContext context)
    {
        ChangeMode(currentTileID == "null");
    }

    private void DisablePlacing(InputAction.CallbackContext context)
    {
        activelyPlacing   = false;
        previewImg.sprite = null;
    }
}
