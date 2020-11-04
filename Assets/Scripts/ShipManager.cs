using System;
using TileSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
///     Used to connect all components needed for a ship
/// </summary>
[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(TileManager))]
public class ShipManager : MonoBehaviour
{
    private Camera          cam;
    private MovementManager movementManager;
    private TileManager     tileManager;
    private bool            placingBlocks  =false;
    private bool            removingBlocks = false;

    /// <value>
    ///     Will enable or disable physics for this ship
    /// </value>
    public bool PhysicsEnabled
    {
        set
        {
            if (!value) return;
            tileManager.PhysicsEnabled = transform;
            UpdatePhysics();
        }
        get => tileManager.PhysicsEnabled;
    }


    private void Awake()
    {
        cam             = Camera.main;
        tileManager     = GetComponent<TileManager>();
        movementManager = GetComponent<MovementManager>();
    }

    private void Start()
    {
        PhysicsEnabled = tileManager.PhysicsEnabled;
    }

    private void OnEnable()
    {
        InputManager.PlayerActions.Move.performed        += SteerShip;
        InputManager.PlayerActions.UpdateMesh.performed  += Test;
        InputManager.PlayerActions.PlaceBlock.performed  += StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed += StartDeletingBlocks;        
        InputManager.PlayerActions.PlaceBlock.canceled   += StopPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.canceled  += StopDeletingBlocks;
    }

    private void OnDisable()
    {
        InputManager.PlayerActions.Move.performed        -= SteerShip;
        InputManager.PlayerActions.UpdateMesh.performed  -= Test;
        InputManager.PlayerActions.PlaceBlock.performed  -= StartPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.performed -= StartDeletingBlocks;        
        InputManager.PlayerActions.PlaceBlock.canceled   -= StopPlacingBlocks;
        InputManager.PlayerActions.DeleteBlock.canceled  -= StopDeletingBlocks;
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

    private void Update()
    {
        if (PhysicsEnabled) return;
        if (placingBlocks)
        {
            Vector2Control mousePos = Mouse.current.position;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            tileManager.SetTile(worldPos, tileManager.TileSet.VariantNameToID["Basic Armor"]);
        }
        else if (removingBlocks)
        {
            Vector2Control mousePos = Mouse.current.position;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            tileManager.RemoveTile(worldPos);
        }
    }

    public void ApplyDamage(Damage dmg)
    {
        if (!PhysicsEnabled) return;
        if (!tileManager.ApplyDamage(dmg)) return;
        UpdatePhysics();
    }

    private void SteerShip(InputAction.CallbackContext ctx)
    {
        var thrust = ctx.ReadValue<Vector3>();
        movementManager.Steer(thrust);
    }

    private void UpdatePhysics()
    {
        movementManager.UpdatePhysics();
    }

    private void Test(InputAction.CallbackContext ctx)
    {
        if (!PhysicsEnabled)
        {
            PhysicsEnabled             = true;
            tileManager.PhysicsEnabled = true;
        }
    }
}
