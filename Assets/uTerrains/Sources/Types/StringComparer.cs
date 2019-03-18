using System.Collections.Generic;

namespace UltimateTerrains
{
    public sealed class StringComparer : IEqualityComparer<string>
    {
        public int GetHashCode(string s)
        {
            return s.GetHashCode();
        }

        public bool Equals(string s1, string s2)
        {
            return s1.Equals(s2);
        }
    }
}