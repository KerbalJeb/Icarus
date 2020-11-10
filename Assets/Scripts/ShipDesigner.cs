using System;
using TileSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


public class ShipDesigner : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private Camera      cam;
    private                  bool        placingBlocks;
    private                  bool        removingBlocks;

    public string CurrentTileID { get; set; } = "default_hull";

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

    private void Update()
    {
        if (placingBlocks)
        {
            Vector2Control mousePos = Mouse.current.position;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            tileManager.SetTile(worldPos, tileManager.TileSet.VariantNameToID[CurrentTileID]);
        }
        else if (removingBlocks)
        {
            Vector2Control mousePos = Mouse.current.position;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            tileManager.RemoveTile(worldPos);
        }
    }
    
    private void StartPlacingBlocks(InputAction.CallbackContext context)
    {
        if (InputManager.IsMouseOverClickableUI())
        {
            return;
        }
        placingBlocks = true;
    }
    private void StopPlacingBlocks(InputAction.CallbackContext context)
    {
        placingBlocks = false;
    }
    private void StartDeletingBlocks(InputAction.CallbackContext context)
    {
        if (InputManager.IsMouseOverClickableUI())
        {
            return;
        }
        removingBlocks = true;
    }    
    private void StopDeletingBlocks(InputAction.CallbackContext context)
    {
        removingBlocks = false;
    }
}

