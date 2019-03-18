using UnityEngine;

namespace UltimateTerrains
{
    internal sealed class ChunkGenerator
    {
        private readonly VoxelTypeSet voxelTypeSet;
        private readonly UnitConverter unitConverter;
        private readonly Param param;
        private readonly bool duplicateVertices;
        private readonly bool computeTangentsOnMeshes;

        private readonly VoxelGenerator generator;
        private readonly ChunkBuilder builder;
        private readonly ChunkGeneratorPools chunkGeneratorPools;
        private readonly GrassGenerator grassGenerator;
        private readonly MegaSplatMeshProcessor megaSplatMeshProcessor;

        internal ChunkGeneratorPools ChunkGeneratorPools {
            get { return chunkGeneratorPools; }
        }

        internal VoxelGenerator VoxelGenerator {
            get { return generator; }
        }

        public sealed class Result
        {
            public long buildId;
            public MeshData meshData;
            public ChunkCache chunkCache;
            public MeshData grassMeshData;
            public Details details;
            public bool mayContainSurface;

            internal void Reset()
            {
                buildId = -1;
                meshData = null;
                chunkCache = null;
                grassMeshData = null;
                details = null;
                mayContainSurface = false;
            }
        }

        internal ChunkGenerator(UltimateTerrain terrain)
        {
            voxelTypeSet = terrain.VoxelTypeSet;
            param = terrain.Params;
            duplicateVertices = param.DuplicateVertices;
            computeTangentsOnMeshes = param.ComputeTangentsOnMeshes;
            unitConverter = terrain.Converter;
            chunkGeneratorPools = new ChunkGeneratorPools(terrain);
            generator = new VoxelGenerator(terrain, chunkGeneratorPools);
            builder = new ChunkBuilder(terrain.Params, generator, terrain.VoxelTypeSet);
            grassGenerator = new GrassGenerator(terrain);
            megaSplatMeshProcessor = new MegaSplatMeshProcessor();
        }

        internal bool ApplyResult(Result result, Chunk chunk)
        {
            return chunk.ApplyBuildResult(result, chunkGeneratorPools);
        }

        /// <summary>
        ///     Generates the and build the chunk on a dedicated thread.
        /// </summary>
        /// <param name="generator">Generator.</param>
        /// <param name="builder">Builder.</param>
        internal void GenerateAndBuild(ChunkState chunkState, Result result, ChunkCache cacheCopy)
        {
            result.buildId = chunkState.buildId;

            // Prepare cache
            UProfiler.Begin("Prepare cache");
            var bCache = cacheCopy ?? chunkGeneratorPools.ChunkCachePool.Get();
            UProfiler.End();

            // Generate octree
            UProfiler.Begin("Generate octree");
            var bOctree = GenerateOctree(chunkState, bCache, chunkGeneratorPools, generator, builder);
            result.mayContainSurface = bOctree.HasNodesNearSurface;
            UProfiler.End();

            // Build chunk (compute new mesh data)
            UProfiler.Begin("Build chunk");
            MeshData bMeshData = bOctree.Mesh;
            if (bMeshData.VertexCount > 0) {
                builder.GenerateDualCountour(bOctree);
            }

            bOctree.Free(chunkGeneratorPools.OctreeNodePool);
            UProfiler.End();


            UProfiler.Begin("Eventually clear cache");
            if (chunkState.ShouldKeepCache(param) && bMeshData.HasTriangles) {
                result.chunkCache = bCache;
            } else {
                bCache.ResetCache(chunkGeneratorPools.Float2DModulesPool);
                chunkGeneratorPools.ChunkCachePool.Free(bCache);
                result.chunkCache = null;
            }

            UProfiler.End();

            // Post process mesh data
            UProfiler.Begin("Post process mesh data");
            if (bMeshData.HasTriangles) {
                result.mayContainSurface = true;
                result.meshData = bMeshData;

                if (duplicateVertices) {
                    UProfiler.Begin("SolveNormals");
                    BuildHelper.SolveNormals(bMeshData, chunkGeneratorPools.MeshDataListsPools, chunkState.level);
                    UProfiler.End();
                }

                if (computeTangentsOnMeshes) {
                    UProfiler.Begin("SolveTangents");
                    BuildHelper.SolveTangents(bMeshData, chunkGeneratorPools.MeshDataListsPools);
                    UProfiler.End();
                }

                UProfiler.Begin("BuildGrassAndDetails");
                BuildGrassAndDetails(chunkState, bMeshData, chunkGeneratorPools, ref result);
                UProfiler.End();

                UProfiler.Begin("PrepareToMesh");
                bMeshData.PrepareToMesh(chunkGeneratorPools.MeshDataListsPools, chunkGeneratorPools.VertexDataPool, voxelTypeSet, megaSplatMeshProcessor);
                UProfiler.End();
            } else {
                bMeshData.Free(chunkGeneratorPools, chunkGeneratorPools.MeshDataListsPools, chunkGeneratorPools.MeshDataPool);
                result.meshData = null;
            }

            //chunkGeneratorPools.DebugLog();
            UProfiler.End();
        }

        private OctreeNode GenerateOctree(ChunkState chunkState, ChunkCache cache, ChunkGeneratorPools pools, VoxelGenerator generator, ChunkBuilder builder)
        {
            var octree = pools.OctreeNodePool.Get();
            octree.PreInitRoot(chunkState, unitConverter, param, voxelTypeSet, pools, generator, cache, octree);
            builder.MarchingCubesQEF.ResetCache();
            OctreeNode.BuildTree(octree, pools.OctreeNodeStackPool, generator, builder);
            return octree;
        }

        private void BuildGrassAndDetails(ChunkState chunkState, MeshData meshData, ChunkGeneratorPools pools, ref Result result)
        {
            if (chunkState.level <= param.MaxLevelGrass || chunkState.level <= param.MaxLevelDetails) {
                MeshData grassMeshData;
                Details details;
                grassGenerator.GenerateGrassMesh(
                    chunkState.level <= param.MaxLevelGrass, out grassMeshData,
                    chunkState.level <= param.MaxLevelDetails, out details,
                    chunkState.WorldPosition, chunkState.level, meshData, pools);
                result.grassMeshData = grassMeshData;
                result.details = details;
            }
        }
    }
}