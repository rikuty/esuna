namespace UltimateTerrains
{
    // NOT THREAD-SAFE! YOU MUST USE ONE GENERATOR INSTANCE PER THREAD
    internal sealed class VoxelGenerator
    {
        // Biome selector and biomes
        private GenerationFlow3D biomeSelectorFlow;

        // To perform operation on voxel
        private readonly UltimateOperationsManager operationsManager;

        // Temporary context
        private double vox2World;
        private int chunkLevel;
        private readonly UnsafeArrayPool<double> float2DModulesPool;
        private Vector3i chunkWorldPosition;
        private ChunkCache cache;
        private OperationList operationList;
        private bool hasActions;
        private int opCount;

        // Temporary variables
        private bool isCachable;

        //private short cachedVersion;
        //private int cpx, cpy, cpz;

        internal UltimateOperationsManager OperationManager {
            get { return operationsManager; }
        }

        internal VoxelGenerator(UltimateTerrain terrain, ChunkGeneratorPools pools)
        {
            operationsManager = terrain.OperationsManager;
            float2DModulesPool = pools.Float2DModulesPool;
            InitGenerationFlows(terrain);

            terrain.OnReloadForPreview += InitGenerationFlows;
        }

        private void InitGenerationFlows(UltimateTerrain terrain)
        {
            biomeSelectorFlow = new GenerationFlow3D(terrain, terrain.GeneratorModules.BiomeSelector.Graph3D);
        }

        internal void Prepare(ChunkState chunkState, OperationList operations, int operationsCount, ChunkCache chunkCache)
        {
            chunkWorldPosition = chunkState.WorldPosition;
            chunkLevel = chunkState.level;
            vox2World = chunkLevel * Param.SIZE_OFFSET_INVERSE;
            cache = chunkCache;
            operationList = operations;
            hasActions = operationList != null && operationsCount > 0;
            opCount = 0;
            if (hasActions) {
                opCount = operationsCount;
            }
        }

        internal void PrepareNode(bool isCacheUsable)
        {
            isCachable = isCacheUsable;
        }

        internal void PrepareWithoutChunk(OperationList operations, ChunkCache chunkCache)
        {
            vox2World = 1;
            chunkLevel = 1;
            cache = chunkCache;
            operationList = operations;
            hasActions = operationList != null && operationList.Count > 0;
            opCount = 0;
            if (hasActions) {
                opCount = operationList.Count;
            }

            PrepareNode(false);
        }

        internal Voxel GenerateNoGradient(Vector3i p)
        {
            return Generate(chunkWorldPosition.x + p.x * vox2World,
                            chunkWorldPosition.y + p.y * vox2World,
                            chunkWorldPosition.z + p.z * vox2World,
                            p.x, p.y, p.z);
        }

        internal Voxel GenerateWithoutChunk(Vector3i wp)
        {
            var p = UnitConverter.ToLocalPositionWithOffset(wp);
            return Generate(wp.x, wp.y, wp.z, p.x, p.y, p.z);
        }

        internal Voxel GenerateWithoutChunk(Vector3d wp)
        {
            var p = UnitConverter.ToLocalPositionWithOffset(new Vector3i(wp));
            return Generate(wp.x, wp.y, wp.z, p.x, p.y, p.z);
        }

        internal Voxel GenerateWithoutChunk(double wpx, double wpy, double wpz)
        {
            var p = UnitConverter.ToLocalPositionWithOffset(new Vector3i(wpx, wpy, wpz));
            return Generate(wpx, wpy, wpz, p.x, p.y, p.z);
        }

        private Voxel Generate(double wpx, double wpy, double wpz, int px, int py, int pz)
        {
            var voxel = new Voxel
            {
                Position =
                {
                    x = (short) px,
                    y = (short) py,
                    z = (short) pz
                }
            };
            var cpx = px >> Param.SIZE_OFFSET_BITS;
            var cpz = pz >> Param.SIZE_OFFSET_BITS;

            // Compute biome id ===========================================
            if (isCachable) {
                double[] selectorValues2D;
                if (!cache.TryGetBiomeSelectorValue2D(cpx, cpz, out selectorValues2D)) {
                    selectorValues2D = float2DModulesPool.Get();
                    biomeSelectorFlow.Compute2DValues(wpx, wpy, wpz, selectorValues2D);
                    cache.CacheBiomeSelectorValue2D(cpx, cpz, selectorValues2D);
                } else {
                    biomeSelectorFlow.Set2DValuesFromCacheAndComputeWhenNeeded(wpx, wpy, wpz, selectorValues2D);
                }
            } else {
                biomeSelectorFlow.Compute2DValuesNoCache(wpx, wpy, wpz);
            }

            double blend;
            int biomeId1, biomeId2;
            GenerationFlow3D biomeFlow1, biomeFlow2;
            biomeSelectorFlow.ExecuteAsBiomeSelector(wpx, wpy, wpz, out biomeId1, out biomeFlow1, out biomeId2, out biomeFlow2, out blend);
            // ============================================================

            // Execute the flow to get or compute 2D values ===============
            ComputeBiome2DValues(wpx, wpy, wpz, cpx, cpz, biomeId1, biomeFlow1);
            if (biomeId2 >= 0)
                ComputeBiome2DValues(wpx, wpy, wpz, cpx, cpz, biomeId2, biomeFlow2);
            // ============================================================

            // Execute the flow to get voxel value and voxel type =========
            double value1;
            VoxelType voxelType1;
            biomeFlow1.Execute(wpx, wpy, wpz, out value1, out voxelType1);

            if (biomeId2 >= 0) {
                double value2;
                VoxelType voxelType2;
                biomeFlow2.Execute(wpx, wpy, wpz, out value2, out voxelType2);
                voxel.Value = blend * value1 + (1 - blend) * value2;
                voxel.VoxelType = blend < 0.5 ? voxelType1 : voxelType2;
            } else {
                voxel.Value = value1;
                voxel.VoxelType = voxelType1;
            }

            // ============================================================

            // Once voxel is created, perform operations on it ============
            if (hasActions) {
                operationList.ActAll(opCount, wpx, wpy, wpz, ref voxel);
            }
            // ============================================================

            return voxel;
        }

        private void ComputeBiome2DValues(double wpx, double wpy, double wpz, int cpx, int cpz, int biomeId, GenerationFlow3D biomeFlow)
        {
            if (isCachable) {
                double[] values2D;
                if (!cache.TryGetValue2D(cpx, cpz, biomeId, out values2D)) {
                    values2D = float2DModulesPool.Get();
                    biomeFlow.Compute2DValues(wpx, wpy, wpz, values2D);
                    cache.CacheValue2D(cpx, cpz, biomeId, values2D);
                } else {
                    biomeFlow.Set2DValuesFromCacheAndComputeWhenNeeded(wpx, wpy, wpz, values2D);
                }
            } else {
                biomeFlow.Compute2DValuesNoCache(wpx, wpy, wpz);
            }
        }

        internal Vector3d ComputeGradientInChunk(Vector3d pos, double step = 0.001)
        {
            pos = pos * chunkLevel + chunkWorldPosition;
            Vector3d gradient;
            gradient.x = GenerateValue(pos.x + step, pos.y, pos.z) - GenerateValue(pos.x - step, pos.y, pos.z);
            gradient.y = GenerateValue(pos.x, pos.y + step, pos.z) - GenerateValue(pos.x, pos.y - step, pos.z);
            gradient.z = GenerateValue(pos.x, pos.y, pos.z + step) - GenerateValue(pos.x, pos.y, pos.z - step);
            return gradient.Normalized;
        }

        internal Vector3d ComputeGradient(Vector3d pos, double step = 0.001)
        {
            Vector3d gradient;
            gradient.x = GenerateValue(pos.x + step, pos.y, pos.z) - GenerateValue(pos.x - step, pos.y, pos.z);
            gradient.y = GenerateValue(pos.x, pos.y + step, pos.z) - GenerateValue(pos.x, pos.y - step, pos.z);
            gradient.z = GenerateValue(pos.x, pos.y, pos.z + step) - GenerateValue(pos.x, pos.y, pos.z - step);
            return gradient.Normalized;
        }


        private double GenerateValue(double wpx, double wpy, double wpz)
        {
            // Compute biome id
            biomeSelectorFlow.Compute2DValuesNoCache(wpx, wpy, wpz);

            double blend;
            int biomeId1, biomeId2;
            GenerationFlow3D biomeFlow1, biomeFlow2;
            biomeSelectorFlow.ExecuteAsBiomeSelector(wpx, wpy, wpz, out biomeId1, out biomeFlow1, out biomeId2, out biomeFlow2, out blend);

            biomeFlow1.Compute2DValuesNoCache(wpx, wpy, wpz);
            if (biomeId2 >= 0)
                biomeFlow2.Compute2DValuesNoCache(wpx, wpy, wpz);

            // Execute the flow to get voxel value and voxel type
            double value1;
            VoxelType voxelType1;
            biomeFlow1.Execute(wpx, wpy, wpz, out value1, out voxelType1);

            Voxel voxel;
            if (biomeId2 >= 0) {
                double value2;
                VoxelType voxelType2;
                biomeFlow2.Execute(wpx, wpy, wpz, out value2, out voxelType2);
                voxel = new Voxel {Value = blend * value1 + (1 - blend) * value2, VoxelType = blend < 0.5 ? voxelType1 : voxelType2};
            } else {
                voxel = new Voxel {Value = value1, VoxelType = voxelType1};
            }

            // Once voxel is created, perform operations on it
            if (hasActions) {
                operationList.ActAll(opCount, wpx, wpy, wpz, ref voxel);
            }

            return voxel.Value;
        }
    }
}