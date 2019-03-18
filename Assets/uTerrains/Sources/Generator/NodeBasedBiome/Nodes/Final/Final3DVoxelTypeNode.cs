namespace UltimateTerrains
{
    public class Final3DVoxelTypeNode : IFinalGeneratorNode
    {
        private readonly CallableNode input;
        private readonly VoxelType voxelType;

        public Final3DVoxelTypeNode(CallableNode input, VoxelType voxelType)
        {
            if (input == null) {
                throw new InvalidFlowException(string.Format("Node of type {0} misses some mandatory input(s).", this.GetType()));
            }

            this.input = input;
            this.voxelType = voxelType;
        }

        public bool Is2D {
            get { return false; }
        }

        public VoxelType VoxelType {
            get { return voxelType; }
        }

        public double Execute(double x, double y, double z, CallableNode flow)
        {
            return flow.Call(input, x, y, z);
        }
    }
}