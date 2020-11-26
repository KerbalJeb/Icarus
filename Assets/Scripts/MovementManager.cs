using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using TileSystem;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
///     Used to manage movement and calculate the flight model based on engine placement
/// </summary>
public class MovementManager
{
    private const    int                                          OutputStateDim = 3;
    private readonly Dictionary<Vector3Int, int>                  engineIDs = new Dictionary<Vector3Int, int>();
    private readonly List<(EnginePart part, Direction direction)> engines = new List<(EnginePart, Direction)>();
    private readonly List<ThrustVector>                           engineVectors = new List<ThrustVector>();
    private readonly List<GameObject>                             exhaustObjects = new List<GameObject>();
    private readonly List<ParticleSystem>                         exhaustParticleSystems = new List<ParticleSystem>();

    private readonly Dictionary<Direction, (Vector3 netThrust, Vector<float> values)> thrustProfiles =
        new Dictionary<Direction, (Vector3 netThrust, Vector<float> values)>();

    private readonly TileManager tileManager;
    private readonly Transform   transform;


    private float[]       a;
    private Vector2       com;
    private Vector3       currentInput;
    private Vector<float> engineInputs;
    private float         goalRot;
    private int           n;
    private Vector3       netThrust;

    private bool          physics;
    private Rigidbody2D   rb2D;
    private Matrix<float> thrustMatrix;


    /// <summary>
    ///     MovementManager Constructor
    /// </summary>
    /// <param name="manager">The tile manager it is attached to</param>
    /// <param name="transform">The transform of the ship</param>
    public MovementManager(TileManager manager, Transform transform)
    {
        tileManager    = manager;
        this.transform = transform;
    }

    private int m => OutputStateDim;

    public void ApplyThrust()
    {
        if (!physics) return;
        if (rb2D == null) rb2D = tileManager.Rigidbody2D;
        rb2D.AddTorque(netThrust.z);
        rb2D.AddRelativeForce(netThrust);
        if (currentInput.z == 0) StabilizeRotation();

        for (var i = 0; i < n; i++)
        {
            float thrust = engineInputs[i];
            if (thrust == 0)
            {
                exhaustParticleSystems[i].Stop();
                exhaustParticleSystems[i].Clear();
            }
            else
            {
                ParticleSystem.MainModule main = exhaustParticleSystems[i].main;
                main.startLifetime = thrust * 1f;

                if (!exhaustParticleSystems[i].isPlaying) exhaustParticleSystems[i].Play();
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
        // Position Controller
        if (position)
        {
            float rotationError = rb2D.rotation - goalRot;
            thrust.z = 0.3f * angularVelocityError + 0.05f * rotationError;
        }
        // Velocity controller
        else
            thrust.z = 0.05f * angularVelocityError;

        SetThrust(thrust);
    }


    /// <summary>
    ///     Adds an engine to the flight model
    /// </summary>
    /// <param name="cord">The location of the engine</param>
    /// <param name="enginePart">The engine variant</param>
    /// <param name="exhaust">The exhaust effect object</param>
    /// <param name="rot">The rotation of the engine</param>
    public void AddEngine(Vector3Int cord, EnginePart enginePart, GameObject exhaust, Direction rot)
    {
        var particles = exhaust.GetComponent<ParticleSystem>();
        particles.Stop();
        engines.Add((enginePart, rot));
        exhaustObjects.Add(exhaust);
        exhaustParticleSystems.Add(particles);
        engineIDs[cord] = n;
        n++;
    }

    public void RebuildFlightModel()
    {
        com = tileManager.CenterOfMass;
        a   = new float[n * m]; // Column Major form
        if (n <= 0)
        {
            physics = false;
            return;
        }

        physics = true;

        engineVectors.Clear();

        foreach (var engineID in engineIDs)
        {
            int        ID  = engineID.Value;
            Vector3Int pos = engineID.Key;
            engineVectors.Add(GetThrustVector(pos, engines[ID].part.thrust, engines[ID].direction));
        }

        engineInputs = Vector<float>.Build.Dense(n);

        thrustMatrix = Matrix<float>.Build.Dense(m, n, a);

        for (var i = 0; i < n; i++) thrustMatrix.SetColumn(i, engineVectors[i].Data);

        foreach (Direction value in (Direction[]) Enum.GetValues(typeof(Direction)))
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
                    case Direction.Up:
                        if (engine.ThrustY <= 0) break;

                        if (engine.ThrustY / engine.Magnitude > threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Direction.Down:
                        if (engine.ThrustY >= 0) break;
                        if (engine.ThrustY / engine.Magnitude < -threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Direction.Left:
                        if (engine.ThrustX <= 0) break;
                        if (engine.ThrustX / engine.Magnitude > threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Direction.Right:
                        if (engine.ThrustX >= 0) break;
                        if (engine.ThrustX / engine.Magnitude < -threshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Direction.ZUp:
                        if (engine.Toque >= toqueThreshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                    case Direction.ZDown:
                        if (engine.Toque <= -toqueThreshold)
                        {
                            dirNetThrust    += engine.NetThrust;
                            engineVector[i] =  1;
                        }

                        break;
                }
            }

            thrustProfiles[value] = (dirNetThrust, engineVector);
        }
    }

    /// <summary>
    ///     Removes an engine
    /// </summary>
    /// <param name="cord">The coordinates of the engine</param>
    public void RemoveEngine(Vector3Int cord)
    {
        if (!engineIDs.ContainsKey(cord)) return;

        int id = engineIDs[cord];
        Object.Destroy(exhaustObjects[id]);
        exhaustObjects.RemoveAt(id);
        exhaustParticleSystems.RemoveAt(id);
        engines.RemoveAt(id);
        n--;
        var cords = engineIDs.Where(engineID => engineID.Value > id).Select(engineID => engineID.Key).ToArray();
        foreach (Vector3Int i in cords) engineIDs[i]--;
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
        if (rb2D == null || !physics) return;
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
        if (n <= 0 || !physics) return;
        var direction = GetRotations(thrust);
        engineInputs = Vector<float>.Build.Dense(n, 0);
        foreach ((Direction tileRotation, float mag) in direction)
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
    private static IEnumerable<(Direction, float)> GetRotations(Vector3 thrust)
    {
        var         tileRot   = new List<(Direction, float)>();
        const float threshold = 0.1f;

        if (thrust.x > threshold) tileRot.Add((Direction.Left, Mathf.Abs(thrust.x)));
        if (thrust.x < -threshold) tileRot.Add((Direction.Right, Mathf.Abs(thrust.x)));
        if (thrust.y > threshold) tileRot.Add((Direction.Up, Mathf.Abs(thrust.y)));
        if (thrust.y < -threshold) tileRot.Add((Direction.Down, Mathf.Abs(thrust.y)));
        if (thrust.z > threshold) tileRot.Add((Direction.ZDown, Mathf.Abs(thrust.z)));
        if (thrust.z < -threshold) tileRot.Add((Direction.ZUp, Mathf.Abs(thrust.z)));

        return tileRot;
    }

    /// <summary>
    ///     Creates a new thrust vector from the engine data
    /// </summary>
    /// <param name="cords">The location of the engine</param>
    /// <param name="thrustMag"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    private ThrustVector GetThrustVector(Vector3Int cords, float thrustMag, Direction rot)
    {
        Vector2 dir      = TileInfo.Directions[rot];
        Vector2 thrust   = dir * thrustMag;
        Vector2 pos      = tileManager.CordsToPosition(cords);
        Vector2 posToCom = pos - com;
        float   toque    = (posToCom.x * dir.y - posToCom.y * dir.x) * thrustMag;
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
