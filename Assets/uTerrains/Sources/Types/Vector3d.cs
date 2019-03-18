using System;
using Newtonsoft.Json;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Vector3d
    {
        public double x;
        public double y;
        public double z;

        public static readonly Vector3d zero = new Vector3d(0, 0, 0);
        public static readonly Vector3d one = new Vector3d(1, 1, 1);
        public static readonly Vector3d two = new Vector3d(2, 2, 2);

        public static readonly Vector3d forward = new Vector3d(0, 0, 1);
        public static readonly Vector3d back = new Vector3d(0, 0, -1);
        public static readonly Vector3d up = new Vector3d(0, 1, 0);
        public static readonly Vector3d down = new Vector3d(0, -1, 0);
        public static readonly Vector3d left = new Vector3d(-1, 0, 0);
        public static readonly Vector3d right = new Vector3d(1, 0, 0);


        public static readonly Vector3d[] directions =
        {
            left, right,
            back, forward,
            down, up
        };

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3d(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static double DistanceSquared(Vector3d a, Vector3d b)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;
            var dz = b.z - a.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public double DistanceSquared(Vector3d v)
        {
            return DistanceSquared(this, v);
        }

        public static double Distance(Vector3d a, Vector3d b)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;
            var dz = b.z - a.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public double Distance(Vector3d v)
        {
            return Distance(this, v);
        }

        public bool IsZero {
            get {
                return x < 1E-12f && x > -1E-12f &&
                       y < 1E-12f && y > -1E-12f &&
                       z < 1E-12f && z > -1E-12f;
            }
        }

        public Vector3i Rounded {
            get {
                return new Vector3i(
                    Convert.ToInt32(x),
                    Convert.ToInt32(y),
                    Convert.ToInt32(z)
                );
            }
        }
        
        public Vector3i Floored {
            get {
                return new Vector3i(
                    (int) Math.Floor(x),
                    (int) Math.Floor(y),
                    (int) Math.Floor(z)
                );
            }
        }

        public double MagnitudeSquared {
            get { return x * x + y * y + z * z; }
        }

        public double Magnitude {
            get { return Math.Sqrt(x * x + y * y + z * z); }
        }

        /// <summary>
        ///     Returns this vector with a magnitude of 1 (Read Only).
        ///     When normalized, a vector keeps the same direction but its length is 1.0.
        ///     Note that the current vector is unchanged and a new normalized vector is returned.
        ///     If the vector is too small to be normalized a zero vector will be returned.
        /// </summary>
        public Vector3d Normalized {
            get {
                var m = Magnitude;
                if (m < Param.PRECISION) {
                    return zero;
                }

                return new Vector3d(x / m, y / m, z / m);
            }
        }

        public static Vector3d Cross(Vector3d left, Vector3d right)
        {
            return new Vector3d(left.y * right.z - left.z * right.y,
                                left.z * right.x - left.x * right.z,
                                left.x * right.y - left.y * right.x);
        }

        public static double Dot(Vector3d left, Vector3d right)
        {
            return left.x * right.x + left.y * right.y + left.z * right.z;
        }

        public static Vector3d Lerp(Vector3d a, Vector3d b, double t)
        {
            t = Math.Max(0d, Math.Min(t, 1d));
            return new Vector3d(a.x * (1d - t) + b.x * t,
                                a.y * (1d - t) + b.y * t,
                                a.z * (1d - t) + b.z * t);
        }

        /// <summary>
        ///     Translate this vector by the virtual world origin to get the eqivalent vector with Unity's scene origin.
        /// </summary>
        /// <returns></returns>
        public Vector3 ToUnityOrigin()
        {
            return (Vector3) this;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3d))
                return false;
            var vector = (Vector3d) other;
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public bool Equals(Vector3d vector)
        {
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public bool Approximately(Vector3d vector)
        {
            return UMath.Approximately(x, vector.x) &&
                   UMath.Approximately(y, vector.y) &&
                   UMath.Approximately(z, vector.z);
        }

        public override string ToString()
        {
            return "Vector3d(" + x + " " + y + " " + z + ")";
        }

        public static bool operator ==(Vector3d a, Vector3d b)
        {
            return a.x == b.x &&
                   a.y == b.y &&
                   a.z == b.z;
        }

        public static bool operator !=(Vector3d a, Vector3d b)
        {
            return a.x != b.x ||
                   a.y != b.y ||
                   a.z != b.z;
        }

        public double this[int i] {
            get {
                if (i == 0)
                    return x;
                if (i == 1)
                    return y;
                if (i == 2)
                    return z;

                // ReSharper disable once HeapView.ObjectAllocation.Evident
                throw new ArgumentOutOfRangeException(string.Format("There is no value at {0} index.", i));
            }
            set {
                if (i == 0)
                    x = value;
                if (i == 1)
                    y = value;
                if (i == 2)
                    z = value;

                // ReSharper disable once HeapView.ObjectAllocation.Evident
                throw new ArgumentOutOfRangeException(string.Format("There is no value at {0} index.", i));
            }
        }

        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3d operator -(Vector3d a)
        {
            return new Vector3d(-a.x, -a.y, -a.z);
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3d operator *(Vector3d a, int b)
        {
            return new Vector3d(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3d operator *(int b, Vector3d a)
        {
            return new Vector3d(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3d operator /(Vector3d a, double b)
        {
            return new Vector3d(a.x / b, a.y / b, a.z / b);
        }

        public static Vector3d operator *(Vector3d a, double b)
        {
            return new Vector3d(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3d operator *(double b, Vector3d a)
        {
            return new Vector3d(a.x * b, a.y * b, a.z * b);
        }

        public static explicit operator Vector3(Vector3d v)
        {
            return new Vector3((float) v.x, (float) v.y, (float) v.z);
        }

        public static explicit operator Vector3d(Vector3 v)
        {
            return new Vector3d(v.x, v.y, v.z);
        }
    }
}