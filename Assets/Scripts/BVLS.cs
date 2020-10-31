using System;
using System.Runtime.InteropServices;
public static class MathFortran
{
    [DllImport("D:\\School\\ECE356\\Icarus\\Assets\\Plugins\\bvlsF.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void bvls_(
        ref int key, //key = 0, the subroutine solves the problem from scratch. If key > 0 the routine initializes using the user's guess about which components of  x  are `active'
        ref int m,
        ref int n,
        [In, Out] double[] a, //  m by n matrix
        [In, Out] double[] b, //  m-vector 
        [In, Out] double[] bl, //n-vector of lower bounds on the components of x.
        [In, Out] double[] bu, //n-vector of upper bounds on the components of x.
        [In, Out] double[] x, //unknown n-vector
        //Working  arrays:
        [In, Out] double[] w,      //dimension n
        [In, Out] double[] act,    //dimension m*(mm+2). mm=min(m,n).
        [In, Out] double[] zz,     //dimension m
        [In, Out] int[]    istate, //dimension n+1.
        ref       int      loopA //   number of iterations taken in the main loop, Loop A.
    );
    
    public static void BVTEST()
    {
        int      key    =0, n =2, m =3;
        double[] a      = { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };
        double[] b      = { 10.0, 20.0, 30.0 };
        double[] bl     = { 0.0, 0.0, 0.0, 0.0, 0.0 };
        double[] bu     = { 1.0, 1.0, 1.0, 1.0, 1.0};
        double[] x      =new double[n];
        double[] w      =new double[n];
        double[] act    =new double[m *Math.Min(m, n) +2];
        double[] zz     =new double[m];
        int[]    istate =new int[n +1];
        int      loopA  = 0;
        // Call Fortran .dll
        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 100000; i++)
        {
            bvls_(ref key, ref m, ref n, a, b, bl, bu, x, w, act, zz, istate, ref loopA);
        }
        watch.Stop();
        
        Console.WriteLine(watch.ElapsedMilliseconds);

    }
}

