//#define UT_SAFE_ARRAYPOOL
//#define UT_SAFE_ARRAYPOOL_RAISE_ERROR

using System.Collections.Generic;

namespace UltimateTerrains
{
    internal sealed class UnsafeArrayPool<T>
    {
        private readonly Stack<T[]> items;
        private readonly int length;

#if UT_SAFE_ARRAYPOOL
		readonly HashSet<T[]> _set;
#endif

        public int Count {
            get { return items.Count; }
        }

        public int TotalInstanciatedCount { get; private set; }

        public UnsafeArrayPool(int length, int initialCapacity, int initialSize)
        {
            TotalInstanciatedCount = 0;
            this.length = length;

            if (initialSize > 0) {
                // Init
                items = new Stack<T[]>(initialSize < initialCapacity ? initialCapacity : initialSize);
#if UT_SAFE_ARRAYPOOL
				_set = new HashSet<T[]> (initialCapacity);
#endif

                for (var i = 0; i < initialSize; ++i) {
                    ++TotalInstanciatedCount;
                    var item = new T[length];
                    items.Push(item);
#if UT_SAFE_ARRAYPOOL
					_set.Add (item);
#endif
                }
            } else {
                // Init
                items = new Stack<T[]>(initialCapacity);
#if UT_SAFE_ARRAYPOOL
				_set = new HashSet<T[]> (initialCapacity);
#endif
            }
        }

        public T[] Get()
        {
            if (items.Count == 0) {
                ++TotalInstanciatedCount;
                var item = new T[length];
                return item;
            }
#if UT_SAFE_ARRAYPOOL
			T[] item = _items.Pop ();
			_set.Remove (item);
			return item;
#else
            return items.Pop();
#endif
        }

        public void Free(T[] item)
        {
            if (item != null) {
#if UT_SAFE_ARRAYPOOL
				if (!_set.Contains (item)) {
					_items.Push (item);
					_set.Add (item);
				} else {
#if UT_SAFE_ARRAYPOOL_RAISE_ERROR
					UDebug.LogError ("item is already free!");
#endif
				}
#else
                items.Push(item);
#endif
            }
        }
    }
}