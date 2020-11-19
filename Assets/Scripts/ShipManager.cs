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
    /// <value>
    ///     The target to aim the weapons at
    /// </value>
    public Transform target;

    /// <value>
    ///     If true will load a ship from the path given in ShipData.Path
    /// </value>
    public bool autoLoadFromShipData;

    /// <value>
    ///     If physics should be enabled when the object if first created
    /// </value>
    [SerializeField] private bool startWithPhysics;

    private TileManager tileManager;

    /// <value>
    ///     The MovementManager for this ship
    /// </value>
    public MovementManager MovementManager { get; private set; }

    /// <value>
    ///     The WeaponsManager for this ship
    /// </value>
    public WeaponsManager WeaponsManager { get; private set; }

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
        MovementManager = new MovementManager(tileManager, transform);
    }

    private void Start()
    {
        if (autoLoadFromShipData) tileManager.LoadFromJson(ShipData.ShipPath);
        PhysicsEnabled = startWithPhysics;
    }

    private void Update()
    {
        if (!(target is null) && PhysicsEnabled) WeaponsManager.UpdateTransform(target);
    }

    private void FixedUpdate()
    {
        if (PhysicsEnabled) MovementManager.ApplyThrust();
    }

    private void OnEnable()
    {
        tileManager.UpdatePhysics                 += UpdatePhysics;
        InputManager.PlayerActions.Fire.performed += StartFiring;
        InputManager.PlayerActions.Fire.canceled  += StopFiring;
        InputManager.PlayerActions.Move.performed += SteerShip;
    }

    private void OnDisable()
    {
        tileManager.UpdatePhysics                 -= UpdatePhysics;
        InputManager.PlayerActions.Fire.performed -= StartFiring;
        InputManager.PlayerActions.Fire.canceled  -= StopFiring;
        InputManager.PlayerActions.Move.performed -= SteerShip;
    }

    /// <summary>
    ///     Callback function used by the input system to control the ship
    /// </summary>
    /// <param name="ctx">From input system</param>
    private void SteerShip(InputAction.CallbackContext ctx)
    {
        var thrust = ctx.ReadValue<Vector3>();
        MovementManager.Steer(thrust);
    }

    /// <summary>
    ///     Will update the physics model for the ship, automatically called when a tile is destroyed
    /// </summary>
    private void UpdatePhysics()
    {
    }

    /// <summary>
    ///     Fires all the weapons attached to the ship
    /// </summary>
    /// <param name="context">Needed for input system to work, but not used in function</param>
    private void StartFiring(InputAction.CallbackContext context)
    {
        if (!PhysicsEnabled || InputManager.IsMouseOverClickableUI()) return;
        WeaponsManager.Firing = true;
        foreach (Vector3Int pos in WeaponsManager.WeaponPos) StartCoroutine(WeaponsManager.FireRoutine(pos));
    }

    /// <summary>
    ///     Stops weapons from firing
    /// </summary>
    /// <param name="context">Needed for input system to work, but not used in function</param>
    private void StopFiring(InputAction.CallbackContext context)
    {
        WeaponsManager.Firing = false;
    }
}
