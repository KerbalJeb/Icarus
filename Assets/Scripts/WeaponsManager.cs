using System.Collections.Generic;
using System.Linq;
using TileSystem;
using UnityEngine;

public class WeaponsManager
{
    private TileManager tileManager;

    private readonly Dictionary<Vector3Int, (Weapon weapon, GameObject gameObject)> weapons =
        new Dictionary<Vector3Int, (Weapon, GameObject)>();

    public WeaponsManager(TileManager manager)
    {
        tileManager = manager;
    }

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

    public void AddWeapon(Vector3Int cords, Weapon weapon, GameObject turret)
    {
        weapons[cords] = (weapon, turret);
    }

    public void RemoveWeapon(Vector3Int cords)
    {
        foreach (var pair in weapons.Where(pair => pair.Key == cords))
        {
            Object.Destroy(pair.Value.gameObject);
            weapons.Remove(cords);
            return;
        }
    }

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
