namespace UltimateTerrains
{
    internal sealed class BuildChunkTask : ITask
    {
        private Chunk chunk;
        private Orchestrator orchestrator;
        private bool clearCache;
        private readonly ChunkState chunkState;
        private readonly ChunkGenerator.Result result;

        public BuildChunkTask()
        {
            chunkState = new ChunkState();
            result = new ChunkGenerator.Result();
        }

        public void Init(Chunk chunk, Orchestrator orchestrator, bool clearCache)
        {
            Reset();
            this.chunk = chunk;
            chunk.FillStateData(chunkState);
            this.orchestrator = orchestrator;
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

            if (generator.ApplyResult(result, chunk)) {
                orchestrator.EnqueueChunkToPostBuild(chunk);
            } else {
                chunk.EnqueueChunkObjectForFree();
            }

            orchestrator.DecrementChunksToBuildEntirelyCount();

            //generator.ChunkGeneratorPools.DebugLog();
        }
    }
}