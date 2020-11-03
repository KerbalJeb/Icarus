using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using TileSystem;
using TileSystem.TileVariants;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TileManager))]
public class MovementManager : MonoBehaviour
{
    private const int OutputStateDim = 3;

    private Dictionary<TileRotation, (Vector3 netThrust, Vector<float> values)> thrustProfiles;

    private List<ThrustVector> engineVectors;

    private float[] a;
    private float[] act;
    private float[] bl;
    private float[] bu;


    private Vector2       com;
    private int[]         istate;
    private Vector3       netThrust;
    private Vector<float> engineInputs;
    private bool          physics = false;
    private Rigidbody2D   rb2D;
    private Matrix<float> thrustMatrix;
    private TileManager   tileManager;
    private float[]       w;
    private float[]       x;
    private float[]       zz;
    private bool          turningActiveInput = false;
    private Vector3       currentInput;
    private float         goalRot =0f;


    private int n;
    private int m => OutputStateDim;

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
            if (currentInput.z == 0)
            {
                StabilizeRotation();
            }
            for (int i = 0; i < n; i++)
            {
                var start = engineVectors[i].Pos;
                var end   = start + engineVectors[i].ThrustDir * (engineInputs[i] * -5f);
                start = transform.TransformPoint(start);
                end   = transform.TransformPoint(end);
                Debug.DrawLine(start, end, Color.green);
            }
        }
    }

    private void StabilizeRotation(bool position=false)
    {
        var angularVelocityError = rb2D.angularVelocity;
        var thrust               = currentInput;
        if (position)
        {
            var rotationError = (rb2D.rotation -goalRot);
            thrust.z = 0.3f *angularVelocityError + 0.05f *rotationError;
        }
        else
        {
            thrust.z = 0.5f * angularVelocityError;
        }
        SetThrust(thrust);
    }

    public void UpdatePhysics()
    {
        rb2D = GetComponent<Rigidbody2D>();
        com  = rb2D.centerOfMass;
        tileManager.GetTilesByVariant<EngineVariant>(out List<(Vector3Int cords, FunctionalTileData data)> engines);
        n = engines.Count;
        engineVectors = new List<ThrustVector>();
        thrustProfiles = new Dictionary<TileRotation, (Vector3 netThrust, Vector<float> values)>();


        if (n < 1) return;

        /*bvls params*/
        a           = new float[n * m]; // Column Major form
        bl          = Enumerable.Repeat(0f, n).ToArray();
        bu          = Enumerable.Repeat(1f, n).ToArray();
        x           = new float[n];
        w           = new float[n];
        act         = new float[m * (Math.Min(n, m) + 2)];
        zz          = new float[m];
        istate      = new int[n + 1];
        engineInputs = Vector<float>.Build.Dense(n);

        thrustMatrix = Matrix<float>.Build.Dense(m, n, a);

        for (var i = 0; i < n; i++)
        {
            (Vector3Int cords, FunctionalTileData data) = engines[i];
            ThrustVector thrustVector = GetThrustVector(cords, data);
            engineVectors.Add(thrustVector);
            thrustMatrix.SetColumn(i, thrustVector.Data);
        }
        foreach (TileRotation value in (TileRotation[])Enum.GetValues(typeof(TileRotation)))
        {
            var         engineVector      = Vector<float>.Build.Dense(n, 0);
            const float threshold      = 0.25f;
            const float toqueThreshold = 1f;
            Vector3     dirNetThrust   = Vector3.zero;
            
            for (int i = 0; i < n; i++)
            {
                var engine = engineVectors[i];
                switch (value)
                {
                    case TileRotation.Up:
                        if (engine.ThrustY<=0)break;

                        if (engine.ThrustY/engine.Magnitude > threshold)
                        {
                            dirNetThrust += engine.NetThrust;
                            engineVector[i]=1;
                        }
                        break;
                    case TileRotation.Down:
                        if (engine.ThrustY>=0)
                        {
                            break;
                        }
                        if (engine.ThrustY/engine.Magnitude < threshold)
                        {
                            dirNetThrust += engine.NetThrust;
                            engineVector[i]=1;
                        }
                        break;
                    case TileRotation.Left:
                        if (engine.ThrustX <= 0)
                        {
                            break;
                        }
                        if (engine.ThrustX /engine.Magnitude > threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }
                        break;
                    case TileRotation.Right:
                        if (engine.ThrustX >= 0)
                        {
                            break;
                        }
                        if (engine.ThrustX /engine.Magnitude < threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }
                        break;
                    case TileRotation.TurnUp:
                        if (engine.Toque >= toqueThreshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }
                        break;
                    case TileRotation.TurnDown:
                        if (engine.Toque <= toqueThreshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }
                        break;
                    case TileRotation.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            thrustProfiles[value] = (dirNetThrust, engineVector);
        }
        physics = true;
    }

    private void SetHeading(float[] dir)
    {
        Debug.Assert(dir.Length == m);
        var loop = 0;
        NativeMath.bvls(0, m, n, a, dir, bl, bu, x, w, act, zz, istate, ref loop, 0);
        engineInputs = Vector<float>.Build.Dense(x);
        var result      = thrustMatrix * engineInputs;
        netThrust = new Vector3(result[0], result[1], result[2]);
    }
    
    public void Steer(InputAction.CallbackContext ctx)
    {
        if (!physics)
        {
            return;
        }

        var thrust = ctx.ReadValue<Vector3>();
        SetThrust(thrust);
        currentInput       = thrust;
        if (thrust.z == 0)
        {
            goalRot       = rb2D.rotation;
        }
    }

    private void SetThrust(Vector3 thrust)
    {
        var direction  = GetRotations(thrust);
        engineInputs = Vector<float>.Build.Dense(n, 0);
        foreach ((TileRotation tileRotation, float mag) in direction)
        {
            var input =  thrustProfiles[tileRotation].values;
            for (int i = 0; i < n; i++)
            {
                engineInputs[i] = Mathf.Clamp(input[i]*mag + engineInputs[i], 0, 1);
            }
        }

        var netEffect = thrustMatrix * engineInputs;
        netThrust = new Vector3(netEffect[0], netEffect[1], netEffect[2]);
    }

    private static IEnumerable<(TileRotation, float)> GetRotations(Vector3 thrust)
    {
        var         tileRot   = new List<(TileRotation, float)>();
        const float threshold = 0.1f;
        
        if (thrust.x > threshold)
        {
            tileRot.Add((TileRotation.Left, Mathf.Abs(thrust.x)));
        }
        if (thrust.x < -threshold)
        {
            tileRot.Add((TileRotation.Right, Mathf.Abs(thrust.x)));
        }
        if (thrust.y > threshold)
        {
            tileRot.Add((TileRotation.Up, Mathf.Abs(thrust.y)));
        }
        if (thrust.y<-threshold)
        {
            tileRot.Add((TileRotation.Down, Mathf.Abs(thrust.y)));
        }
        if (thrust.z>threshold)
        {
            tileRot.Add((TileRotation.TurnDown, Mathf.Abs(thrust.z)));
        }
        if (thrust.z<-threshold)
        {
            tileRot.Add((TileRotation.TurnUp, Mathf.Abs(thrust.z)));
        }

        return tileRot;
    }

    private ThrustVector GetThrustVector(Vector3Int cords, FunctionalTileData data)
    {
        Vector2 dir       = TileInfo.Directions[data.Rotation];
        float   thrustMag = ((EngineVariant) tileManager.TileSet.TileVariants[data.ID]).Thrust;
        Vector2 thrust    = dir * thrustMag;
        var     pos       = tileManager.CordsToPosition(cords);
        var   posToCom = pos - com;
        float toque    = (posToCom.x * dir.y - posToCom.y * dir.x) * thrustMag;
        return new ThrustVector(thrust.x, thrust.y, toque, pos);
    }

    private class ThrustVector
    {
        public ThrustVector(float thrustX, float thrustY, float toque, Vector3 pos)
        {
            Pos  = pos;
            Data      = new[] {thrustX, thrustY, toque};
            NetThrust = new Vector3(thrustX, thrustY, toque);
            Direction = NetThrust.normalized;
            var dir = new Vector3(thrustX, thrustY);
            ThrustDir = dir.normalized;
            Magnitude = Mathf.Sqrt(thrustX * thrustX + thrustY * thrustY);
        }

        public float   ThrustX   => Data[0];
        public float   ThrustY   => Data[1];
        public float   Toque     => Data[2];
        public float[] Data      { get; }
        public Vector3 NetThrust { get; }
        public Vector3 Direction { get; }
        public Vector3 ThrustDir { get; }
        public Vector3 Pos;
        public float   Magnitude { get; }

        public static implicit operator Vector3(ThrustVector t) => new Vector3(t.ThrustX, t.ThrustY, t.Toque);
    }
}
