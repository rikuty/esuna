//#define UT_SAFE_MMMESHPOOL
//#define UT_SAFE_MMMESHPOOL_RAISE_ERROR

using System.Collections.Generic;

namespace UltimateTerrains
{
    internal sealed class MeshDataPool
    {
        private readonly Stack<MeshData> items;
        private int totalInstanciatedCount;
        private readonly int materialsCount;
        private readonly int initialListsSize;

#if UT_SAFE_MMMESHPOOL
		readonly HashSet<MeshData> _set;
#endif

        public int Count {
            get { return items.Count; }
        }

        public int TotalInstanciatedCount {
            get { return totalInstanciatedCount; }
        }

        public MeshDataPool(int matCount, int initialMeshListsSize, int initialCapacity, int initialSize)
        {
            materialsCount = matCount;
            initialListsSize = initialMeshListsSize;
            totalInstanciatedCount = 0;

            if (initialSize > 0) {
                // Init
                items = new Stack<MeshData>(initialSize < initialCapacity ? initialCapacity : initialSize);
#if UT_SAFE_MMMESHPOOL
				_set = new HashSet<MeshData> (initialCapacity);
#endif

                for (var i = 0; i < initialSize; ++i) {
                    var item = new MeshData(materialsCount, initialListsSize);
                    ++totalInstanciatedCount;
                    items.Push(item);
#if UT_SAFE_MMMESHPOOL
					_set.Add (item);
#endif
                }
            } else {
                // Init
                items = new Stack<MeshData>(initialCapacity);
#if UT_SAFE_MMMESHPOOL
				_set = new HashSet<MeshData> (initialCapacity);
#endif
            }
        }

        public MeshData Get()
        {
            if (items.Count == 0) {
                ++totalInstanciatedCount;
                var item = new MeshData(materialsCount, initialListsSize);
                return item;
            }
#if UT_SAFE_MMMESHPOOL
			MeshData item = _items.Pop ();
			_set.Remove (item);
			return item;
#else
            return items.Pop();
#endif
        }

        public void Free(MeshData item)
        {
            if (item != null) {
#if UT_SAFE_MMMESHPOOL
					if (!_set.Contains (item)) {
						_items.Push (item);
						_set.Add (item);
					} else {
#if UT_SAFE_MMMESHPOOL_RAISE_ERROR
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