using UnityEngine;

public struct Damage
{
    public Damage(Vector3 startPos, Vector3 endPos, float baseDamage)
    {
        StartPos = startPos;
        EndPos = endPos;
        BaseDamage = baseDamage;
    }

    public Vector3 StartPos;
    public Vector3 EndPos;
    public float BaseDamage;
}