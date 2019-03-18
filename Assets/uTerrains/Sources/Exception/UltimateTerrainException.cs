using System;

namespace UltimateTerrains
{
    public class UltimateTerrainException : Exception
    {
        public UltimateTerrainException(string message, Exception inner) : base(message, inner)
        {
        }

        public UltimateTerrainException(string message) : base(message)
        {
        }
    }
}