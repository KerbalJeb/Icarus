using System;
using TileSystem;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///     Used to connect all components needed for a ship
/// </summary>
[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(TileManager))]
public class ShipManager : MonoBehaviour
{
    private                  MovementManager movementManager;
    private readonly         Transform       target = null;
    private                  TileManager     tileManager;
    public                   WeaponsManager  WeaponsManager { get; private set; }
    public                   bool            autoLoadFromShipData = false;
    [SerializeField] private bool            startWithPhysics     = false;

    /// <value>
    ///     Will enable or disable physics for this ship
    /// </value>
    public bool PhysicsEnabled
    {
        set
        {
            tileManager.PhysicsEnabled = value;
            if (!value) return;
            UpdatePhysics();
        }
        get => tileManager.PhysicsEnabled;
    }

    private void Awake()
    {
        tileManager     = GetComponent<TileManager>();
        WeaponsManager  = new WeaponsManager(tileManager);
        movementManager = new MovementManager(tileManager, transform);
    }

    private void Start()
    {
        if (autoLoadFromShipData)
        {
            tileManager.LoadFromJson(ShipData.ShipName);
        }
        PhysicsEnabled = startWithPhysics;
    }

    private void Update()
    {
        if (!(target is null)) WeaponsManager.UpdateTransform(target);
    }

    private void FixedUpdate()
    {
        if (PhysicsEnabled)
        {
            movementManager.ApplyThrust();
        }
    }

    private void OnEnable()
    {
        tileManager.UpdatePhysics                       += UpdatePhysics;
        InputManager.PlayerActions.Move.performed       += SteerShip;
        InputManager.PlayerActions.UpdateMesh.performed += Test;
    }

    private void OnDisable()
    {
        tileManager.UpdatePhysics                       -= UpdatePhysics;
        InputManager.PlayerActions.Move.performed       -= SteerShip;
        InputManager.PlayerActions.UpdateMesh.performed -= Test;
    }

    /// <summary>
    ///     Callback function used by the input system to control the ship
    /// </summary>
    /// <param name="ctx">From input system</param>
    public void SteerShip(InputAction.CallbackContext ctx)
    {
        var thrust = ctx.ReadValue<Vector3>();
        movementManager.Steer(thrust);
    }

    /// <summary>
    ///     Will update the physics model for the ship, automatically called when a tile is destroyed
    /// </summary>
    public void UpdatePhysics()
    {
        if (tileManager.PhysicsEnabled)
        {
            movementManager.UpdatePhysics();
        }
    }

    /// <summary>
    ///     Enables physics
    /// </summary>
    /// <param name="ctx"></param>
    private void Test(InputAction.CallbackContext ctx)
    {
        if (!PhysicsEnabled)
        {
            UpdatePhysics();
            tileManager.PhysicsEnabled = true;
        }
    }
}
