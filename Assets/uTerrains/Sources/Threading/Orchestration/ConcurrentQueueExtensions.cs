#if NET_4_6
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;

namespace UltimateTerrains
{
    public static class ConcurrentQueueExtensions
    {
        public static T DequeueOrFail<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            if (!queue.TryDequeue(out item)) {
                UDebug.Fatal("Could not dequeue item from ConcurrentQueue. Is another thread trying to dequeue at the same time?");
            }
            return item;
        }
    }
}

#endif