using System;

namespace UltimateTerrains
{
    public class InvalidFlowException : UltimateTerrainException
    {
        public InvalidFlowException(string message, Exception inner) : base(message, inner)
        {
        }

        public InvalidFlowException(string message) : base(message)
        {
        }
    }
}