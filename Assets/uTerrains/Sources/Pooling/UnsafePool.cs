//#define UT_SAFE_POOL
//#define UT_SAFE_POOL_RAISE_ERROR

using System;
using System.Collections.Generic;

namespace UltimateTerrains
{
    internal sealed class UnsafePool<T> where T : class 
    {
        public delegate T Constructor();

        private readonly Constructor constructor;
        private readonly Stack<T> items;
        private int totalInstanciatedCount;

#if UT_SAFE_POOL
		readonly HashSet<T> _set;
#endif

        public int Count {
            get { return items.Count; }
        }

        public int TotalInstanciatedCount {
            get { return totalInstanciatedCount; }
        }

        public UnsafePool(Constructor constructorDelegate, int initialCapacity, int initialSize)
        {
            constructor = constructorDelegate;
            totalInstanciatedCount = 0;

            if (initialSize > 0) {
                // Init
                items = new Stack<T>(initialSize < initialCapacity ? initialCapacity : initialSize);
#if UT_SAFE_POOL
				_set = new HashSet<T> (initialCapacity);
#endif

                for (var i = 0; i < initialSize; ++i) {
                    ++totalInstanciatedCount;
                    var item = constructor();
                    items.Push(item);
#if UT_SAFE_POOL
					_set.Add (item);
#endif
                }
            } else {
                // Init
                items = new Stack<T>(initialCapacity);
#if UT_SAFE_POOL
				_set = new HashSet<T> (initialCapacity);
#endif
            }
        }

        public T Get()
        {
            if (items.Count == 0) {
                ++totalInstanciatedCount;
                var item = constructor();
                //GCHandle.Alloc(item);
                return item;
            }
#if UT_SAFE_POOL
			T item = items.Pop ();
			_set.Remove (item);
			return item;
#else
            return items.Pop();
#endif
        }

        public void Free(T item)
        {
            if (item != null) {
#if UT_SAFE_POOL
				if (!_set.Contains (item)) {
					items.Push (item);
					_set.Add (item);
				}
#if UT_SAFE_POOL_RAISE_ERROR
				else {
					UDebug.LogError ("item (" + typeof(T) + ") is already free!");
				}
#endif
#else
                items.Push(item);
#endif
            }
        }

        public void BulkFree(IList<T> itemsToFree, int length)
        {
            for (var i = 0; i < length; ++i) {
                var item = itemsToFree[i];
                if (item != null) {
#if UT_SAFE_POOL
					if (!_set.Contains (item)) {
						items.Push (item);
						_set.Add (item);
					}
#if UT_SAFE_POOL_RAISE_ERROR
					else {
						UDebug.LogError ("item (" + typeof(T) + ") is already free!");
					}
#endif
#else
                    items.Push(item);
#endif
                }
            }
        }
    }
}