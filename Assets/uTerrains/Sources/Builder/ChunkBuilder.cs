namespace UltimateTerrains
{
    internal sealed class ChunkBuilder
    {
        private readonly DualContouring dualContourer;
        private readonly MarchingCubesQEF marchingCubesQEF;
        private readonly QEF qef;

        public MarchingCubesQEF MarchingCubesQEF {
            get { return marchingCubesQEF; }
        }

        public QEF QEF {
            get { return qef; }
        }

        public ChunkBuilder(Param param, VoxelGenerator voxelGenerator, VoxelTypeSet voxelTypeSet)
        {
            dualContourer = new DualContouring(param);
            marchingCubesQEF = new MarchingCubesQEF(voxelGenerator, voxelTypeSet);
            qef = new QEF();
        }

        public void GenerateDualCountour(OctreeNode octree)
        {
            dualContourer.GenerateDualCountour(octree);
        }
    }
}