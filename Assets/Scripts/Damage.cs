using System;
using TileSystem;
using UnityEngine;

/// <summary>
///     A basic class used to describe and apply damage to tilemaps
/// </summary>
public class Damage
{
    public           float          BaseDamage;
    public           Vector2        Direction;
    public           Vector2        EndPos;

    public Vector2 StartPos;

    /// <summary>
    ///     Basic constructor for damage, used to apply damage in a line. Will damage all tiles hit until the base damage has
    ///     been depleted (ie. if basedmg=100 and it hits a tile with 50hp and then a tile with 100hp it will destroy the first
    ///     tile and do 50 dmg to the second).
    /// </summary>
    /// <param name="startPos">The starting position of the damage line</param>
    /// <param name="endPos">The end position of the damage line (max range)</param>
    /// <param name="baseDamage">How much total damage to deal</param>
    public Damage(Vector2 startPos, Vector2 endPos, float baseDamage)
    {
        StartPos   = startPos;
        Direction  = endPos - startPos;
        EndPos     = endPos;
        BaseDamage = baseDamage;
    }


    /// <summary>
    ///     Apply the damage to all tilemaps in range. Will penetrate multiple tiles if damage is high enough
    /// </summary>
    public void ApplyDamage()
    {
        var hits = Physics2D.RaycastAll(StartPos, Direction);
        Array.Sort(hits, (h1, h2) => h1.distance.CompareTo(h2.distance));
        Debug.Log(hits);
        Debug.DrawLine(StartPos, EndPos, Color.red, 5f);
        var done = false;
        foreach (RaycastHit2D hit in hits)
        {
            var          destroyedTile = false;
            Vector2      lineStart     = hit.point - 0.5f * Direction;
            Vector2      lineEnd       = EndPos;
            var          tileManager   = hit.transform.GetComponent<TileManager>();
            if (tileManager is null) continue;
            var dmgToObject = 0f;
            var line        = RasterUtil.Line(tileManager.PositionToCords(lineStart), tileManager.PositionToCords(lineEnd));

            foreach (Vector3Int cord in line)
            {
                tileManager.DamageTile(cord, BaseDamage, out float damageUsed);
                if (Math.Abs(damageUsed - BaseDamage) < float.Epsilon)
                {
                    done = true;
                    break;
                }

                BaseDamage    -= damageUsed;
                dmgToObject   += damageUsed;
                destroyedTile =  true;
            }

            if (destroyedTile) tileManager.RefreshPhysics();

            tileManager.Rigidbody2D.AddForceAtPosition(Direction * dmgToObject * 1e-3f, lineStart);

            if (done) return;
        }
    }
}
