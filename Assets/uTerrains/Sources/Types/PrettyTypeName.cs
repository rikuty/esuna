using System;

namespace UltimateTerrains
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PrettyTypeName : Attribute
    {
        public readonly string Name;

        public PrettyTypeName(string name)
        {
            Name = name;
        }
    }
}