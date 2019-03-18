namespace UltimateTerrains
{
    internal sealed class BuildChunkForOperationTask : ITask
    {
        private Chunk chunk;
        private AsyncOperationOrchestrator asyncOperationOrchestrator;
        private bool clearCache;
        private readonly ChunkState chunkState;
        private readonly ChunkGenerator.Result result;

        public BuildChunkForOperationTask()
        {
            chunkState = new ChunkState();
            result = new ChunkGenerator.Result();
        }

        public void Init(Chunk chunk, AsyncOperationOrchestrator asyncOperationOrchestrator, bool clearCache)
        {
            Reset();
            this.chunk = chunk;
            chunk.FillStateData(chunkState);
            this.asyncOperationOrchestrator = asyncOperationOrchestrator;
            this.clearCache = clearCache;
        }

        private void Reset()
        {
            chunk = null;
            chunkState.Reset();
            result.Reset();
        }

        public void Execute(ThreadSpecificParams threadParams)
        {
            var generator = threadParams.chunkGenerator;
            var cache = clearCache ? null : chunk.GetCacheCopy(generator.ChunkGeneratorPools);
            generator.GenerateAndBuild(chunkState, result, cache);

            generator.ApplyResult(result, chunk);
            asyncOperationOrchestrator.EnqueueChunkToPostBuildOrFree(chunk);
            asyncOperationOrchestrator.DecrementChunksToBuildForOperationCount();
        }
    }
}