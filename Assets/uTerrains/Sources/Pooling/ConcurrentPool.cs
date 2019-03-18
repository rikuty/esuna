#if NET_4_6
using System.Collections.Concurrent;
#else
using UltimateTerrains.Concurrent.NET3x;

#endif

namespace UltimateTerrains
{
    internal sealed class ConcurrentPool<T> where T : class
    {
        public delegate T Constructor();

        private readonly Constructor constructor;
        private readonly ConcurrentStack<T> items;

        public ConcurrentPool(Constructor constructorDelegate, int initialSize = 0)
        {
            constructor = constructorDelegate;
            items = new ConcurrentStack<T>();

            for (var i = 0; i < initialSize; ++i) {
                var item = constructor();
                items.Push(item);
            }
        }

        public T Get()
        {
            T item;
            if (!items.TryPop(out item)) {
                return constructor();
            }

            return item;
        }

        public void Free(T item)
        {
            if (item != null) {
                items.Push(item);
            }
        }
    }
}