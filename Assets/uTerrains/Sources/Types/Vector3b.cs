using System;
using Newtonsoft.Json;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Vector3b
    {
        [JsonProperty] public sbyte x;
        [JsonProperty] public sbyte y;
        [JsonProperty] public sbyte z;

        public Vector3b(sbyte x, sbyte y, sbyte z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3b(int x, int y, int z)
        {
            this.x = (sbyte) x;
            this.y = (sbyte) y;
            this.z = (sbyte) z;
        }

        public Vector3b(Vector3 v)
        {
            x = (sbyte) v.x;
            y = (sbyte) v.y;
            z = (sbyte) v.z;
        }

        public Vector3b(Vector3i v)
        {
            x = (sbyte) v.x;
            y = (sbyte) v.y;
            z = (sbyte) v.z;
        }

        public override int GetHashCode()
        {
            return x ^ (y << 2) ^ (z >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3b))
                return false;
            var vector = (Vector3b) other;
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public bool Equals(Vector3b vector)
        {
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public override string ToString()
        {
            return "Vector3b(" + x + " " + y + " " + z + ")";
        }

        public string ToShortString()
        {
            return "(" + x + " " + y + " " + z + ")";
        }

        public static bool operator ==(Vector3b a, Vector3b b)
        {
            return a.x == b.x &&
                   a.y == b.y &&
                   a.z == b.z;
        }

        public static bool operator !=(Vector3b a, Vector3b b)
        {
            return a.x != b.x ||
                   a.y != b.y ||
                   a.z != b.z;
        }

        public sbyte this[int i] {
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

        public static Vector3b operator -(Vector3b a, Vector3b b)
        {
            return new Vector3b(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3b operator -(Vector3b a)
        {
            return new Vector3b(-a.x, -a.y, -a.z);
        }

        public static Vector3b operator +(Vector3b a, Vector3b b)
        {
            return new Vector3b(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3b operator *(Vector3b a, sbyte b)
        {
            return new Vector3b(a.x * b, a.y * b, a.z * b);
        }

        public static implicit operator Vector3(Vector3b v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector3i(Vector3b v)
        {
            return new Vector3i(v.x, v.y, v.z);
        }
    }
}