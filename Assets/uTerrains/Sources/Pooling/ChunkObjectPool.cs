using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UltimateTerrains
{
    internal static class ChunkObjectPool
    {
        private static bool warnedOnce;

        private static UltimateTerrain terrain;
        private static Queue<ChunkComponent>[] freeChunks;

        private static UnsafePool<Mesh> meshPool;

        internal static UnsafePool<Mesh> MeshPool {
            get { return meshPool; }
        }

        public static void Init(UltimateTerrain uTerrain)
        {
            terrain = uTerrain;
            var p = terrain.Params;

            var meshCount = 0;
            for (var lvl = 0; lvl < p.LevelCount; ++lvl) {
                meshCount += uTerrain.Params.InitialChunkCountLevel[lvl];
            }

            var grassMeshDataCount = 0;
            for (var lvl = 0; lvl < p.MaxLevelIndexGrass; ++lvl) {
                grassMeshDataCount += uTerrain.Params.InitialChunkCountLevel[lvl];
            }

            meshPool = new UnsafePool<Mesh>(() => new Mesh(), meshCount + grassMeshDataCount, meshCount);
            freeChunks = new Queue<ChunkComponent>[terrain.ChunkLevelCount];
            var totalCount = 0;

            var watch = Stopwatch.StartNew();
            for (var lvl = 0; lvl < terrain.ChunkLevelCount; ++lvl) {
                var cCount = uTerrain.Params.InitialChunkCountLevel[lvl];
                totalCount += cCount;
                var freeChunksOfLevel = new Queue<ChunkComponent>(cCount);
                freeChunks[lvl] = freeChunksOfLevel;
                for (var i = 0; i < cCount; ++i) {
                    var chunkObject = ChunkComponent.CreateChunkObject(UnitConverter.LevelIndexToLevel(lvl), terrain);
                    freeChunksOfLevel.Enqueue(chunkObject);
                }
            }

            watch.Stop();
            UDebug.Log(string.Format("Took {0}ms to initialize {1} chunk objects.", watch.ElapsedMilliseconds, totalCount));
        }

        public static void Reset()
        {
            terrain = null;
            freeChunks = null;
        }

        public static ChunkComponent Use(Chunk chunk)
        {
            ChunkComponent chunkObject;
            var freeChunksOfLevel = freeChunks[UnitConverter.LevelToLevelIndex(chunk.Level)];
            if (freeChunksOfLevel.Count == 0) {
                if (!warnedOnce) {
                    warnedOnce = true;
                    var lvl = UnitConverter.LevelToLevelIndex(chunk.Level) + 1;
                    UDebug.LogWarning(string.Format("Not enough chunks of level {0}. You should increase the initial chunk count of LOD {0}.", lvl));
                }

                chunkObject = ChunkComponent.CreateChunkObject(chunk.Level, terrain);
            } else {
                chunkObject = freeChunksOfLevel.Dequeue();
            }

            chunkObject.SetUsedBy(chunk);
            chunkObject.AffectNewPosition();
            return chunkObject;
        }

        public static void Free(ChunkComponent chunkObject)
        {
            if (!chunkObject.IsFree) {
                if (chunkObject.IsBelongingToItsChunk) {
                    UDebug.LogError("Trying to free a ChunkObject that is still used by its Chunk");
                }

                chunkObject.SetFree();
                freeChunks[UnitConverter.LevelToLevelIndex(chunkObject.Level)].Enqueue(chunkObject);
            } else {
                UDebug.LogError("Trying to free a ChunkObject that is already free");
            }
        }
    }
}