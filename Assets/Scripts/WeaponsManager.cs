using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TileSystem;
using UnityEngine;

// TODO add max rotation speed to turrets
// TODO add time of flight projectiles
// TODO add fire groups

/// <summary>
///     Used to manage weapons on a ship
/// </summary>
public class WeaponsManager
{
    private readonly TileManager                  tileManager;
    private readonly Dictionary<Vector3Int, bool> weaponCoroutineStatues = new Dictionary<Vector3Int, bool>();
    private readonly List<Vector3Int>             weaponPos              = new List<Vector3Int>();

    private readonly Dictionary<Vector3Int, (Weapon weapon, GameObject gameObject, WeaponFx fx)> weapons =
        new Dictionary<Vector3Int, (Weapon weapon, GameObject gameObject, WeaponFx fx)>();

    /// <value>
    ///     If the weapons are currently firing, if set all active weapon coroutines will fire their weapons
    /// </value>
    public bool Firing = false;


    /// <summary>
    ///     The constructor for the weapons manager
    /// </summary>
    /// <param name="manager">The tile manager it is attached to</param>
    public WeaponsManager(TileManager manager)
    {
        tileManager = manager;
    }

    public ReadOnlyCollection<Vector3Int> WeaponPos => weaponPos.AsReadOnly();

    /// <summary>
    ///     Updates the rotations of the weapons turrets
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
    ///     Registers a new weapon wi
    /// </summary>
    /// <param name="cords">The coordinates of the weapon in the tile map</param>
    /// <param name="weapon">The weapon variant</param>
    /// <param name="turret">The turret GameObject</param>
    public void AddWeapon(Vector3Int cords, Weapon weapon, GameObject turret)
    {
        weapons[cords]                = (weapon, turret, turret.GetComponent<WeaponFx>());
        weaponCoroutineStatues[cords] = false;
        weaponPos.Add(cords);
    }

    /// <summary>
    ///     De-Registers a weapon
    /// </summary>
    /// <param name="cords">The coordinates of the weapon in the tile map</param>
    public void RemoveWeapon(Vector3Int cords)
    {
        foreach (var pair in weapons.Where(pair => pair.Key == cords))
        {
            Object.Destroy(pair.Value.gameObject);
            weapons.Remove(cords);
            weaponCoroutineStatues.Remove(cords);
            weaponPos.Remove(cords);
            return;
        }
    }

    /// <summary>
    ///     A coroutine that is used to fire a weapon, will continue firing while the Firing property is true
    /// </summary>
    /// <param name="cords">The position of the weapon</param>
    /// <returns>Delay for coroutine</returns>
    public IEnumerator FireRoutine(Vector3Int cords)
    {
        if (weaponCoroutineStatues[cords]) yield break;

        (Weapon w, GameObject o, WeaponFx fx) = weapons[cords];
        weaponCoroutineStatues[cords]         = true;
        while (Firing)
        {
            Quaternion dir      = o.transform.rotation;
            Vector3    startPos = o.transform.position;
            Vector3    endPos   = startPos + dir * Vector3.up * w.range;
            var        dmg      = new Damage(startPos, endPos, w.baseDamage);
            (Vector3 hitPos, Vector3 endHitPos) = dmg.ApplyDamage(new[] {tileManager});
            fx.ApplyFX(startPos, endHitPos, hitPos);
            yield return new WaitForSeconds(w.firePeriod);
        }

        weaponCoroutineStatues[cords] = false;
    }
}
