using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using TileSystem;
using TileSystem.TileVariants;
using UnityEngine;

/// <summary>
///     Used to manage movement and calculate the flight model based on engine placement
/// </summary>
[RequireComponent(typeof(TileManager))]
public class MovementManager : MonoBehaviour
{
    private const int OutputStateDim = 3;


    private float[]                                                           a;
    private Vector2                                                           com;
    private Vector3                                                           currentInput;
    private Vector<float>                                                     engineInputs;
    private List<ThrustVector>                                                engineVectors;
    private float                                                             goalRot = 0f;
    private int                                                               n;
    private Vector3                                                           netThrust;
    private bool                                                              physics = false;
    private Rigidbody2D                                                       rb2D;
    private Matrix<float>                                                     thrustMatrix;
    private Dictionary<Directions, (Vector3 netThrust, Vector<float> values)> thrustProfiles;
    private TileManager                                                       tileManager;
    private int                                                               m => OutputStateDim;


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
            if (currentInput.z == 0) StabilizeRotation();
            for (var i = 0; i < n; i++)
            {
                Vector3 start = engineVectors[i].Pos;
                Vector3 end   = start + engineVectors[i].ThrustDir * (engineInputs[i] * -5f);
                start = transform.TransformPoint(start);
                end   = transform.TransformPoint(end);
                Debug.DrawLine(start, end, Color.green);
            }
        }
    }

    /// <summary>
    ///     Stabilizes the rotation of the ship using a PD controller
    /// </summary>
    /// <param name="position">
    ///     If true the controller will attempt to maintain the original rotation, otherwise it will only
    ///     attempt to set the angular velocity to zero
    /// </param>
    private void StabilizeRotation(bool position = false)
    {
        float   angularVelocityError = rb2D.angularVelocity;
        Vector3 thrust               = currentInput;
        // Position Controler
        if (position)
        {
            float rotationError = rb2D.rotation - goalRot;
            thrust.z = 0.3f * angularVelocityError + 0.05f * rotationError;
        }
        // Velocity controller
        else
            thrust.z = 0.5f * angularVelocityError;

        SetThrust(thrust);
    }

    /// <summary>
    ///     Gets the engine information from the TileManger and updates the flight model accordingly
    /// </summary>
    public void UpdatePhysics()
    {
        rb2D = GetComponent<Rigidbody2D>();
        com  = rb2D.centerOfMass;
        tileManager.GetTilesByVariant<EngineVariant>(out var engines);
        n              = engines.Count;
        engineVectors  = new List<ThrustVector>();
        thrustProfiles = new Dictionary<Directions, (Vector3 netThrust, Vector<float> values)>();

        if (n < 1)
        {
            physics = false;
            return;
        }

        a            = new float[n * m]; // Column Major form
        engineInputs = Vector<float>.Build.Dense(n);

        thrustMatrix = Matrix<float>.Build.Dense(m, n, a);

        for (var i = 0; i < n; i++)
        {
            (Vector3Int cords, TileInstanceData data) = engines[i];
            ThrustVector thrustVector = GetThrustVector(cords, data);
            engineVectors.Add(thrustVector);
            thrustMatrix.SetColumn(i, thrustVector.Data);
        }

        foreach (Directions value in (Directions[]) Enum.GetValues(typeof(Directions)))
        {
            var         engineVector   = Vector<float>.Build.Dense(n, 0);
            const float threshold      = 0.25f;
            const float toqueThreshold = 1f;
            Vector3     dirNetThrust   = Vector3.zero;

            for (var i = 0; i < n; i++)
            {
                ThrustVector engine = engineVectors[i];
                switch (value)
                {
                    case Directions.Up:
                        if (engine.ThrustY <= 0) break;

                        if (engine.ThrustY / engine.Magnitude > threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Directions.Down:
                        if (engine.ThrustY >= 0) break;
                        if (engine.ThrustY / engine.Magnitude < threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Directions.Left:
                        if (engine.ThrustX <= 0) break;
                        if (engine.ThrustX / engine.Magnitude > threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Directions.Right:
                        if (engine.ThrustX >= 0) break;
                        if (engine.ThrustX / engine.Magnitude < threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Directions.ZUp:
                        if (engine.Toque >= toqueThreshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Directions.ZDown:
                        if (engine.Toque <= toqueThreshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                }
            }

            thrustProfiles[value] = (dirNetThrust, engineVector);
        }

        physics = true;
    }

    /// <summary>
    ///     Used to steer/move the ship in a given direction
    /// </summary>
    /// <param name="thrust">
    ///     The x and y components correspond to the x-y movement and the z component is used to indicate turning/toque. All
    ///     components of the vectors should be in the range [-1,1]
    /// </param>
    public void Steer(Vector3 thrust)
    {
        if (!physics) return;

        SetThrust(thrust);
        currentInput = thrust;
        if (thrust.z == 0) goalRot = rb2D.rotation;
    }

    /// <summary>
    ///     Attempts to output the desired thrust vector with the constraints of engine placement.
    /// </summary>
    /// <param name="thrust">
    ///     The x and y components correspond to the x-y movement and the z component is used to indicate
    ///     turning/toque. All components of the vectors should be in the range [-1,1]
    /// </param>
    private void SetThrust(Vector3 thrust)
    {
        var direction = GetRotations(thrust);
        engineInputs = Vector<float>.Build.Dense(n, 0);
        foreach ((Directions tileRotation, float mag) in direction)
        {
            var input                                   = thrustProfiles[tileRotation].values;
            for (var i = 0; i < n; i++) engineInputs[i] = Mathf.Clamp(input[i] * mag + engineInputs[i], 0, 1);
        }

        var netEffect = thrustMatrix * engineInputs;
        netThrust = new Vector3(netEffect[0], netEffect[1], netEffect[2]);
    }

    /// <summary>
    ///     Gets all the directions a engine thrusts in
    /// </summary>
    /// <param name="thrust">The engine thrust vector</param>
    /// <returns></returns>
    private static IEnumerable<(Directions, float)> GetRotations(Vector3 thrust)
    {
        var         tileRot   = new List<(Directions, float)>();
        const float threshold = 0.1f;

        if (thrust.x > threshold) tileRot.Add((Directions.Left, Mathf.Abs(thrust.x)));
        if (thrust.x < -threshold) tileRot.Add((Directions.Right, Mathf.Abs(thrust.x)));
        if (thrust.y > threshold) tileRot.Add((Directions.Up, Mathf.Abs(thrust.y)));
        if (thrust.y < -threshold) tileRot.Add((Directions.Down, Mathf.Abs(thrust.y)));
        if (thrust.z > threshold) tileRot.Add((Directions.ZDown, Mathf.Abs(thrust.z)));
        if (thrust.z < -threshold) tileRot.Add((Directions.ZUp, Mathf.Abs(thrust.z)));

        return tileRot;
    }

    /// <summary>
    ///     Creates a new thrust vector from the engine data
    /// </summary>
    /// <param name="cords">The location of the engine</param>
    /// <param name="data">The instance data for the engine</param>
    /// <returns></returns>
    private ThrustVector GetThrustVector(Vector3Int cords, TileInstanceData data)
    {
        Vector2 dir       = TileInfo.Directions[data.Rotation];
        float   thrustMag = ((EngineVariant) tileManager.TileSet.TileVariants[data.ID]).Thrust;
        Vector2 thrust    = dir * thrustMag;
        Vector2 pos       = tileManager.CordsToPosition(cords);
        Vector2 posToCom  = pos - com;
        float   toque     = (posToCom.x * dir.y - posToCom.y * dir.x) * thrustMag;
        return new ThrustVector(thrust.x, thrust.y, toque, pos);
    }

    /// <summary>
    ///     Used to represent the thrust from an engine.
    /// </summary>
    private class ThrustVector
    {
        public readonly Vector3 Pos;

        public ThrustVector(float thrustX, float thrustY, float toque, Vector3 pos)
        {
            Pos       = pos;
            Data      = new[] {thrustX, thrustY, toque};
            NetThrust = new Vector3(thrustX, thrustY, toque);
            var dir = new Vector3(thrustX, thrustY);
            ThrustDir = dir.normalized;
            Magnitude = Mathf.Sqrt(thrustX * thrustX + thrustY * thrustY);
        }

        public float   ThrustX   => Data[0];
        public float   ThrustY   => Data[1];
        public float   Toque     => Data[2];
        public float[] Data      { get; }
        public Vector3 NetThrust { get; }
        public Vector3 ThrustDir { get; }
        public float   Magnitude { get; }

        public static implicit operator Vector3(ThrustVector t) => new Vector3(t.ThrustX, t.ThrustY, t.Toque);
    }
}
