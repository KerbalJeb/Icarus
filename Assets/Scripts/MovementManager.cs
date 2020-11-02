using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using TileSystem;
using TileSystem.TileVariants;
using UnityEngine;

[RequireComponent(typeof(TileManager))]
public class MovementManager : MonoBehaviour
{
    private const int outputStateDim = 3;

    private static readonly ReadOnlyDictionary<TileRotation, Vector2> Directions =
        new ReadOnlyDictionary<TileRotation, Vector2>(new Dictionary<TileRotation, Vector2>
        {
            {TileRotation.Up, Vector2.up},
            {TileRotation.Down, Vector2.down},
            {TileRotation.Left, Vector2.left},
            {TileRotation.Right, Vector2.right},
        });

    private readonly List<ThrustVector> engineVectors = new List<ThrustVector>();

    private float[] a;
    private float[] act;
    private float[] bl;
    private float[] bu;


    private Vector2       com;
    private int[]         istate;
    private Vector3       netThrust;
    private bool          physics = false;
    private Rigidbody2D   rb2D;
    private Matrix<float> thrustMatrix;
    private TileManager   tileManager;
    private float[]       w;
    private float[]       x;
    private float[]       zz;


    public int n;

    private int m => outputStateDim;

    private void Awake()
    {
        tileManager = GetComponent<TileManager>();
    }

    private void FixedUpdate()
    {
        if (physics)
        {
            rb2D.AddTorque(netThrust.z);
            rb2D.AddRelativeForce(netThrust);
            for (int i = 0; i < engineVectors.Count; i++)
            {
                var vec       = engineVectors[i];
                var position3  = transform.TransformPoint(vec.Position);
                var position = new Vector2(position3.x, position3.y);
                var direction = new Vector2(vec.Direction.x, vec.Direction.y);
            }
        }
    }

    public void UpdatePhysics()
    {
        rb2D = GetComponent<Rigidbody2D>();
        com  = rb2D.centerOfMass;
        tileManager.GetTilesByVariant<EngineVariant>(out List<(Vector3Int cords, FunctionalTileData data)> engines);
        n = engines.Count;


        if (n < 1) return;

        /*bvls params*/
        a      = new float[n * m]; // Column Major form
        bl     = Enumerable.Repeat(0f, n).ToArray();
        bu     = Enumerable.Repeat(1f, n).ToArray();
        x      = new float[n];
        w      = new float[n];
        act    = new float[m * (Math.Min(n, m) + 2)];
        zz     = new float[m];
        istate = new int[n + 1];

        thrustMatrix = Matrix<float>.Build.Dense(m, n, a);

        for (var i = 0; i < n; i++)
        {
            (Vector3Int cords, FunctionalTileData data) = engines[i];
            ThrustVector thrustVector = GetThrustVector(cords, data);
            engineVectors.Add(thrustVector);
            thrustMatrix.SetColumn(i, thrustVector.Data);
        }


        SetHeading(new[] {0f, 4f, 0f});
        physics = true;
    }

    private float[] SetHeading(float[] dir)
    {
        Debug.Assert(dir.Length == m);
        var loop = 0;
        NativeMath.bvls(0, m, n, a, dir, bl, bu, x, w, act, zz, istate, ref loop, 0);
        var inputVector = Vector<float>.Build.Dense(x);
        var result      = thrustMatrix * inputVector;
        netThrust = new Vector3(result[0], result[1], result[2]);
        return x;
    }


    private void SetThrustProfile(Vector3 dir)
    {
        float mag = dir.sqrMagnitude;
        dir = dir.normalized;
        var thrustProfile = new float[n];

        for (var i = 0; i < thrustProfile.Length; i++)
        {
            float power = Vector3.Dot(dir, engineVectors[i].Direction);
            thrustProfile[i] =  power > 0.25f ? power : 0f;
            netThrust        += thrustProfile[i] * (Vector3) engineVectors[i];
        }

        float ratio = mag / netThrust.sqrMagnitude;
        ratio = Mathf.Clamp(ratio, 0, 1);

        for (var i = 0; i < thrustProfile.Length; i++) thrustProfile[i] *= ratio;

        netThrust *= ratio;
    }


    private ThrustVector GetThrustVector(Vector3Int cords, FunctionalTileData data)
    {
        Vector2 dir       = Directions[data.Rotation];
        float   thrustMag = ((EngineVariant) tileManager.TileSet.TileVariants[data.ID]).Thrust;
        Vector2 thrust    = dir * thrustMag;
        var     pos       = new Vector2(cords.x * tileManager.TileSize, cords.y * tileManager.TileSize);
        pos -= com;
        float toque = (pos.x * dir.y - pos.y * dir.x) * thrustMag;
        return new ThrustVector(thrust.x, thrust.y, toque, pos);
    }


    private class ThrustVector
    {
        public ThrustVector(float thrustX, float thrustY, float toque, Vector2 position)
        {
            Data      = new[] {thrustX, thrustY, toque};
            Direction = new Vector3(thrustX, thrustY, toque).normalized;
            
            Position  = position;
        }

        public float   ThrustX   => Data[0];
        public float   ThrustY   => Data[1];
        public float   Toque     => Data[2];
        public float[] Data      { get; }
        public Vector3 Direction { get; }
        public Vector2 Position;

        public static implicit operator Vector3(ThrustVector t) => new Vector3(t.ThrustX, t.ThrustY, t.Toque);
    }
}
