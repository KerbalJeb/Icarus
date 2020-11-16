using System;
using System.Collections.Generic;
using System.Linq;
using TileSystem;
using UnityEngine;
using Object = UnityEngine.Object;

public class WeaponsManager
{
    private Dictionary<Vector3Int, (Weapon weapon, GameObject gameObject)> weapons = new Dictionary<Vector3Int, (Weapon, GameObject)>();
    private TileManager tileManager;

    public WeaponsManager(TileManager manager)
    {
        tileManager = manager;
    }
    
    public void UpdateTransform(Transform target)
    {
        var targetPos = target.position;
        foreach (var valueTuple in weapons)
        {
            var turret = valueTuple.Value.gameObject.transform;
            var rot    = Quaternion.LookRotation(Vector3.forward, targetPos - turret.position);
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
            var (w, o) = valueTuple.Value;
            var dir      = o.transform.rotation;
            var startPos = o.transform.position;
            var endPos   = dir * Vector3.forward * w.range;
            var dmg      = new Damage(startPos, endPos, w.baseDamage);
            dmg.ApplyDamage();
        }
    }
}

