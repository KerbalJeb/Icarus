using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using TileSystem;
using TileSystem.TileVariants;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TileManager))]
public class MovementManager : MonoBehaviour
{
    private TileManager                                       tileManager;
    private Vector2                                           com;
    private List<(Vector3Int cords, FunctionalTileData data)> engines;
    private List<ThrustVector>                                engineVectors = new List<ThrustVector>();

    private readonly Dictionary<ThrustDir, List<ThrustVector>> directionToThrustVectors =
        new Dictionary<ThrustDir, List<ThrustVector>>();
    
    private static readonly ReadOnlyDictionary<TileRotation, Vector2> Directions = 
        new ReadOnlyDictionary<TileRotation, Vector2>(new Dictionary<TileRotation, Vector2>
        {
            {TileRotation.Up, Vector2.down},
            {TileRotation.Down, Vector2.up},
            {TileRotation.Left, Vector2.left},
            {TileRotation.Right, Vector2.right},
        });
    private void Awake()
    {
        tileManager = GetComponent<TileManager>();
        com         = GetComponent<Rigidbody2D>().centerOfMass;
        
        tileManager.GetTilesByVariant<EngineVariant>(out engines);
        engineVectors.Capacity = engines.Count;

        foreach (var value in (ThrustDir[])Enum.GetValues(typeof(ThrustDir)))
        {
            directionToThrustVectors[value] = new List<ThrustVector>();
        }

        foreach ((Vector3Int cords, FunctionalTileData data) in engines)
        {
            var dir          = Directions[data.Rotation];
            var thrust       = ((EngineVariant) tileManager.TileSet.TileVariants[data.ID]).Thrust;
            var vector = dir * thrust;
            var pos          = new Vector2(cords.x * tileManager.TileSize, cords.y * tileManager.TileSize);
            pos -= com;
            var toque        = (pos.x * dir.y - pos.y * dir.x) * thrust;
            var thrustVector = new ThrustVector(vector, toque);
            engineVectors.Add(thrustVector);

            foreach (ThrustDir thrustDir in thrustVector.ThrustDirs)
            {
                directionToThrustVectors[thrustDir].Add(thrustVector);
            }
        }
    }
/*
    private void BoundedLeastSquares(Matrix<float> A, Vector<float> b, Vector<float> l, Vector<float> u, out Vector<float> x)
    {
        int m      = A.RowCount;
        int n      = A.ColumnCount;
        // 1.
        var F      = new HashSet<int>();
        var L      = new HashSet<int>();
        var U      = new HashSet<int>(Enumerable.Range(0, m));
        var allIDs = new HashSet<int>(Enumerable.Range(0, m));
        var A_T    = A.Transpose();
        var x0 = l.Clone();
        
        // 2.
        var w = A_T*(b-A*x0);
        
        // 3.
        if (F.Equals(allIDs) || L.All(j=>w[j]>=0)&& U.All(j=>w[j]<=0))
        {
            x = x0;
           return;
        }
        
        // 4.
        int tL     = L.Aggregate((t1, t2) => w[t1] >= w[t2]? t1: t2);
        int tU     = U.Aggregate((t1, t2) => w[t1] <= w[t2]? t1: t2);
        int t_star = w[tL] > -w[tU] ? tL : tU;
        
        // 5.
        F.Add(t_star);
        
        // 6.
        var b_prime = b.Clone();
        
        for (int j = 0; j < n; j++)
        {
            float sum = L.Union(U).Sum(k => A[j, k] * x0[j]);
            b_prime -= sum;
        }

        var APrime  = A.Clone();
        var jPrimes = new Dictionary<int, int>();
        var count   = 0;
        
        foreach (int j in F.OrderBy(i=>i))
        {
            jPrimes[count] = j;
            count++;
        }
        foreach (int i in allIDs.Except(F))
        {
            APrime.RemoveColumn(i);
        }
        LinearLeastSquares(APrime, b_prime, out var z);

        // 7.
        if (jPrimes.All(pair=> l[pair.Key] < z[pair.Value] && z[pair.Value]<u[pair.Key]))
        {
            // Goto 2.
        }
        
        // 8.
        var J = jPrimes.Where(pair => !(l[pair.Key] < z[pair.Value] && z[pair.Value] < u[pair.Key]))
                       .Select(pair => pair.Key);

        foreach (var j in J)
        {
            
        }

    }

    private static void LinearLeastSquares(Matrix<float> A, Vector<float> b, out Vector<float> x)
    {
        int m               = A.RowCount;
        int n               = A.ColumnCount;
        var qrFactorization = A.QR();
        var q               = qrFactorization.Q.SubMatrix(0, m, 0, n);
        var r               = qrFactorization.R.SubMatrix(0, n, 0, n);
        var y               = q.Transpose() * b;
        
        x = r.Solve(y);
    }
 */   
    public class ThrustVector
    {
        public ThrustVector(Vector2 thrust, float toque)
        {
            Thrust     = thrust;
            Toque      = toque;
        }

        public Vector2         Thrust;
        public float           Toque;
        public List<ThrustDir> ThrustDirs => GetThrustDirs(this);
        public static explicit operator float[](ThrustVector v) => new []{v.Thrust.x, v.Thrust.y, v.Toque};
    }

    private static List<ThrustDir> GetThrustDirs(ThrustVector vec)
    {
        var thrustDirs = new List<ThrustDir>();
        
        if (vec.Thrust.x > 0)
        {
            thrustDirs.Add(ThrustDir.ThrustRight);
        }
        else if (vec.Thrust.x < 0)
        {
            thrustDirs.Add(ThrustDir.ThrustLeft);
        }

        if (vec.Thrust.y > 0)
        {
            thrustDirs.Add(ThrustDir.ThrustUp);
        }
        else if (vec.Thrust.y < 0)
        {
            thrustDirs.Add(ThrustDir.ThrustDown);
        }

        if (vec.Toque > 0)
        {
            thrustDirs.Add(ThrustDir.ToqueLeft);
        }
        else if (vec.Toque < 0)
        {
            thrustDirs.Add(ThrustDir.ToqueRight);

        }

        return thrustDirs;
    }
    
    public enum ThrustDir
    {
        ThrustUp,
        ThrustDown,
        ThrustLeft,
        ThrustRight,
        ToqueLeft,
        ToqueRight,
    }
}
