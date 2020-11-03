using System.Collections.Generic;
using TileSystem;
using TileSystem.TileVariants;
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
    [SerializeField] private GameObject      template =null;
    private                  Camera          cam;
    private                  MovementManager movementManager;
    private                  bool            physics = false;
    private                  Rigidbody2D     rb2D;
    private                  TileManager     tileManager;

    /// <value>
    ///     Will enable or disable physics for this tilemap (Splits tile map on enable)
    /// </value>
    public bool PhysicsEnabled
    {
        set
        {
            physics          = value;
            rb2D.isKinematic = !value;
            if (!value) return;
            Split();           
            movementManager.UpdatePhysics();
        }
        get => physics;
    }


    private void Awake()
    {
        cam               =  Camera.main;
        tileManager       =  GetComponent<TileManager>();
        movementManager   =  GetComponent<MovementManager>();
        rb2D              =  GetComponent<Rigidbody2D>();
        rb2D              =  GetComponent<Rigidbody2D>();
        rb2D.isKinematic  =  true;
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
        Split();
        movementManager.UpdatePhysics();
    }

    public void Test(InputAction.CallbackContext ctx)
    {
        if (!PhysicsEnabled)
        {
            PhysicsEnabled = true;
        }
    }

    /// <summary>
    ///     Splits the tilemap into multiple objects if there are unconnected reagons
    /// </summary>
    private void Split()
    {
        var islands = tileManager.FindIslands();
        if (islands.Count <= 1) return;

        foreach (var island in islands)
        {
            GameObject obj            = Instantiate(template);
            var        newTileManager = obj.GetComponent<TileManager>();
            var        newShipManager = obj.GetComponent<ShipManager>();
            var        newRb2D        = obj.GetComponent<Rigidbody2D>();

            newTileManager.ResetTiles();

            Transform thisTransform = transform;
            Transform tileTransform = newTileManager.transform;

            tileTransform.position = thisTransform.position;
            tileTransform.rotation = thisTransform.rotation;

            var structuralTiles = new List<StructuralTileData>();
            var functionalTiles = new List<FunctionalTileData>();
            var functionalCords = new List<Vector3Int>();

            foreach (Vector3Int cords in island)
            {
                if (!tileManager.GetTile(cords, out StructuralTileData stcTile)) continue;
                structuralTiles.Add(stcTile);

                if (!tileManager.GetTile(cords, out FunctionalTileData funTile)) continue;
                functionalTiles.Add(funTile);
                functionalCords.Add(cords);
            }

            newTileManager.SetFunctionalTiles(functionalCords.ToArray(), functionalTiles.ToArray());
            newTileManager.SetStructuralTiles(island.ToArray(), structuralTiles.ToArray());
            if (!PhysicsEnabled) continue;

            newShipManager.PhysicsEnabled = true;
            newRb2D.velocity              = rb2D.velocity;
            newRb2D.angularVelocity       = rb2D.angularVelocity;
        }

        Destroy(gameObject);
    }
}
