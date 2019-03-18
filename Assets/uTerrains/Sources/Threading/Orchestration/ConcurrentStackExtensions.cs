#if NET_4_6
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;

namespace UltimateTerrains
{
    public static class ConcurrentStackExtensions
    {
        public static T PopOrFail<T>(this ConcurrentStack<T> stack)
        {
            T item;
            if (!stack.TryPop(out item)) {
                UDebug.Fatal("Could not pop item from ConcurrentStack. Is another thread trying to pop at the same time?");
            }
            return item;
        }
    }
}

#endif