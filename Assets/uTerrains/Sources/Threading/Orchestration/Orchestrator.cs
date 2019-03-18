using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;
#if NET_4_6
using System.Collections.Concurrent;
#else
using UltimateTerrains.Concurrent.NET3x;

#endif

namespace UltimateTerrains
{
    public sealed class Orchestrator : MonoBehaviour
    {
        public long Iteration { get; private set; }
        private bool firstLoad;
        private Coroutine orchestrateCoroutine;
        private IEnumerator orchestrateCoroutineForEditor;
        private ZeroAllocThreadPool threadPool;
        private AsyncOperationOrchestrator asyncOperationOrchestrator;
        private readonly Stopwatch updateWatch = new Stopwatch();
        private int maxUpdateTimePerFrame;
        private int postBuildMaxCountPerFrame;

        private UltimateTerrain terrain;
        private Chunk[] chunkRoots;
        private TreesChunk[] treesChunks;
        private Vector3i oldCameraChunkPosition;
        private bool isTerrainInfinite;
        private int? forcedLevel;
        private bool preventTreesSpawning;
        private DateTime startTime;
        private bool stopping;
        private bool hotReloading;

        private ConcurrentQueue<Chunk> chunksToBuild = new ConcurrentQueue<Chunk>();
        private ConcurrentQueue<ChunkComponent> chunksToFree = new ConcurrentQueue<ChunkComponent>();
        private ConcurrentQueue<Chunk> chunksToPostBuild = new ConcurrentQueue<Chunk>();
        private Queue<ChunkComponent> chunksToEnableRenderer = new Queue<ChunkComponent>();
        private ConcurrentQueue<TreeEntry> treesToSpawn = new ConcurrentQueue<TreeEntry>();
        private ConcurrentQueue<TreeComponent> treesToFree = new ConcurrentQueue<TreeComponent>();

        public bool IsFirstLoadDone {
            get { return !firstLoad; }
        }

        internal bool IsCommitPending { get; private set; }


        private int chunkRootsToUpdateCount;

        public void DecrementChunkRootsToUpdateCount()
        {
            Interlocked.Decrement(ref chunkRootsToUpdateCount);
        }

        private int treesChunkToUpdateCount;

        public void DecrementTreesChunkToUpdateCount()
        {
            Interlocked.Decrement(ref treesChunkToUpdateCount);
        }

        private int chunksToBuildEntirelyCount;

        private void IncrementChunksToBuildEntirelyCount()
        {
            Interlocked.Increment(ref chunksToBuildEntirelyCount);
        }

        public void DecrementChunksToBuildEntirelyCount()
        {
            Interlocked.Decrement(ref chunksToBuildEntirelyCount);
        }

        public void Init(UltimateTerrain terrain, int? forcedLevel)
        {
            startTime = DateTime.Now;
            this.terrain = terrain;
            threadPool = new ZeroAllocThreadPool(terrain);
            asyncOperationOrchestrator = terrain.AsyncOperationOrchestrator;
            chunkRoots = terrain.Chunks;
            treesChunks = terrain.TreesChunks;
            isTerrainInfinite = terrain.Params.IsTerrainInfinite;
            this.forcedLevel = forcedLevel;
            preventTreesSpawning = terrain.Params.PreventTreesSpawning;
            maxUpdateTimePerFrame = terrain.Params.PostBuildMaxElapsedTimePerFrame;
            postBuildMaxCountPerFrame = terrain.Params.PostBuildMaxCountPerFrame;
            stopping = false;
            chunksToBuildEntirelyCount = 0;
            treesChunkToUpdateCount = 0;
            IsCommitPending = false;
            Iteration = 0;
        }

        public void StartOrchestration(bool fromEditor)
        {
            stopping = false;
            Iteration = 0;
#if !NO_MULTITHREAD
            threadPool.StartThreads();
#endif
            if (fromEditor) {
                orchestrateCoroutineForEditor = Orchestrate(true);
            } else {
                orchestrateCoroutine = StartCoroutine(Orchestrate(false));
            }
        }

        public void Stop()
        {
            stopping = true;
            if (orchestrateCoroutine != null) {
                StopCoroutine(orchestrateCoroutine);
                orchestrateCoroutine = null;
            }

            StopAllCoroutines();
            if (updateWatch != null) {
                updateWatch.Stop();
            }

            if (threadPool != null) {
                threadPool.StopThreads();
                threadPool = null;
            }

            terrain = null;
            asyncOperationOrchestrator = null;
            chunkRoots = null;
            treesChunks = null;
            chunksToBuild = new ConcurrentQueue<Chunk>();
            chunksToFree = new ConcurrentQueue<ChunkComponent>();
            chunksToPostBuild = new ConcurrentQueue<Chunk>();
            chunksToEnableRenderer = new Queue<ChunkComponent>();
            treesToSpawn = new ConcurrentQueue<TreeEntry>();
            treesToFree = new ConcurrentQueue<TreeComponent>();
        }

        internal void EnqueueChunkToBuild(Chunk chunk)
        {
            if (Interlocked.CompareExchange(ref chunk.IsEnqueuedForBuild, 1, 0) == 0) {
                IncrementChunksToBuildEntirelyCount();
                chunksToBuild.Enqueue(chunk);
            }
        }

        internal void EnqueueChunkToPostBuild(Chunk chunk)
        {
            if (Interlocked.CompareExchange(ref chunk.IsEnqueuedForPostBuild, 1, 0) == 0) {
                IncrementChunksToBuildEntirelyCount();
                chunksToPostBuild.Enqueue(chunk);
            }
        }

        internal void EnqueueChunkToFree(ChunkComponent chunkObject)
        {
            chunksToFree.Enqueue(chunkObject);
        }

        internal void EnqueueTreeToSpawn(TreeEntry treeEntry)
        {
            treesToSpawn.Enqueue(treeEntry);
        }

        internal void EnqueueTreeToFree(TreeComponent treeObject)
        {
            treesToFree.Enqueue(treeObject);
        }

        private bool HasRemainingTimeForPostBuildOnMainThread {
            get { return updateWatch.ElapsedMilliseconds < Math.Max(1, maxUpdateTimePerFrame - 2) || firstLoad; }
        }

        private bool HasRemainingTimeOnMainThread {
            get { return updateWatch.ElapsedMilliseconds < maxUpdateTimePerFrame || firstLoad; }
        }

        public bool HotReloading {
            get { return hotReloading; }
        }

        internal void EnqueuePersistNewOperations(UltimateOperationsManager operationManager)
        {
            threadPool.EnqueuePersistOperationsTask(operationManager);
        }

        // should be used by editor scripts only
        public void MoveNextFromEditor()
        {
            Update();
            orchestrateCoroutineForEditor.MoveNext();
        }

        private void Update()
        {
#if NO_MULTITHREAD
            threadPool.DoWorkOnMainThreadForDebug();
#endif
            Profiler.BeginSample("Update (orchestrator)");
            try {
                updateWatch.Stop();
                updateWatch.Reset();
                updateWatch.Start();

                Profiler.BeginSample("DoPostBuilds");
                var postBuildCount = 0;
                while (!chunksToPostBuild.IsEmpty && HasRemainingTimeForPostBuildOnMainThread && postBuildCount < postBuildMaxCountPerFrame) {
                    var chunk = chunksToPostBuild.DequeueOrFail();
                    Interlocked.Exchange(ref chunk.IsEnqueuedForPostBuild, 0);
                    if (DoPostBuild(chunk)) {
                        postBuildCount++;
                    }
                }

                Profiler.EndSample();


                Profiler.BeginSample("DoSpawnTrees");
                while (!treesToFree.IsEmpty) {
                    var treeToFree = treesToFree.DequeueOrFail();
                    TreesObjectPools.Free(treeToFree);
                }

                while (!treesToSpawn.IsEmpty && HasRemainingTimeOnMainThread) {
                    var treeEntry = treesToSpawn.DequeueOrFail();
                    DoSpawnTree(treeEntry);
                }

                Profiler.EndSample();
            }
            finally {
                updateWatch.Stop();
                updateWatch.Reset();
            }

            Profiler.EndSample();
        }

        public void HotReload()
        {
            hotReloading = true;
            oldCameraChunkPosition = terrain.CurrentCameraChunkPosition + Vector3i.back;
        }

        private IEnumerator Orchestrate(bool fromEditor)
        {
            firstLoad = true;

            while (!stopping) {
                while (!firstLoad && oldCameraChunkPosition == terrain.CurrentCameraChunkPosition) {
                    terrain.UpdateCurrentCameraChunkPosition();
                    yield return null;
                }

                var cameraChunkPosition = terrain.CurrentCameraChunkPosition;
                oldCameraChunkPosition = cameraChunkPosition;

                if (fromEditor) {
                    var e = UpdateChunkTrees(cameraChunkPosition);
                    while (e.MoveNext()) {
                        yield return null;
                    }
                } else {
                    yield return StartCoroutine(UpdateChunkTrees(cameraChunkPosition));
                }

                BuildChunks();

                if (!preventTreesSpawning) {
                    UpdateTerrainTrees(cameraChunkPosition);
                }

                // Wait for all chunk to be built
                while (chunksToBuildEntirelyCount > 0) {
                    yield return null;
                }

                if (!asyncOperationOrchestrator.IsComputing) {
                    if (fromEditor) {
                        var e = Commit(fromEditor);
                        while (e.MoveNext()) {
                            yield return null;
                        }
                    } else {
                        yield return StartCoroutine(Commit(fromEditor));
                    }
                } else {
                    IsCommitPending = true;
                    while (IsCommitPending) {
                        yield return null;
                    }
                }

                hotReloading = false;
                if (firstLoad) {
                    firstLoad = false;
                    terrain.TriggerOnLoaded();
                    UDebug.Log("Done. Total loading time: " + (DateTime.Now - startTime) + "s");
                    GC.Collect();

                    if (!terrain.Params.IsTerrainInfinite && terrain.Params.MaxLevel == 1) {
                        UDebug.Log("Ending normal orchestration as your terrain is finite and LOD count is equal to 1. " +
                                   "This will save computation time.");
                        break;
                    }
                }
            }
        }

        internal IEnumerator DoPendingCommit()
        {
            if (IsCommitPending) {
                yield return StartCoroutine(Commit(false));
                IsCommitPending = false;
            }
        }

        private IEnumerator UpdateChunkTrees(Vector3i cameraChunkPosition)
        {
            chunkRootsToUpdateCount = chunkRoots.Length;

            updateWatch.Stop();
            updateWatch.Reset();
            updateWatch.Start();
            for (var i = 0; i < chunkRoots.Length; ++i) {
                threadPool.EnqueueUpdateChunkTreeTask(chunkRoots[i], isTerrainInfinite, cameraChunkPosition, this, forcedLevel);
                if (!HasRemainingTimeOnMainThread) {
                    updateWatch.Stop();
                    updateWatch.Reset();
                    yield return null;
                    updateWatch.Start();
                }
            }

            while (chunkRootsToUpdateCount > 0) {
                yield return null;
            }
        }

        public void RefreshBuildChunks()
        {
            BuildChunks();
        }

        private void BuildChunks()
        {
            while (!chunksToBuild.IsEmpty) {
                var chunk = chunksToBuild.DequeueOrFail();
                Interlocked.Exchange(ref chunk.IsEnqueuedForBuild, 0);
                threadPool.EnqueueBuildChunkTask(chunk, this, false);
            }
        }

        private void UpdateTerrainTrees(Vector3i cameraChunkPosition)
        {
            // Before refreshing trees, we wait for all tasks to be done and all tree objects to be spawned
            // This is extremely important because if we don't wait, we might try to spawn entries that have been
            // free in the mean time.
            if (treesChunkToUpdateCount == 0 && treesToSpawn.IsEmpty) {
                treesChunkToUpdateCount = treesChunks.Length;

                for (var i = 0; i < treesChunks.Length; ++i) {
                    threadPool.EnqueueSpawnTreesTask(treesChunks[i], cameraChunkPosition, this);
                }
            } else if (treesChunkToUpdateCount < 0) {
                UDebug.LogError("treesChunkToUpdateCount should never be negative");
            }
        }

        private bool DoPostBuild(Chunk chunk)
        {
            Profiler.BeginSample("DoPostBuild");
            var chunkObject = chunk.PostBuild();
            if (chunkObject != null) {
                chunksToEnableRenderer.Enqueue(chunkObject);
            } else {
                UDebug.LogWarning("Chunk object should never be null after PostBuild. Was PostBuild called on empty mesh data?");
            }

            threadPool.EnqueueAfterPostBuildChunkTask(chunk, this);
            Profiler.EndSample();
            return chunkObject != null;
        }

        private void DoSpawnTree(TreeEntry tree)
        {
            Profiler.BeginSample("DoSpawnTree");
            if (tree.TreeObject == null) {
                var obj = TreesObjectPools.Get(tree.IndexOfObject);
                tree.TreeObject = obj;
                obj.transform.parent = terrain.GetTreesParent();
                obj.transform.position = tree.Position.ToUnityOrigin();
                obj.transform.eulerAngles = tree.Rotation;
                obj.transform.localScale = tree.Scale;
                obj.Enable();
            }

            Profiler.EndSample();
        }

        private IEnumerator Commit(bool fromEditor)
        {
            if (chunksToBuildEntirelyCount != 0) {
                UDebug.Fatal("chunksToBuildEntirelyCount != 0 in commit. Was equal to " + chunksToBuildEntirelyCount);
            }

            if (fromEditor) {
                var e = UpdateTreesVisibility();
                while (e.MoveNext()) {
                    yield return null;
                }
            } else {
                yield return StartCoroutine(UpdateTreesVisibility());
            }

            Profiler.BeginSample("FreeChunks");
            while (!chunksToFree.IsEmpty) {
                ChunkObjectPool.Free(chunksToFree.DequeueOrFail());
            }

            Profiler.EndSample();

            Profiler.BeginSample("EnableRendererOfChunks");
            while (chunksToEnableRenderer.Count > 0) {
                var chunkObject = chunksToEnableRenderer.Dequeue();
                if (chunkObject.IsBelongingToItsChunk) {
                    chunkObject.Enable();
                }
            }

            Profiler.EndSample();

            Iteration++;
            terrain.TriggerOnCommitted();
        }

        private IEnumerator UpdateTreesVisibility()
        {
            updateWatch.Stop();
            updateWatch.Reset();
            updateWatch.Start();
            for (var i = 0; i < chunkRoots.Length; ++i) {
                Profiler.BeginSample("UpdateTreesVisibility");
                chunkRoots[i].UpdateTreeVisibility();
                Profiler.EndSample();
                Chunk.DeepDebug.CheckTreeVisibility(chunkRoots[i]);
                if (!HasRemainingTimeOnMainThread) {
                    updateWatch.Stop();
                    updateWatch.Reset();
                    yield return null;
                    updateWatch.Start();
                }
            }
        }

        internal void EnqueueBuildChunkForOperationTask(Chunk chunk, bool clearCache)
        {
            threadPool.EnqueueBuildChunkForOperationTask(chunk, asyncOperationOrchestrator, clearCache);
        }

        internal void EnqueueAfterPostBuildChunkForOperationTask(Chunk chunk)
        {
            threadPool.EnqueueAfterPostBuildChunkForOperationTask(chunk);
        }
    }
}