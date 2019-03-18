using System;
using System.Diagnostics;
using UnityEngine.Profiling;

namespace UltimateTerrains
{
    public static class UProfiler
    {
        private static long totalMemoryAllocated;
        private static string memoryName;

        [Conditional("UPROFILE_MEMORY")]
        public static void BeginMemory(string name)
        {
            memoryName = name;
            GC.Collect();
            totalMemoryAllocated = GC.GetTotalMemory(false);
        }
        
        [Conditional("UPROFILE_MEMORY")]
        public static void EndMemory()
        {
            GC.Collect();
            UnityEngine.Debug.Log(string.Format("Memory allocated ({0}) = {1}", 
                                                memoryName, 
                                                (GC.GetTotalMemory(false) - totalMemoryAllocated) / 1000000.0));
        }
        
        [Conditional("NO_MULTITHREAD")]
        public static void Begin(string name)
        {
            Profiler.BeginSample(name);
        }

        [Conditional("NO_MULTITHREAD")]
        public static void End()
        {
            Profiler.EndSample();
        }
    }
}