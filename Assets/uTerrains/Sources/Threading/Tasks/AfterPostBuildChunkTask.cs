namespace UltimateTerrains
{
    internal sealed class AfterPostBuildChunkTask : ITask
    {
        private Chunk chunk;
        private Orchestrator orchestrator;

        public void Init(Chunk chunk, Orchestrator orchestrator)
        {
            Reset();
            this.chunk = chunk;
            this.orchestrator = orchestrator;
        }

        private void Reset()
        {
            chunk = null;
        }

        public void Execute(ThreadSpecificParams threadParams)
        {
            chunk.OnPostBuildDone(threadParams.chunkGenerator.ChunkGeneratorPools);
            orchestrator.DecrementChunksToBuildEntirelyCount();
        }
    }
}