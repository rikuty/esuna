using System.Collections.Generic;
#if NET_4_6
using System.Collections.Concurrent;
#else
using UltimateTerrains.Concurrent.NET3x;

#endif

namespace UltimateTerrains
{
    internal sealed class ConcurrentListPool<T>
    {
        private readonly ConcurrentStack<List<T>> items;

        public ConcurrentListPool(int initialSize, int initialListCapacity)
        {
            items = new ConcurrentStack<List<T>>();

            for (var i = 0; i < initialSize; ++i) {
                var item = new List<T>(initialListCapacity);
                items.Push(item);
            }
        }

        public List<T> Get(int initialListCapacity)
        {
            List<T> item;
            if (!items.TryPop(out item)) {
                return new List<T>(initialListCapacity);
            }

            if (item.Capacity < initialListCapacity) {
                item.Capacity = initialListCapacity;
            }

            return item;
        }

        public void Free(List<T> item)
        {
            if (item != null) {
                item.Clear();
                items.Push(item);
            }
        }
    }
}