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
        PhysicsEnabled  = tileManager.PhysicsEnabled;
    }

    private void Update()
    {
        if (!PhysicsEnabled)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                Vector2Control mousePos = Mouse.current.position;
                Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
                tileManager.SetTile(worldPos, tileManager.TileSet.VariantNameToID["Basic Armor"]);
            }
            else if (Mouse.current.middleButton.isPressed)
            {
                Vector2Control mousePos = Mouse.current.position;
                Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
                tileManager.RemoveTile(worldPos);
            }
        }
    }

    public void ApplyDamage(Damage dmg)
    {
        if (!PhysicsEnabled) return;
        if (!tileManager.ApplyDamage(dmg)) return;
        UpdatePhysics();
    }

    public void SteerShip(InputAction.CallbackContext ctx)
    {
        var thrust = ctx.ReadValue<Vector3>();
        movementManager.Steer(thrust);
    }

    private void UpdatePhysics()
    {
        movementManager.UpdatePhysics();
    }

    public void Test(InputAction.CallbackContext ctx)
    {
        if (!PhysicsEnabled)
        {
            PhysicsEnabled             = true;
            tileManager.PhysicsEnabled = true;
        }
    }
}
