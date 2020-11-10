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

    /// <value>
    ///     Will enable or disable physics for this ship
    /// </value>
    public bool PhysicsEnabled
    {
        set
        {
            if (!value) return;
            UpdatePhysics();
        }
        get => tileManager.PhysicsEnabled;
    }

    private void Awake()
    {
        cam             = Camera.main;
        tileManager     = GetComponent<TileManager>();
        movementManager = GetComponent<MovementManager>();
        PhysicsEnabled  = tileManager.PhysicsEnabled;
    }

    private void OnEnable()
    {
        InputManager.PlayerActions.Move.performed        += SteerShip;
        InputManager.PlayerActions.UpdateMesh.performed  += Test;
    }

    private void OnDisable()
    {
        InputManager.PlayerActions.Move.performed        -= SteerShip;
        InputManager.PlayerActions.UpdateMesh.performed  -= Test;
    }
    private void Start()
    {
        tileManager.UpdatePhysics += UpdatePhysics;
    }

    public void SteerShip(InputAction.CallbackContext ctx)
    {
        var thrust = ctx.ReadValue<Vector3>();
        movementManager.Steer(thrust);
    }

    public void UpdatePhysics()
    {
        movementManager.UpdatePhysics();
    }

    private void Test(InputAction.CallbackContext ctx)
    {
        if (!PhysicsEnabled)
        {
            UpdatePhysics();
            tileManager.PhysicsEnabled = true;
        }
    }
}
