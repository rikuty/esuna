using System.Collections.Generic;
using LibNoise.Primitive;
using UnityEngine;

namespace UltimateTerrains
{
    public sealed class TreesChunk
    {
        private static readonly SimplexPerlin SimplexDensity = new SimplexPerlin();
        private static readonly SimplexPerlin SimplexTreeType = new SimplexPerlin();
        private static readonly SimplexPerlin SimplexScale = new SimplexPerlin();

        private Vector3i treesChunkPosition;
        private Vector3i positionAsVoxelPosition;
        private Vector3i size;
        private int step;
        private double stepNoiseFrequency;
        private int verticalStep;
        private UltimateTerrain terrain;
        private VoxelTypeSet voxelTypeSet;
        private UltimateOperationsManager operationsManager;
        private TreesParam[] treeParams;

        private List<TreeEntry> trees;

        private bool firstRun;

        internal void Init(UltimateTerrain uTerrain, Vector2i pos)
        {
            this.terrain = uTerrain;
            voxelTypeSet = uTerrain.VoxelTypeSet;
            treeParams = uTerrain.Params.TreesParams;
            operationsManager = uTerrain.OperationsManager;
            trees = new List<TreeEntry>();

            size = uTerrain.Params.MaxLevelTrees * new Vector3i(Param.SIZE_X, Param.SIZE_Y * uTerrain.Params.VerticalBuildDistance, Param.SIZE_Z);
            step = uTerrain.Params.TreeDensityStep;
            stepNoiseFrequency = uTerrain.Params.TreeDensityStepNoiseFrequency;
            verticalStep = uTerrain.Params.TreeVerticalStep;

            treesChunkPosition = new Vector3i(pos.x, 0, pos.y);
            positionAsVoxelPosition = new Vector3i(size.x * treesChunkPosition.x,
                                                   terrain.Params.MaxLevelTrees * Param.SIZE_Y * treesChunkPosition.y,
                                                   size.z * treesChunkPosition.z) / terrain.Params.MaxLevelTrees;
            firstRun = true;
        }

        private void Add(TreesSpawnerPool treesSpawnerPool, int indexOfObject, Vector3d position, Vector3 rotation, Vector3 scale)
        {
            var entry = treesSpawnerPool.TreeEntryPool.Get();
            entry.IndexOfObject = indexOfObject;
            entry.Position = position;
            entry.Rotation = rotation;
            entry.Scale = scale;
            trees.Add(entry);
            terrain.Orchestrator.EnqueueTreeToSpawn(entry);
        }

        private void FreeAll(TreesSpawnerPool treesSpawnerPool)
        {
            foreach (var tree in trees) {
                if (tree.TreeObject != null) {
                    terrain.Orchestrator.EnqueueTreeToFree(tree.TreeObject);
                    tree.TreeObject = null;
                }

                tree.IndexOfObject = -1;
                treesSpawnerPool.TreeEntryPool.Free(tree);
            }

            trees.Clear();
        }

        internal void UpdatePosition(Vector3i playerChunkPos, ChunkGenerator chunkGenerator, TreesSpawnerPool treesSpawnerPool)
        {
            var np = treesChunkPosition;
            var dx = np.x - playerChunkPos.x;
            var dz = np.z - playerChunkPos.z;

            var doChangePosition = false;
            var buildTreeDistance = terrain.Params.TreesBuildDistanceChunk;

            while (dx < -buildTreeDistance || dx >= buildTreeDistance ||
                   dz < -buildTreeDistance || dz >= buildTreeDistance) {
                if (dx < -buildTreeDistance)
                    np.x += 2 * buildTreeDistance;
                else if (dx >= buildTreeDistance)
                    np.x -= 2 * buildTreeDistance;

                if (dz < -buildTreeDistance)
                    np.z += 2 * buildTreeDistance;
                else if (dz >= buildTreeDistance)
                    np.z -= 2 * buildTreeDistance;

                doChangePosition = true;
                dx = np.x - playerChunkPos.x;
                dz = np.z - playerChunkPos.z;
            }

            if (doChangePosition || firstRun) {
                firstRun = false;
                np.y = playerChunkPos.y;
                AffectNewPosition(chunkGenerator, np, treesSpawnerPool);
            }
        }

        private void AffectNewPosition(ChunkGenerator chunkGenerator, Vector3i newPosition, TreesSpawnerPool treesSpawnerPool)
        {
            FreeAll(treesSpawnerPool);
            treesChunkPosition = newPosition;
            positionAsVoxelPosition = new Vector3i(size.x * newPosition.x,
                                                   terrain.Params.MaxLevelTrees * Param.SIZE_Y * newPosition.y,
                                                   size.z * newPosition.z) / terrain.Params.MaxLevelTrees;
            GenerateEntries(chunkGenerator, treesSpawnerPool);
        }

        private void GenerateEntries(ChunkGenerator chunkGenerator, TreesSpawnerPool treesSpawnerPool)
        {
            var noisedStep = step;
            for (var nx = positionAsVoxelPosition.x; nx < positionAsVoxelPosition.x + size.x; nx += noisedStep) {
                for (var nz = positionAsVoxelPosition.z; nz < positionAsVoxelPosition.z + size.z; nz += noisedStep) {
                    noisedStep = (int) (step * (SimplexDensity.GetValue(nx * stepNoiseFrequency, nz * stepNoiseFrequency) + 2f));
                    var x = (int) (nx + (BevinsValue.ValueNoise1D(nx + (nx + nz * 9) % 5) + 1) * 0.4 * noisedStep);
                    var z = (int) (nz + (BevinsValue.ValueNoise1D(nz + (nx * 7 - nz) % 6) + 1) * 0.4 * noisedStep);
                    if (positionAsVoxelPosition.x <= x && x < positionAsVoxelPosition.x + size.x && positionAsVoxelPosition.z <= z && z < positionAsVoxelPosition.z + size.z) {
                        var wpos = new Vector3i(x, positionAsVoxelPosition.y + size.y, z);
                        Vector3i voxelPos;
                        Vector3 hitNormal;
                        ushort voxelTypeIndex;
                        if (RaycastVoxelDown(chunkGenerator, wpos, 2 * size.y, verticalStep, out voxelPos, out hitNormal, out voxelTypeIndex)) {
                            TryAddSomeTreeAt(treesSpawnerPool, voxelTypeIndex, terrain.Converter.VoxelToUnityPosition(voxelPos), hitNormal);
                        }
                    }
                }
            }
        }

        private void TryAddSomeTreeAt(TreesSpawnerPool treesSpawnerPool, ushort voxelTypeIndex, Vector3d position, Vector3 normal)
        {
            var voxelType = voxelTypeSet.GetVoxelType(voxelTypeIndex);
            if (voxelType.EnabledTreesIndex >= 0 && voxelType.EnabledTreesIndex < treeParams.Length) {
                var param = treeParams[voxelType.EnabledTreesIndex];

                if (normal.y > param.MinNormalY) {
                    AddTreeAt(treesSpawnerPool, param, position);
                }
            }
        }

        private void AddTreeAt(TreesSpawnerPool treesSpawnerPool, TreesParam param, Vector3d position)
        {
            var objectCount = param.ObjectCount;
            if (objectCount > 0) {
                var noise = (SimplexTreeType.GetValue(position.x * param.ObjectsNoiseFrequency, position.z * param.ObjectsNoiseFrequency) + 1.0) * 0.5;
                for (var i = 0; i < objectCount; ++i) {
                    var obj = param.Objects[i];
                    if (obj.ObjectProbability > noise) {
                        var rot = param.Rotate ? new Vector3(0, 360f / 16f * ((int) (position.x + position.z) % 17), 0) : Vector3.zero;
                        var scale = Vector3.one * Mathf.Clamp(
                                        (float) (SimplexScale.GetValue(position.x * param.ScaleNoiseFrequency, position.z * param.ScaleNoiseFrequency) + 1.0) * 0.5f *
                                        (param.MaxScale - param.MinScale) + param.MinScale,
                                        param.MinScale, param.MaxScale);
                        Add(treesSpawnerPool, obj.Index, position, rot, scale);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Computes voxel at given voxel-world-position and returns it.
        /// </summary>
        /// <remarks>
        ///     Don't use it to iterate over voxels because it may be slow.
        /// </remarks>
        /// <param name="worldVoxelPosition">
        ///     World voxel position (use terrain.Converter.UnityToVoxelPositionRound or
        ///     terrain.Converter.UnityToVoxelPositionFloor if needed)
        /// </param>
        /// <param name="withGradient">If true, the gradient of the voxel will be computed (about 6 times slower to compute)</param>
        /// <returns>The voxel at the given world position. Can't be null.</returns>
        private bool RaycastVoxelDown(ChunkGenerator chunkGenerator, Vector3i worldVoxelPosition, int distance, int minOutsideLength,
                                       out Vector3i hitPoint, out Vector3 hitNormal, out ushort voxelTypeIndex)
        {
            
            Vector3i? currentColumnLeveledWorldPos = null;
            var rayStep = minOutsideLength - 1;

            var lastInsideY = worldVoxelPosition.y;
            var wpos = worldVoxelPosition;
            for (var y = worldVoxelPosition.y; y >= worldVoxelPosition.y - distance; y -= rayStep) {
                var columnChunkWorldPos = UnitConverter.VoxelToChunkPosition(new Vector3i(wpos.x, y, wpos.z));
                var columnLeveledWorldPos = UnitConverter.ChunkToLeveledChunkPosition(columnChunkWorldPos, terrain.Params.MaxLevelTrees);
                if (!currentColumnLeveledWorldPos.HasValue || currentColumnLeveledWorldPos.Value != columnLeveledWorldPos) {
                    currentColumnLeveledWorldPos = columnLeveledWorldPos;
                    var operations = operationsManager.GetOperations(columnLeveledWorldPos, terrain.Params.MaxLevelIndexTrees - 1);
                    chunkGenerator.VoxelGenerator.PrepareWithoutChunk(operations, null);
                }

                wpos.y = y;
                var voxel = chunkGenerator.VoxelGenerator.GenerateWithoutChunk(wpos);
                if (voxel.IsInside && y < lastInsideY - minOutsideLength) {
                    lastInsideY = y;
                    var d = Mathf.Min(rayStep + minOutsideLength, worldVoxelPosition.y - y);
                    if (RaycastVoxelUp(chunkGenerator, wpos, d, minOutsideLength, out hitPoint, out hitNormal, out voxelTypeIndex)) {
                        return true;
                    }
                }
            }

            hitPoint = Vector3i.zero;
            hitNormal = Vector3.zero;
            voxelTypeIndex = 0;
            return false;
        }

        private static bool RaycastVoxelUp(ChunkGenerator chunkGenerator, Vector3i worldVoxelPosition, int distance, int minOutsideLength,
                                    out Vector3i hitPoint, out Vector3 hitNormal, out ushort voxelTypeIndex)
        {
            var maxY = worldVoxelPosition.y + distance;

            var wpos = worldVoxelPosition;
            var outsideLength = 0;
            var lastVoxelInsideWorldPosition = worldVoxelPosition;

            for (var y = worldVoxelPosition.y; y <= maxY; y++) {
                wpos.y = y;
                // Prepare the generator
                // Finally, compute the voxel (or get it from the cache if it exists)
                var voxel = chunkGenerator.VoxelGenerator.GenerateWithoutChunk(wpos);
                if (voxel.IsInside) {

                    outsideLength = 0;
                    lastVoxelInsideWorldPosition = wpos;
                    if (lastVoxelInsideWorldPosition.y + minOutsideLength > maxY) {
                        hitPoint = Vector3i.zero;
                        hitNormal = Vector3.zero;
                        voxelTypeIndex = 0;
                        return false;
                    }
                } else {
                    outsideLength++;
                    if (outsideLength >= minOutsideLength) {
                        hitPoint = lastVoxelInsideWorldPosition;
                        voxel = chunkGenerator.VoxelGenerator.GenerateWithoutChunk(lastVoxelInsideWorldPosition);
                        hitNormal = (Vector3) chunkGenerator.VoxelGenerator.ComputeGradient(lastVoxelInsideWorldPosition);
                        voxelTypeIndex = voxel.VoxelTypeIndex;
                        return true;
                    }
                }
            }

            hitPoint = Vector3i.zero;
            hitNormal = Vector3.zero;
            voxelTypeIndex = 0;
            return false;
        }
    }
}