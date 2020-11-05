using System;
using TileSystem;
using UnityEngine;

public class Damage
{
    private readonly RaycastHit2D[] hits = new RaycastHit2D[20];
    public           float          BaseDamage;
    public           Vector2        Direction;
    public           Vector2        EndPos;

    public Vector2 StartPos;

    public Damage(Vector2 startPos, Vector2 endPos, float baseDamage)
    {
        StartPos   = startPos;
        Direction  = endPos - startPos;
        EndPos     = endPos;
        BaseDamage = baseDamage;
    }


    public void ApplyDamage()
    {
        int numHits = Physics2D.RaycastNonAlloc(StartPos, Direction, hits);
        Debug.DrawLine(StartPos, EndPos, Color.red, 5f);
        var done = false;
        for (var i = 0; i < numHits; i++)
        {
            var          destroyedTile = false;
            RaycastHit2D hit           = hits[i];
            Vector2      lineStart     = hit.point - 0.5f * Direction;
            Vector2      lineEnd       = EndPos;
            var          shipManager   = hit.transform.GetComponent<ShipManager>();
            if (shipManager is null) continue;
            if (!shipManager.PhysicsEnabled) continue;
            TileManager tileManager = shipManager.TileManager;
            var dmgToObject = 0f;
            var line = RasterUtil.Line(tileManager.PositionToCords(lineStart), tileManager.PositionToCords(lineEnd));

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

            if (destroyedTile) shipManager.UpdatePhysics();

            shipManager.Rigidbody2D.AddForceAtPosition(Direction * dmgToObject * 1e-3f, lineStart);

            if (done) return;
        }
    }
}
