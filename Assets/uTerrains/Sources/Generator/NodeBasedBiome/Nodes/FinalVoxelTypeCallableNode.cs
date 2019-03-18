namespace UltimateTerrains
{
    public class FinalVoxelTypeCallableNode : FinalCallableNode
    {
        private readonly VoxelType voxelType;

        public FinalVoxelTypeCallableNode(IGeneratorNode node, VoxelType voxelType) : base(node)
        {
            this.voxelType = voxelType;
        }

        public VoxelType VoxelType {
            get { return voxelType; }
        }
    }
}