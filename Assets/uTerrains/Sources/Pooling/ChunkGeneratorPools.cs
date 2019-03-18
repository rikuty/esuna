using System.Diagnostics.CodeAnalysis;
using UnityEngine.Profiling;

namespace UltimateTerrains
{
    internal sealed class ChunkGeneratorPools
    {
        private const int MeshInitialListsSize = 2048;
        private const int GrassmeshInitialListsSize = 16384;
        private const int MeshDataInitialCount = 4;
        private const int GrassMeshDataInitialCount = 4;

        private const int OctreenodePoolInitialCount = Param.SIZE_3D_TOTAL;

        private const int VertexdataPoolInitialCount = MeshInitialListsSize + GrassmeshInitialListsSize;


        // THREAD CHUNK GENERATOR ---------------------------------------------

        private readonly UnsafePool<VertexData> vertexDataPool;

        internal UnsafePool<VertexData> VertexDataPool {
            get { return vertexDataPool; }
        }

        private readonly MeshDataPool meshDataPool;

        internal MeshDataPool MeshDataPool {
            get { return meshDataPool; }
        }

        private readonly MeshDataPool grassMeshDataPool;

        internal MeshDataPool GrassMeshDataPool {
            get { return grassMeshDataPool; }
        }

        private readonly UnsafePool<ChunkCache> chunkCachePool;

        internal UnsafePool<ChunkCache> ChunkCachePool {
            get { return chunkCachePool; }
        }

        private readonly UnsafePool<OctreeNode> octreeNodePool;

        internal UnsafePool<OctreeNode> OctreeNodePool {
            get { return octreeNodePool; }
        }

        private readonly UnsafeStackPool<OctreeNode> octreeNodeStackPool;

        internal UnsafeStackPool<OctreeNode> OctreeNodeStackPool {
            get { return octreeNodeStackPool; }
        }

        private readonly UnsafeArrayPool<double> float2DModulesPool;

        internal UnsafeArrayPool<double> Float2DModulesPool {
            get { return float2DModulesPool; }
        }

        private readonly DetailsPool detailsPool;

        internal DetailsPool DetailsPool {
            get { return detailsPool; }
        }

        private readonly MeshDataListsPools meshDataListsPools;

        internal MeshDataListsPools MeshDataListsPools {
            get { return meshDataListsPools; }
        }

        private readonly MeshDataListsPools grassMeshDataListsPools;

        internal MeshDataListsPools GrassMeshDataListsPools {
            get { return grassMeshDataListsPools; }
        }
        // --------------------------------------------------------------------------------------


        public ChunkGeneratorPools(UltimateTerrain uTerrain)
        {
            var p = uTerrain.Params;

            var chunkCacheCount = 1; // start at 1 because there's always at least one working cache
            for (var lvl = 0; lvl < p.MaxLevelWithCache; ++lvl) {
                chunkCacheCount += uTerrain.Params.InitialChunkCountLevel[lvl];
            }

            var values2DArraysCount = Param.CACHE_2D_SIZE * chunkCacheCount;

            var detailsCount = 0;
            for (var lvl = 0; lvl < p.MaxLevelIndexDetails; ++lvl) {
                detailsCount += uTerrain.Params.InitialChunkCountLevel[lvl];
            }

            var threadCount = UltimateTerrain.ThreadCount + 1;

            UProfiler.BeginMemory("Create vertexDataPool");
            vertexDataPool = new UnsafePool<VertexData>(() => new VertexData(), VertexdataPoolInitialCount, VertexdataPoolInitialCount);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create meshDataPool");
            var materialsCount = uTerrain.VoxelTypeSet.MaterialsCount;
            meshDataPool = new MeshDataPool(materialsCount, MeshInitialListsSize, MeshDataInitialCount, MeshDataInitialCount);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create grassMeshDataPool");
            var grassMaterialsCount = uTerrain.VoxelTypeSet.GrassMaterials.Length;
            grassMeshDataPool = new MeshDataPool(grassMaterialsCount, GrassmeshInitialListsSize, GrassMeshDataInitialCount, GrassMeshDataInitialCount);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create chunkCachePool");
            chunkCachePool = new UnsafePool<ChunkCache>(() => new ChunkCache(uTerrain), chunkCacheCount / threadCount, chunkCacheCount / threadCount);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create octreeNodePool");
            octreeNodePool = new UnsafePool<OctreeNode>(() => new OctreeNode(), OctreenodePoolInitialCount, OctreenodePoolInitialCount);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create octreeNodeStackPool");
            octreeNodeStackPool = new UnsafeStackPool<OctreeNode>(256, 1, 1);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create float2DModulesPool");
            float2DModulesPool = new UnsafeArrayPool<double>(Param.Max2DModulesCount, values2DArraysCount / threadCount, values2DArraysCount / threadCount);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create detailsPool");
            detailsPool = new DetailsPool(uTerrain, detailsCount / threadCount, detailsCount / threadCount);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create meshDataListsPools");
            meshDataListsPools = new MeshDataListsPools(2 * threadCount, MeshInitialListsSize);
            UProfiler.EndMemory();

            UProfiler.BeginMemory("Create grassMeshDataListsPools");
            grassMeshDataListsPools = new MeshDataListsPools(1 * threadCount, GrassmeshInitialListsSize);
            UProfiler.EndMemory();
        }
    }
}