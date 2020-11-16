using System;
using System.IO;
using TileSystem;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
///     A class used to manage drawing tiles in ship design mode
/// </summary>
public class ShipDesigner : MonoBehaviour
{
    // todo Add preview of blocks to be placed
    // todo Add saving method
    // todo Add line and box drawing tools
    // todo Change deleting blocks to hotkey instead of middle mouse
    [SerializeField] private TileManager    tileManager =null;
    [SerializeField] private Camera         cam         =null;
    [SerializeField] private PopUp          nameConflictPopUp;
    [SerializeField] private PopUp          savePopUp;
    [SerializeField] private TextList       shipSelector;
    private                  bool           placingBlocks;
    private                  bool           removingBlocks; private TileSet tileSet;
    [SerializeField] private TMP_InputField inputField;
    private const            string         DefaultText = "Enter Ship Name..";
    private                  string         shipSavePath;
    

    public string CurrentTileID { get; set; } = "default_hull";

    private void Awake()
    {
        tileSet      = TileSet.Instance;
        shipSavePath = Application.persistentDataPath + "/ships";
    }

    private void Update()
    {
        if (placingBlocks)
        {
            Vector2Control mousePos = Mouse.current.position;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            tileManager.SetTile(worldPos, tileSet.VariantNameToID[CurrentTileID]);
        }
        else if (removingBlocks)
        {
            Vector2Control mousePos = Mouse.current.position;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            tileManager.RemoveTile(worldPos);
        }
    }

    private void OnEnable()
    {
        InputManager.PlayerActions.PlaceBlock.performed  += StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed += StartDeletingBlocks;
        InputManager.PlayerActions.PlaceBlock.canceled   += StopPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.canceled  += StopDeletingBlocks;
    }

    private void OnDisable()
    {
        InputManager.PlayerActions.PlaceBlock.performed  -= StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed -= StartDeletingBlocks;
        InputManager.PlayerActions.PlaceBlock.canceled   -= StopPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.canceled  -= StopDeletingBlocks;
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

    private void StartDeletingBlocks(InputAction.CallbackContext context)
    {
        // Stop 'click through' on UI
        if (InputManager.IsMouseOverClickableUI()) return;
        removingBlocks = true;
    }

    private void StopDeletingBlocks(InputAction.CallbackContext context)
    {
        removingBlocks = false;
    }

    public void StartSave()
    {
        inputField.text = DefaultText;
    }

    public void TrySave(bool overwrite = false)
    {
        var filePath = shipSavePath + "/" + inputField.text + ".json";
        if (!Directory.Exists(shipSavePath))
        {
            Directory.CreateDirectory(shipSavePath);
        }
        if (File.Exists(filePath) && !overwrite)
        {
            nameConflictPopUp.Open();
            return;
        }

        var jsonData = tileManager.DesignToJson();
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
            var shipName = Path.GetFileName(ship);
            shipSelector.AddElement(shipName.Remove(shipName.Length-5), this);
        }
    }

    public void LoadDesign(string shipName)
    {
        var filePath = shipSavePath + "/" + shipName + ".json";
        tileManager.LoadFromJson(filePath);
    }
}
