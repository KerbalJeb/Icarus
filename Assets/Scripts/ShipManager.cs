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
    public  TileManager     TileManager { get; private set; }
    public  Rigidbody2D     Rigidbody2D { get; private set; }

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
        get => TileManager.PhysicsEnabled;
    }


    private void Awake()
    {
        cam             = Camera.main;
        TileManager     = GetComponent<TileManager>();
        movementManager = GetComponent<MovementManager>();
        Rigidbody2D     = GetComponent<Rigidbody2D>();
        PhysicsEnabled  = TileManager.PhysicsEnabled;
    }

    private void Update()
    {
        if (!PhysicsEnabled)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                Vector2Control mousePos = Mouse.current.position;
                Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
                TileManager.SetTile(worldPos, TileManager.TileSet.VariantNameToID["Basic Armor"]);
            }
            else if (Mouse.current.middleButton.isPressed)
            {
                Vector2Control mousePos = Mouse.current.position;
                Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
                TileManager.RemoveTile(worldPos);
            }
        }
    }

    public void SteerShip(InputAction.CallbackContext ctx)
    {
        var thrust = ctx.ReadValue<Vector3>();
        movementManager.Steer(thrust);
    }

    public void UpdatePhysics()
    {
        TileManager.PhysicsEnabled = true;
        movementManager.UpdatePhysics();
    }

    public void Test(InputAction.CallbackContext ctx)
    {
        if (!PhysicsEnabled)
        {
            UpdatePhysics();
        }
    }
}
