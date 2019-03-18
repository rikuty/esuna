//#define UT_SAFE_ARRAYPOOL
//#define UT_SAFE_ARRAYPOOL_RAISE_ERROR

using System.Collections.Generic;

namespace UltimateTerrains
{
    internal sealed class UnsafeStackPool<T>
    {
        private readonly Stack<Stack<T>> items;
        private readonly int length;

#if UT_SAFE_ARRAYPOOL
		readonly HashSet<Stack<T>> _set;
		#endif

        public int Count {
            get { return items.Count; }
        }

        public int TotalInstanciatedCount { get; private set; }

	    public UnsafeStackPool(int length, int initialCapacity, int initialSize)
        {
            TotalInstanciatedCount = 0;
            this.length = length;

            if (initialSize > 0) {
                // Init
                items = new Stack<Stack<T>>(initialSize < initialCapacity ? initialCapacity : initialSize);
#if UT_SAFE_ARRAYPOOL
				_set = new HashSet<Stack<T>> (initialCapacity);
				#endif

                for (var i = 0; i < initialSize; ++i) {
                    ++TotalInstanciatedCount;
#if UT_SAFE_ARRAYPOOL
					Stack<T> item = new Stack<T> (length);
					_items.Push (item);
					_set.Add (item);
					#else
                    items.Push(new Stack<T>(length));
#endif
                }
            } else {
                // Init
                items = new Stack<Stack<T>>(initialCapacity);
#if UT_SAFE_ARRAYPOOL
				_set = new HashSet<Stack<T>> (initialCapacity);
				#endif
            }
        }

        public Stack<T> Get()
        {
            if (items.Count == 0) {
                ++TotalInstanciatedCount;
                return new Stack<T>(length);
            }
#if UT_SAFE_ARRAYPOOL
			Stack<T> item = _items.Pop ();
			_set.Remove (item);
			return item;
			#else
            return items.Pop();
#endif
        }

        public void Free(Stack<T> item)
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