using System.Collections;
using System.Collections.Generic;
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
    public sealed class AsyncOperationOrchestrator : MonoBehaviour
    {
        private Coroutine orchestrateCoroutine;
        public bool IsComputing { get; private set; }

        private long currentIteration;
        private int chunksToBuildForOperationCount;

        private void IncrementChunksToBuildForOperationCount()
        {
            Interlocked.Increment(ref chunksToBuildForOperationCount);
        }

        public void DecrementChunksToBuildForOperationCount()
        {
            Interlocked.Decrement(ref chunksToBuildForOperationCount);
        }

        private Orchestrator orchestrator;
        private UltimateOperationsManager operationsManager;
        private readonly Queue<Chunk> chunksToBuild = new Queue<Chunk>();
        private readonly ConcurrentQueue<Chunk> chunksToPostBuildOrFree = new ConcurrentQueue<Chunk>();
        private readonly Queue<OperationHandler> operationsToCallback = new Queue<OperationHandler>();

        internal void EnqueueChunkToBuild(Chunk chunk)
        {
            IncrementChunksToBuildForOperationCount();
            chunksToBuild.Enqueue(chunk);
        }

        internal void EnqueueChunkToPostBuildOrFree(Chunk chunk)
        {
            chunksToPostBuildOrFree.Enqueue(chunk);
        }

        internal void EnqueueOperationToCallback(OperationHandler operation)
        {
            operationsToCallback.Enqueue(operation);
        }

        internal void EnqueuePersistNewOperations()
        {
            orchestrator.EnqueuePersistNewOperations(operationsManager);
        }

        internal void Init(UltimateTerrain terrain, Orchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
            operationsManager = terrain.OperationsManager;
        }

        public void StartOrchestration()
        {
            IsComputing = true;
            currentIteration = orchestrator.Iteration;
            orchestrateCoroutine = StartCoroutine(Orchestrate());
        }

        public void Stop()
        {
            if (orchestrateCoroutine != null) {
                StopCoroutine(orchestrateCoroutine);
            }

            StopAllCoroutines();
            orchestrator = null;
            operationsManager = null;
        }

        private IEnumerator Orchestrate()
        {
            BuildChunks();

            yield return StartCoroutine(WaitAllChunkBuilds());

            yield return StartCoroutine(Commit());
        }

        private void BuildChunks()
        {
            while (chunksToBuild.Count > 0) {
                var chunk = chunksToBuild.Dequeue();
                orchestrator.EnqueueBuildChunkForOperationTask(chunk, false);
            }
        }

        private IEnumerator WaitAllChunkBuilds()
        {
            while (chunksToBuildForOperationCount > 0) {
                yield return null;
            }
        }

        private IEnumerator Commit()
        {
            Profiler.BeginSample("Commit AsyncOperationOrchestrator 1/2");

            if (chunksToBuildForOperationCount != 0) {
                UDebug.Fatal("chunksToBuildForOperationCount != 0 in commit. Was equal to " + chunksToBuildForOperationCount);
            }

            if (currentIteration == orchestrator.Iteration) {
                while (!chunksToPostBuildOrFree.IsEmpty) {
                    var chunk = chunksToPostBuildOrFree.DequeueOrFail();
                    if (chunk.PostBuildOrFreeImmediately()) {
                        orchestrator.EnqueueAfterPostBuildChunkForOperationTask(chunk);
                    }
                }
            } else {
                ClearChunksToPostBuildQueue();
            }

            Profiler.EndSample();

            IsComputing = false;
            if (orchestrator.IsCommitPending) {
                yield return StartCoroutine(orchestrator.DoPendingCommit());
            }

            Profiler.BeginSample("Commit AsyncOperationOrchestrator 2/2");
            while (operationsToCallback.Count > 0) {
                operationsToCallback.Dequeue().DoCallback(operationsManager.AffectedCollidersBuffer);
            }

            Profiler.EndSample();
        }

        private void ClearChunksToPostBuildQueue()
        {
            while (!chunksToPostBuildOrFree.IsEmpty) {
                chunksToPostBuildOrFree.DequeueOrFail();
            }
        }
    }
}