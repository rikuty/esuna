using System.Collections.Generic;

namespace UltimateTerrains
{
    internal sealed class UnsafeListPool<T>
    {
        private readonly Stack<List<T>> items;

        public UnsafeListPool(int initialSize, int initialListCapacity)
        {
            items = new Stack<List<T>>();

            for (var i = 0; i < initialSize; ++i) {
                var item = new List<T>(initialListCapacity);
                items.Push(item);
            }
        }

        public List<T> Get(int initialListCapacity)
        {
            List<T> item;
            if (items.Count > 0) {
                item = items.Pop();
            } else {
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