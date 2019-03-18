using System;

namespace UltimateTerrains
{
    [Serializable]
    public sealed class VoxelType
    {
        // Common properties
        public string Name = "New Type";
        public ushort MaterialIndex = 0;
        public AbstractVoxelTypeFunctions Functions;
        public bool IsGrassEnabled;
        public int Priority = 1;
        public float Blockiness = 0f;
        public GrassParam GrassParam;
        public bool[] EnabledDetails;
        public int EnabledTreesIndex = -1;

        public ushort Index;

        public int DetailsCount {
            get { return EnabledDetails != null ? EnabledDetails.Length : 0; }
        }

        public Type GetFunctionsType()
        {
            return Functions.GetType();
        }
    }
}