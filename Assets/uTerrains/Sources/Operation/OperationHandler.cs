using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    public sealed class OperationHandler
    {
        private readonly UltimateTerrain terrain;
        private readonly ChunkGenerator mainThreadChunkGenerator;
        private readonly ChunkGenerator.Result chunkBuildResult;
        private readonly ChunkState chunkToBuildState;
        private readonly object syncMerge = new object();
        public bool TryMerge { get; private set; }

        private IOperation operation;
        private Vector3i from;
        private Vector3i to;
        private List<Vector3i> affectedVirtualChunks;
        private List<Vector3i> affectedVirtualChunkTrees;
        private bool undoing;
        private int mergeId;

        public IOperation Operation {
            get { return operation; }
        }

        internal List<Vector3i> AffectedVirtualChunks {
            get { return affectedVirtualChunks; }
        }

        internal OperationHandler(IOperation op, UltimateTerrain uTerrain, bool tryMerge)
        {
            TryMerge = tryMerge;
            operation = op;
            terrain = uTerrain;
            mainThreadChunkGenerator = uTerrain.MainThreadChunkGenerator;
            chunkBuildResult = new ChunkGenerator.Result();
            chunkToBuildState = new ChunkState();
            affectedVirtualChunks = new List<Vector3i>();
            affectedVirtualChunkTrees = new List<Vector3i>();

            from = operation.GetAreaOfEffectMin();
            to = operation.GetAreaOfEffectMax();

            var cStart = UnitConverter.VoxelToChunkPosition(@from - Vector3i.one);
            var cEnd = UnitConverter.VoxelToChunkPosition(to + Vector3i.one);
            var lStart = UnitConverter.ToLocalPositionWithOffset(@from - Vector3i.one);
            for (var x = cStart.x; x <= cEnd.x; ++x) {
                for (var y = cStart.y; y <= cEnd.y; ++y) {
                    for (var z = cStart.z; z <= cEnd.z; ++z) {
                        AddAffectedChunk(new Vector3i(x, y, z), lStart);
                    }
                }
            }
        }

        // This methods apply the operation to affected chunks (both visible and those that are going to be visible).
        // It is VERY IMPORTANT to keep this order:
        //      1. Mark all possibly affected chunks as dirty (ie. needBuild = true)
        //      2. (Re)Build currently visible chunks (isBeingDisplayed == true)
        //      3. (Re)Build chunks that are going to be visible (isBeingDisplayed == false and willBeDisplayed == true)
        //
        // Why is it important? Because the chunk-tree-iteration could change these values (needBuild/willBeDisplayed)
        // while we are doing this (on another thread).
        // 
        // Case A: The chunk-tree-iteration set needBuild to false AFTER we set it to true (ie. after MarkDirty). In this case,
        // we are sure that willBeDisplayed has been set to true. So we will still affect the chunk in (3).
        //
        // Case B: The chunk-tree-iteration set needBuild to false BEFORE we set it to true (ie. before MarkDirty). In this case,
        // we are NOT sure that willBeDisplayed has been set to true. But we are sure that even if it will be the case, the chunk
        // will be affected because it will be (re)computed as needBuild is true.
        internal void Perform(bool performAsync, HashSet<Chunk> alreadyBuildChunksSet, HashSet<Chunk> alreadyRecomputedChunksSet, bool undo, Collider[] collidersBuffer)
        {
            undoing = undo;
            // Mark all affected chunks as dirty
            foreach (var chunkTreePosition in affectedVirtualChunkTrees) {
                var root = terrain.GetChunkRoot(chunkTreePosition);
                if (root != null) {
                    Chunk.DeepDebug.CheckTreeVisibility(root);
                    // TODO: is it still necessary?
                    root.MarkDirty();
                }
            }

            // Recompute chunk mesh
            foreach (var chunkPosition in affectedVirtualChunks) {
                var chunk = terrain.GetChunkVisible(chunkPosition);
                if (chunk != null && !alreadyBuildChunksSet.Contains(chunk)) {
                    alreadyBuildChunksSet.Add(chunk);
                    if (performAsync) {
                        terrain.AsyncOperationOrchestrator.EnqueueChunkToBuild(chunk);
                    } else {
                        BuildChunkImmediately(chunk, undo);
                    }
                }

                // Recompute already computed chunks
                chunk = terrain.GetChunkThatWillBeVisible(chunkPosition);
                if (chunk != null && !alreadyRecomputedChunksSet.Contains(chunk)) {
                    if (alreadyBuildChunksSet.Contains(chunk)) {
                        Debug.LogError("alreadyBuildChunksSet contains a chunk that will be visible " + chunk.Id);
                    }

                    terrain.Orchestrator.EnqueueChunkToBuild(chunk);
                    alreadyRecomputedChunksSet.Add(chunk);
                }
            }

            if (!performAsync) {
                DoCallback(collidersBuffer);
            } else {
                terrain.AsyncOperationOrchestrator.EnqueueOperationToCallback(this);
            }
        }

        internal void DoCallback(Collider[] collidersBuffer)
        {
            SendMessageToAffectedColliders(collidersBuffer);

            // Operation has been done
            if (!undoing) {
                operation.OnOperationDone();
            } else {
                operation.OnOperationUndone();
            }
        }

        private void BuildChunkImmediately(Chunk chunk, bool undo)
        {
            chunk.FillStateData(chunkToBuildState);
            var cache = undo ? null : chunk.GetCacheCopy(mainThreadChunkGenerator.ChunkGeneratorPools);
            mainThreadChunkGenerator.GenerateAndBuild(chunkToBuildState, chunkBuildResult, cache);
            mainThreadChunkGenerator.ApplyResult(chunkBuildResult, chunk);
            if (chunk.PostBuildOrFreeImmediately()) {
                chunk.OnPostBuildDone(mainThreadChunkGenerator.ChunkGeneratorPools);
            }

            chunkBuildResult.Reset();
            chunkToBuildState.Reset();
        }

        internal void Act(double x, double y, double z, ref Voxel voxel, int overridableVoxelTypePriority)
        {
            if (x >= from.x && x <= to.x &&
                y >= from.y && y <= to.y &&
                z >= from.z && z <= to.z) {

                if (operation.Act(x, y, z, ref voxel)) {
                    voxel.OverrideVoxelTypePriority = overridableVoxelTypePriority;
                }
            }
        }

        internal bool IsAlreadyMergedWith(OperationHandler other)
        {
            return other.operation == operation; // if operations are equal, they are already merged
        }

        internal bool Merge(int currentMergeId, OperationHandler newOperation)
        {
            lock (syncMerge) {
                if (mergeId >= currentMergeId) return false; // we already tried to merge these operations

                mergeId = currentMergeId;
                if (from >= newOperation.from && to <= newOperation.to && newOperation.operation.CanBeMergedWith(operation)) {
                    newOperation.operation = newOperation.operation.Merge(operation);
                    this.CopyValuesFrom(newOperation);
                    return true;
                }

                return false;
            }
        }

        private void CopyValuesFrom(OperationHandler other)
        {
            operation = other.operation;
            from = other.from;
            to = other.to;
            affectedVirtualChunks = other.affectedVirtualChunks;
            affectedVirtualChunkTrees = other.affectedVirtualChunkTrees;
        }

        internal bool HasEffectOnArea(Vector3d otherFrom, Vector3d otherTo)
        {
            return !(otherFrom.x > to.x || otherTo.x < from.x ||
                     otherFrom.y > to.y || otherTo.y < from.y ||
                     otherFrom.z > to.z || otherTo.z < from.z);
        }

        internal bool IsCompletelyInsideAreaOfEffect(Vector3d otherFrom, Vector3d otherTo)
        {
            return otherFrom.x > from.x && otherTo.x < to.x &&
                   otherFrom.y > from.y && otherTo.y < to.y &&
                   otherFrom.z > from.z && otherTo.z < to.z;
        }

        private void AddAffectedChunk(Vector3i chunkPosition, Vector3i locPos)
        {
            var voxelStartPos = UnitConverter.ChunkToVoxelPosition(chunkPosition);
            if (!operation.WillAffectChunk(voxelStartPos, voxelStartPos + Param.ChunkVoxelSize))
                return;
                
            if (!affectedVirtualChunks.Contains(chunkPosition))
                affectedVirtualChunks.Add(chunkPosition);

            var chunkTreePosition = terrain.Converter.ChunkToChunkTreePosition(chunkPosition);
            if (!affectedVirtualChunkTrees.Contains(chunkTreePosition))
                affectedVirtualChunkTrees.Add(chunkTreePosition);

            if (locPos.x == 0) {
                AddAffectedNeighbour(chunkPosition + new Vector3i(-1, 0, 0));
                if (locPos.y == 0) {
                    AddAffectedNeighbour(chunkPosition + new Vector3i(0, -1, 0));
                    AddAffectedNeighbour(chunkPosition + new Vector3i(-1, -1, 0));
                    if (locPos.z == 0) {
                        AddAffectedNeighbour(chunkPosition + new Vector3i(0, 0, -1));
                        AddAffectedNeighbour(chunkPosition + new Vector3i(-1, 0, -1));
                        AddAffectedNeighbour(chunkPosition + new Vector3i(0, -1, -1));
                        AddAffectedNeighbour(chunkPosition + new Vector3i(-1, -1, -1));
                    }
                } else {
                    if (locPos.z == 0) {
                        AddAffectedNeighbour(chunkPosition + new Vector3i(0, 0, -1));
                        AddAffectedNeighbour(chunkPosition + new Vector3i(-1, 0, -1));
                    }
                }
            } else {
                if (locPos.y == 0) {
                    AddAffectedNeighbour(chunkPosition + new Vector3i(0, -1, 0));
                    if (locPos.z == 0) {
                        AddAffectedNeighbour(chunkPosition + new Vector3i(0, 0, -1));
                        AddAffectedNeighbour(chunkPosition + new Vector3i(0, -1, -1));
                    }
                } else {
                    if (locPos.z == 0) {
                        AddAffectedNeighbour(chunkPosition + new Vector3i(0, 0, -1));
                    }
                }
            }
        }

        private void AddAffectedNeighbour(Vector3i neighbourPosition)
        {
            if (!affectedVirtualChunks.Contains(neighbourPosition))
                affectedVirtualChunks.Add(neighbourPosition);

            var chunkTreePosition = terrain.Converter.ChunkToChunkTreePosition(neighbourPosition);
            if (!affectedVirtualChunkTrees.Contains(chunkTreePosition))
                affectedVirtualChunkTrees.Add(chunkTreePosition);
        }


        private void SendMessageToAffectedColliders(Collider[] collidersBuffer)
        {
            var hitCount = operation.FindAffectedColliders(collidersBuffer);
            for (var i = 0; i < hitCount; i++) {
                collidersBuffer[i].gameObject.SendMessageUpwards("OnAffectedByTerrainOperation", operation, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}