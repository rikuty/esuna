using System;
using UnityEngine.Profiling;
#if NET_4_6
using UltimateTerrains.Concurrent.NET4x;
#else
using UltimateTerrains.Concurrent.NET3x;

#endif

namespace UltimateTerrains
{
    internal sealed class ThreadWorker
    {
        private bool run;
        private readonly ZeroAllocThreadPool threadPool;
        private readonly PriorizedBlockingQueue<ITask> taskQueue;
        private readonly ThreadSpecificParams threadParams;

#if UNITY_2018_1_OR_NEWER
        private readonly CustomSampler sampler;
#endif

        internal ThreadWorker(ZeroAllocThreadPool threadPool, PriorizedBlockingQueue<ITask> taskQueue, UltimateTerrain terrain)
        {
            run = true;
            this.threadPool = threadPool;
            this.taskQueue = taskQueue;

            Profiler.BeginSample("Create thread ChunkGenerator");
            threadParams.chunkGenerator = new ChunkGenerator(terrain);
            Profiler.EndSample();

            Profiler.BeginSample("Create thread ChunkTreePool");
            threadParams.chunkTreePool = new ChunkTreePool(terrain);
            Profiler.EndSample();

            Profiler.BeginSample("Create thread TreesSpawnerPool");
            threadParams.treesSpawnerPool = new TreesSpawnerPool();
            Profiler.EndSample();

#if UNITY_2018_1_OR_NEWER
            sampler = CustomSampler.Create("Execute Task");
#endif
        }

        internal void Stop()
        {
            run = false;
        }


        internal void DoWorkMainLoop()
        {
            try {
#if UNITY_2018_1_OR_NEWER
                Profiler.BeginThreadProfiling("uTerrains", "Worker Thread");
#endif
                while (run) {
                    DoWork();
                }
            }
#if NET_4_6
            catch (OperationCanceledException) {
                // do nothing, this one is ok
            }
#endif
            catch (Exception e) {
                UDebug.LogException(e);
                throw;
            }
            finally {
#if UNITY_2018_1_OR_NEWER
                Profiler.EndThreadProfiling();
#endif
            }
        }

        internal void DoWork()
        {
            var task = taskQueue.BlockingTake();
            if (task != null) {
#if UNITY_2018_1_OR_NEWER
                sampler.Begin();
#endif
                task.Execute(threadParams);
#if UNITY_2018_1_OR_NEWER
                sampler.End();
#endif
                threadPool.TaskDone(task);
                //this.threadParams.chunkGenerator.ChunkGeneratorPools.DebugLog();
            }
        }
    }
}