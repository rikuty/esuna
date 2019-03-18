using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UltimateTerrains
{
    public static class UMath
    {
        private const Int16 FN_INLINE = 256; //(Int16)MethodImplOptions.AggressiveInlining;

        public const double Sqr3 = 1.732050807568877;
        public const double Sqr3Inverse = 1.0 / Sqr3;

        [MethodImpl(FN_INLINE)]
        public static double Min(double a, double b, double c)
        {
            return Math.Min(a, Math.Min(b, c));
        }

        [MethodImpl(FN_INLINE)]
        public static int Min(int a, int b, int c)
        {
            return Math.Min(a, Math.Min(b, c));
        }

        [MethodImpl(FN_INLINE)]
        public static double Max(double a, double b, double c)
        {
            return Math.Max(a, Math.Max(b, c));
        }

        [MethodImpl(FN_INLINE)]
        public static int Max(int a, int b, int c)
        {
            return Math.Max(a, Math.Max(b, c));
        }

        [MethodImpl(FN_INLINE)]
        public static int Mod(int x, int m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        [MethodImpl(FN_INLINE)]
        public static double Mod(double x, double m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        [MethodImpl(FN_INLINE)]
        public static double Lerp(double a, double b, double t)
        {
            t = Math.Max(0d, Math.Min(t, 1d));
            return a * (1d - t) + b * t;
        }

        [MethodImpl(FN_INLINE)]
        public static double InverseLerp(double a, double b, double l)
        {
            l = Math.Max(a, Math.Min(l, b));
            return (l - a) / (b - a);
        }

        [MethodImpl(FN_INLINE)]
        public static double Clamp(double min, double max, double val)
        {
            return Math.Max(min, Math.Min(val, max));
        }

        [MethodImpl(FN_INLINE)]
        public static bool Approximately(double a, double b)
        {
            return a - b < Param.PRECISION && b - a < Param.PRECISION;
        }

        [MethodImpl(FN_INLINE)]
        public static bool Approximately(double a, double b, double precision)
        {
            return a - b < precision && b - a < precision;
        }

        public static double TrilinearInterpolate(double f000, double f001, double f010, double f011,
                                                  double f100, double f101, double f110, double f111, Vector3d position)
        {
            var oneMinX = 1.0 - position.x;
            var oneMinY = 1.0 - position.y;
            var oneMinZ = 1.0 - position.z;
            return oneMinX * (oneMinY * (f000 * oneMinZ + f001 * position.z) + position.y * (f010 * oneMinZ + f011 * position.z))
                   + oneMinY * position.x * (f100 * oneMinZ + f101 * position.z)
                   + position.x * position.y * (f110 * oneMinZ + f111 * position.z);
        }

        public static double BilinearInterpolate(double f00, double f01, double f10, double f11, double x, double y)
        {
            var oneMinX = 1.0 - x;
            var oneMinY = 1.0 - y;
            return oneMinX * (oneMinY * f00 + y * f01) +
                   x * (oneMinY * f10 + y * f11);
        }

        public static double DistanceToPlane(Vector3d p, Vector3d pointOfPlane, Vector3d normalOfPlane)
        {
            var a = normalOfPlane.x;
            var b = normalOfPlane.y;
            var c = normalOfPlane.z;
            // ax + by + cz + d = 0
            var d = -(a * pointOfPlane.x + b * pointOfPlane.y + c * pointOfPlane.z);
            return (a * p.x + b * p.y + c * p.z + d) / normalOfPlane.Magnitude;
        }

        public static double DistanceToPlaneNormalized(Vector3d p, Vector3d pointOfPlane, Vector3d normalOfPlane)
        {
            var a = normalOfPlane.x;
            var b = normalOfPlane.y;
            var c = normalOfPlane.z;
            // ax + by + cz + d = 0
            var d = -(a * pointOfPlane.x + b * pointOfPlane.y + c * pointOfPlane.z);
            return a * p.x + b * p.y + c * p.z + d;
        }

        [MethodImpl(FN_INLINE)]
        public static double DistanceToLine(Vector3d p, Vector3d pointOfLane, Vector3d direction)
        {
            return Vector3d.Cross(p - pointOfLane, direction.Normalized).Magnitude;
        }

        [MethodImpl(FN_INLINE)]
        public static Vector3d OrthogonalProjectionOntoLine(Vector3d p, Vector3d pointOfLane, Vector3d direction)
        {
            return OrthogonalProjectionOntoLineNormalized(p, pointOfLane, direction.Normalized);
        }

        [MethodImpl(FN_INLINE)]
        public static Vector3d OrthogonalProjectionOntoLineNormalized(Vector3d p, Vector3d pointOfLane, Vector3d direction)
        {
            return Vector3d.Dot(p - pointOfLane, direction) * direction + pointOfLane;
        }

        [MethodImpl(FN_INLINE)]
        public static Vector3d Min(Vector3d a, Vector3d b)
        {
            return new Vector3d(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
        }

        [MethodImpl(FN_INLINE)]
        public static Vector3i Min(Vector3i a, Vector3i b)
        {
            return new Vector3i(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
        }

        [MethodImpl(FN_INLINE)]
        public static Vector3d Max(Vector3d a, Vector3d b)
        {
            return new Vector3d(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }

        [MethodImpl(FN_INLINE)]
        public static Vector3i Max(Vector3i a, Vector3i b)
        {
            return new Vector3i(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }

        public static void ComputeEdgeVectors(Vector3 start, Vector3 end, float width, float height,
                                              out Vector3 vL, out Vector3 vH, out Vector3 vW)
        {
            vL = end - start;
            if (Approximately(vL.x, 0) && Approximately(vL.z, 0)) {
                vW = Vector3.right * width;
                vH = Vector3.forward * height;
            } else {
                vW = Vector3.Cross(Vector3.up, vL).normalized * width;
                vH = Vector3.Cross(vL, vW).normalized * height;
            }
        }

        public static void ComputeEdgeVectors(Vector3d start, Vector3d end, double width, double height,
                                              out Vector3d vL, out Vector3d vH, out Vector3d vW)
        {
            vL = end - start;
            if (Approximately(vL.x, 0) && Approximately(vL.z, 0)) {
                vW = Vector3d.right * width;
                vH = Vector3d.forward * height;
            } else {
                vW = Vector3d.Cross(Vector3d.up, vL).Normalized * width;
                vH = Vector3d.Cross(vL, vW).Normalized * height;
            }
        }
    }
}