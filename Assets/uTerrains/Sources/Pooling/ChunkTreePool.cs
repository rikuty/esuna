using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UltimateTerrains
{
    internal sealed class ChunkTreePool
    {
        private readonly UnsafePool<Chunk> chunkPool;

        private readonly HashSet<Vector3i> reportBackOperationsTemporarySet = new HashSet<Vector3i>(new Vector3iComparer());

        internal UnsafePool<Chunk> ChunkPool {
            get { return chunkPool; }
        }

        internal HashSet<Vector3i> ReportBackOperationsTemporarySet {
            get { return reportBackOperationsTemporarySet; }
        }

        internal ChunkTreePool(UltimateTerrain terrain)
        {
            var p = terrain.Params;

            var totalChunkCount = p.ChunkTreeCount * p.LevelCount;
            chunkPool = new UnsafePool<Chunk>(() => new Chunk(), totalChunkCount, totalChunkCount);
        }

        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
        public void DebugLog()
        {
            UDebug.Log("-------------------------------------------------------------------------------");
            UDebug.Log("There are currently " + chunkPool.Count + "/" + chunkPool.TotalInstanciatedCount + " Chunk in pool.");
            UDebug.Log("-------------------------------------------------------------------------------");
        }
    }
}