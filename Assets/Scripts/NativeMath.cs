using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

public static class NativeMath
{
    [DllImport("BVLS.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int bvls(
        int key, //key = 0, the subroutine solves the problem from scratch. If key > 0 the routine initializes using the user's guess about which components of  x  are `active'
        int m,
        int n,
        float[] a, //  m by n matrix
        float[] b, //  m-vector 
        float[] bl, //n-vector of lower bounds on the components of x.
        float[] bu, //n-vector of upper bounds on the components of x.
        float[] x, //unknown n-vector
        //Working  arrays:
        float[] w,      //dimension n
        float[] act,    //dimension m*(mm+2). mm=min(m,n).
        float[] zz,     //dimension m
        int[]   istate, //dimension n+1.
        ref int loopA,  //   number of iterations taken in the main loop, Loop A.
        int     verbose
    );

    public static void BVTEST()
    {
        int     key    = 0, n = 2, m = 3;
        float[] a      = {1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
        float[] b      = {10.0f, 20.0f, 30.0f};
        float[] bl     = {0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
        float[] bu     = {1.0f, 1.0f, 1.0f, 1.0f, 1.0f};
        var     x      = new float[n];
        var     w      = new float[n];
        var     act    = new float[m * Math.Min(m, n) + 2];
        var     zz     = new float[m];
        var     istate = new int[n + 1];
        var     loopA  = 0;
        // Call .dll
        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 10000; i++) bvls(key, m, n, a, b, bl, bu, x, w, act, zz, istate, ref loopA, 0);
        watch.Stop();

        Debug.Log(watch.ElapsedMilliseconds);
    }
}
