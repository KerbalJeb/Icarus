using TileSystem;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Grid))]
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
                var mousePos = Mouse.current.position;
                var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
                tileManager.SetTile(worldPos, tileManager.TileSet.StructuralTiles[0]);
            }
            else if (Mouse.current.middleButton.isPressed)
            {
                var mousePos = Mouse.current.position;
                var worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
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
