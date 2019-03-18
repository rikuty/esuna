namespace UltimateTerrains
{
    internal sealed class SpawnTreesTask : ITask
    {
        private TreesChunk treesChunk;
        private Vector3i cameraChunkPosition;
        private Orchestrator orchestrator;

        public void Init(TreesChunk treesChunk, Vector3i cameraChunkPosition, Orchestrator orchestrator)
        {
            this.treesChunk = treesChunk;
            this.cameraChunkPosition = cameraChunkPosition;
            this.orchestrator = orchestrator;
        }

        public void Execute(ThreadSpecificParams threadParams)
        {
            UProfiler.Begin("Spawn trees, UpdatePosition");
            treesChunk.UpdatePosition(cameraChunkPosition, threadParams.chunkGenerator, threadParams.treesSpawnerPool);
            UProfiler.End();
            orchestrator.DecrementTreesChunkToUpdateCount();
        }
    }
}