using System.Collections.Generic;
using System.Linq;
using TileSystem;
using UnityEngine;

// TODO add max rotation speed to turrets
// TODO add time of flight projectiles
// TODO add fire groups

/// <summary>
/// Used to manage weapons on a ship
/// </summary>
public class WeaponsManager
{
    private readonly TileManager tileManager;

    private readonly Dictionary<Vector3Int, (Weapon weapon, GameObject gameObject)> weapons =
        new Dictionary<Vector3Int, (Weapon, GameObject)>();

    /// <summary>
    /// The constructor for the weapons manager
    /// </summary>
    /// <param name="manager">The tile manager it is attached to</param>
    public WeaponsManager(TileManager manager)
    {
        tileManager = manager;
    }

    /// <summary>
    /// Updates the rotations of the weapons turrets
    /// </summary>
    /// <param name="target">The target to aim towards</param>
    public void UpdateTransform(Transform target)
    {
        Vector3 targetPos = target.position;
        foreach (var valueTuple in weapons)
        {
            Transform  turret = valueTuple.Value.gameObject.transform;
            Quaternion rot    = Quaternion.LookRotation(Vector3.forward, targetPos - turret.position);
            turret.rotation = rot;
        }
    }

    /// <summary>
    /// Registers a new weapon wi
    /// </summary>
    /// <param name="cords">The coordinates of the weapon in the tile map</param>
    /// <param name="weapon">The weapon variant</param>
    /// <param name="turret">The turret GameObject</param>
    public void AddWeapon(Vector3Int cords, Weapon weapon, GameObject turret)
    {
        weapons[cords] = (weapon, turret);
    }

    /// <summary>
    /// De-Registers a weapon
    /// </summary>
    /// <param name="cords">The coordinates of the weapon in the tile map</param>
    public void RemoveWeapon(Vector3Int cords)
    {
        foreach (var pair in weapons.Where(pair => pair.Key == cords))
        {
            Object.Destroy(pair.Value.gameObject);
            weapons.Remove(cords);
            return;
        }
    }

    /// <summary>
    /// Fires all weapons in the direction it is currently facing
    /// </summary>
    public void Fire()
    {
        foreach (var valueTuple in weapons)
        {
            (Weapon w, GameObject o) = valueTuple.Value;
            Quaternion dir      = o.transform.rotation;
            Vector3    startPos = o.transform.position;
            Vector3    endPos   = startPos + dir * Vector3.up * w.range;
            var        dmg      = new Damage(startPos, endPos, w.baseDamage);
            dmg.ApplyDamage(new []{tileManager});
        }
    }
}
