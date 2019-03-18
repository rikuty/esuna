using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Profiling;
#if NET_4_6
using System.Collections.Concurrent;
using UltimateTerrains.Concurrent.NET4x;
#else
using UltimateTerrains.Concurrent.NET3x;

#endif

namespace UltimateTerrains
{
    internal sealed class ZeroAllocThreadPool
    {
        private struct BackgroundWorker
        {
            public ThreadWorker Worker;
            public Thread Thread;
        }

        private readonly PriorizedBlockingQueue<ITask> taskQueue;
        private readonly List<BackgroundWorker> backgroundWorkers;

        private readonly ConcurrentPool<UpdateChunkTreeTask> updateChunkTreeTaskPool = new ConcurrentPool<UpdateChunkTreeTask>(() => new UpdateChunkTreeTask());
        private readonly ConcurrentPool<BuildChunkTask> buildChunkTaskPool = new ConcurrentPool<BuildChunkTask>(() => new BuildChunkTask());
        private readonly ConcurrentPool<AfterPostBuildChunkTask> afterPostBuildChunkTaskPool = new ConcurrentPool<AfterPostBuildChunkTask>(() => new AfterPostBuildChunkTask());
        private readonly ConcurrentPool<SpawnTreesTask> spawnTreesTaskPool = new ConcurrentPool<SpawnTreesTask>(() => new SpawnTreesTask());
        private readonly ConcurrentPool<BuildChunkForOperationTask> buildChunkForOperationTaskPool = new ConcurrentPool<BuildChunkForOperationTask>(() => new BuildChunkForOperationTask());
        private readonly ConcurrentPool<AfterPostBuildChunkForOperationTask> afterPostBuildChunkForOperationTaskPool = new ConcurrentPool<AfterPostBuildChunkForOperationTask>(() => new AfterPostBuildChunkForOperationTask());
        private readonly ConcurrentPool<PersistOperationsTask> persistOperationsTaskTaskPool = new ConcurrentPool<PersistOperationsTask>(() => new PersistOperationsTask());


        internal ZeroAllocThreadPool(UltimateTerrain terrain)
        {
            taskQueue = new PriorizedBlockingQueue<ITask>();

            var threadCount = UltimateTerrain.ThreadCount;
            UDebug.Log(string.Format("The internal uTerrains thread pool will start {0} background threads", threadCount));

            backgroundWorkers = new List<BackgroundWorker>(threadCount);
            for (var i = 0; i < threadCount; ++i) {
                var bw = new BackgroundWorker {Worker = new ThreadWorker(this, taskQueue, terrain)};
                bw.Thread = new Thread(bw.Worker.DoWorkMainLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal,
                    Name = "uTerrainsThread"
                };
                backgroundWorkers.Add(bw);
            }
        }

        internal void StartThreads()
        {
            foreach (var bw in backgroundWorkers) {
                bw.Thread.Start();
            }
        }

        internal void StopThreads()
        {
            foreach (var bw in backgroundWorkers) {
                bw.Worker.Stop();
            }

            taskQueue.Close();
            foreach (var bw in backgroundWorkers) {
                if (bw.Thread.IsAlive) {
                    bw.Thread.Join();
                }
            }

            UDebug.Log("All threads stopped.");
        }

        private void EnqueueTask(ITask task)
        {
            taskQueue.Add(task);
        }

        private void EnqueueHighPriorityTask(ITask task)
        {
            taskQueue.AddHighPriority(task);
        }

        internal void TaskDone(ITask task)
        {
            if (task is UpdateChunkTreeTask) {
                updateChunkTreeTaskPool.Free((UpdateChunkTreeTask) task);
            } else if (task is BuildChunkTask) {
                buildChunkTaskPool.Free((BuildChunkTask) task);
            } else if (task is AfterPostBuildChunkTask) {
                afterPostBuildChunkTaskPool.Free((AfterPostBuildChunkTask) task);
            } else if (task is SpawnTreesTask) {
                spawnTreesTaskPool.Free((SpawnTreesTask) task);
            } else if (task is BuildChunkForOperationTask) {
                buildChunkForOperationTaskPool.Free((BuildChunkForOperationTask) task);
            } else if (task is AfterPostBuildChunkForOperationTask) {
                afterPostBuildChunkForOperationTaskPool.Free((AfterPostBuildChunkForOperationTask) task);
            } else if (task is PersistOperationsTask) {
                persistOperationsTaskTaskPool.Free((PersistOperationsTask) task);
            } else {
                // ReSharper disable once HeapView.ObjectAllocation.Evident
                throw new NotSupportedException("Unmanaged Task type " + task.GetType());
            }
        }

        internal void EnqueueUpdateChunkTreeTask(Chunk chunkRoot, bool isTerrainInfinite, Vector3i cameraChunkPosition, Orchestrator orchestrator, int? forcedLevel)
        {
            Profiler.BeginSample("EnqueueUpdateChunkTreeTask");
            var task = updateChunkTreeTaskPool.Get();
            task.Init(chunkRoot, isTerrainInfinite, cameraChunkPosition, orchestrator, forcedLevel);
            EnqueueTask(task);
            Profiler.EndSample();
        }

        internal void EnqueueBuildChunkTask(Chunk chunk, Orchestrator orchestrator, bool clearCache)
        {
            var task = buildChunkTaskPool.Get();
            task.Init(chunk, orchestrator, clearCache);
            EnqueueTask(task);
        }

        internal void EnqueueAfterPostBuildChunkTask(Chunk chunk, Orchestrator orchestrator)
        {
            var task = afterPostBuildChunkTaskPool.Get();
            task.Init(chunk, orchestrator);
            EnqueueHighPriorityTask(task);
        }

        internal void EnqueueSpawnTreesTask(TreesChunk chunk, Vector3i cameraChunkPosition, Orchestrator orchestrator)
        {
            var task = spawnTreesTaskPool.Get();
            task.Init(chunk, cameraChunkPosition, orchestrator);
            EnqueueTask(task);
        }

        internal void EnqueueBuildChunkForOperationTask(Chunk chunk, AsyncOperationOrchestrator orchestrator, bool clearCache)
        {
            var task = buildChunkForOperationTaskPool.Get();
            task.Init(chunk, orchestrator, clearCache);
            EnqueueHighPriorityTask(task);
        }

        internal void EnqueueAfterPostBuildChunkForOperationTask(Chunk chunk)
        {
            var task = afterPostBuildChunkForOperationTaskPool.Get();
            task.Init(chunk);
            EnqueueHighPriorityTask(task);
        }

        internal void EnqueuePersistOperationsTask(UltimateOperationsManager operationManager)
        {
            var task = persistOperationsTaskTaskPool.Get();
            task.Init(operationManager);
            EnqueueTask(task);
        }

        internal void DoWorkOnMainThreadForDebug()
        {
            foreach (var bw in backgroundWorkers) {
                if (!taskQueue.IsEmpty) {
                    bw.Worker.DoWork();
                }
            }
        }
    }
}