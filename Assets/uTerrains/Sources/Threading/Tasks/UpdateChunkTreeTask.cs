namespace UltimateTerrains
{
    internal sealed class UpdateChunkTreeTask : ITask
    {
        private Chunk chunkRoot;
        private bool isTerrainInfinite;
        private int? forcedLevel;
        private Vector3i cameraChunkPosition;
        private Orchestrator orchestrator;

        public void Init(Chunk chunkRoot, bool isTerrainInfinite, Vector3i cameraChunkPosition, Orchestrator orchestrator, int? forcedLevel)
        {
            this.forcedLevel = forcedLevel;
            this.chunkRoot = chunkRoot;
            this.isTerrainInfinite = isTerrainInfinite;
            this.cameraChunkPosition = cameraChunkPosition;
            this.orchestrator = orchestrator;
        }

        public void Execute(ThreadSpecificParams threadParams)
        {
            var chunkTreePool = threadParams.chunkTreePool;
            var chunkGeneratorPools = threadParams.chunkGenerator.ChunkGeneratorPools;
            if (isTerrainInfinite) {
                UProfiler.Begin("UpdatePositionOfRoot");
                chunkRoot.UpdatePositionOfRoot(chunkTreePool, chunkGeneratorPools, cameraChunkPosition);
                UProfiler.End();
            }

            UProfiler.Begin("GenerateChunkTree");
            chunkRoot.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
            UProfiler.End();
            orchestrator.DecrementChunkRootsToUpdateCount();

            //chunkTreePool.DebugLog();
        }
    }
}