using TileSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Used to connect all components needed for a ship
/// </summary>
[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(TileManager))]
public class ShipManager : MonoBehaviour
{
    private Camera      cam;
    private TileManager tileManager;

    private void Awake()
    {
        cam         = Camera.main;
        tileManager = GetComponent<TileManager>();
    }

    private void Update()
    {
        if (!tileManager.PhysicsEnabled)
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
        tileManager.ApplyDamage(dmg);
    }

    public void Test(InputAction.CallbackContext ctx)
    {
        tileManager.PhysicsEnabled = true;
    }
}
