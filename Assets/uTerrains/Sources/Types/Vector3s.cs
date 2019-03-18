using System;
using Newtonsoft.Json;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Vector3s
    {
        [JsonProperty] public short x;
        [JsonProperty] public short y;
        [JsonProperty] public short z;

        public Vector3s(short x, short y, short z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3s(int x, int y, int z)
        {
            this.x = (short) x;
            this.y = (short) y;
            this.z = (short) z;
        }

        public Vector3s(Vector3 v)
        {
            x = (short) v.x;
            y = (short) v.y;
            z = (short) v.z;
        }

        public Vector3s(Vector3i v)
        {
            x = (short) v.x;
            y = (short) v.y;
            z = (short) v.z;
        }

        public override int GetHashCode()
        {
            return x ^ (y << 2) ^ (z >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3s))
                return false;
            var vector = (Vector3s) other;
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public bool Equals(Vector3s vector)
        {
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public override string ToString()
        {
            return "Vector3s(" + x + " " + y + " " + z + ")";
        }

        public string ToShortString()
        {
            return "(" + x + " " + y + " " + z + ")";
        }

        public static bool operator ==(Vector3s a, Vector3s b)
        {
            return a.x == b.x &&
                   a.y == b.y &&
                   a.z == b.z;
        }

        public static bool operator !=(Vector3s a, Vector3s b)
        {
            return a.x != b.x ||
                   a.y != b.y ||
                   a.z != b.z;
        }

        public short this[int i] {
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

        public static Vector3s operator -(Vector3s a, Vector3s b)
        {
            return new Vector3s(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3s operator -(Vector3s a)
        {
            return new Vector3s(-a.x, -a.y, -a.z);
        }

        public static Vector3s operator +(Vector3s a, Vector3s b)
        {
            return new Vector3s(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3s operator *(Vector3s a, short b)
        {
            return new Vector3s(a.x * b, a.y * b, a.z * b);
        }

        public static implicit operator Vector3(Vector3s v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector3i(Vector3s v)
        {
            return new Vector3i(v.x, v.y, v.z);
        }
    }
}