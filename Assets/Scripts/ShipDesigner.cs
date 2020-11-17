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
    private const string DefaultText = "Enter Ship Name..";

    // todo Add line and box drawing tools
    [SerializeField] private TileManager    tileManager = null;
    [SerializeField] private Camera         cam         = null;
    [SerializeField] private PopUp          nameConflictPopUp;
    [SerializeField] private PopUp          savePopUp;
    [SerializeField] private TextList       shipSelector;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private SpriteRenderer previewImg;
    [SerializeField] private Color          addingColor;
    [SerializeField] private Color          removingColor;
    [SerializeField] private Sprite         blankTile;
    private                  string         activeTileID = "default_hull";


    private string     currentTileID = "default_hull";
    private Directions direction     = Directions.Up;
    private Vector3Int lastPos;
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
            currentTileID = value;
        }
    }

    private void Awake()
    {
        tileSet             = TileSet.Instance;
        shipSavePath        = Application.persistentDataPath + "/ships";
        previewImgTransform = previewImg.transform;
        previewImg.color    = addingColor;
    }

    private void Update()
    {
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
    }

    private void OnDisable()
    {
        InputManager.PlayerActions.PlaceBlock.performed      -= StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed     -= DeleteBlocks;
        InputManager.PlayerActions.PlaceBlock.canceled       -= StopPlacingBlocks;
        InputManager.PlayerActions.RotateTileLeft.performed  -= RotateLeft;
        InputManager.PlayerActions.RotateTileRight.performed -= RotateRight;
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
}
