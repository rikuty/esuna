namespace UltimateTerrains
{
    internal sealed class AfterPostBuildChunkForOperationTask : ITask
    {
        private Chunk chunk;

        public void Init(Chunk chunk)
        {
            Reset();
            this.chunk = chunk;
        }

        private void Reset()
        {
            chunk = null;
        }

        public void Execute(ThreadSpecificParams threadParams)
        {
            chunk.OnPostBuildDone(threadParams.chunkGenerator.ChunkGeneratorPools);
        }
    }
}