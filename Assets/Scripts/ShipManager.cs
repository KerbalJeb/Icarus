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
    private          Camera          cam;
    private          MovementManager movementManager;
    private readonly Transform       target = null;
    private          TileManager     tileManager;
    public           WeaponsManager  WeaponsManager { get; private set; }

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
        WeaponsManager  = new WeaponsManager(tileManager);
    }

    private void Start()
    {
        tileManager.UpdatePhysics += UpdatePhysics;
    }


    private void Update()
    {
        if (!(target is null)) WeaponsManager.UpdateTransform(target);
    }

    private void OnEnable()
    {
        InputManager.PlayerActions.Move.performed       += SteerShip;
        InputManager.PlayerActions.UpdateMesh.performed += Test;
    }

    private void OnDisable()
    {
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
        movementManager.UpdatePhysics();
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
