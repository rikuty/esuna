using System.Collections.Generic;

namespace UltimateTerrains
{
    internal sealed class DetailsPool
    {
        private readonly Stack<Details> items;
        private readonly int maxDetailsCountPerChunkPerType;
        private readonly DetailObjectsIndexer detailObjectsIndexer;

        public int Count {
            get { return items.Count; }
        }

        public int TotalInstanciatedCount { get; private set; }

        public DetailsPool(UltimateTerrain terrain, int initialCapacity, int initialSize)
        {
            TotalInstanciatedCount = 0;
            maxDetailsCountPerChunkPerType = terrain.Params.MaxDetailsCountPerChunkPerType;
            detailObjectsIndexer = terrain.DetailObjectsIndexer;

            if (initialSize > 0) {
                // Init
                items = new Stack<Details>(initialSize < initialCapacity ? initialCapacity : initialSize);

                for (var i = 0; i < initialSize; ++i) {
                    var item = new Details(maxDetailsCountPerChunkPerType, detailObjectsIndexer);
                    ++TotalInstanciatedCount;
                    items.Push(item);
                }
            } else {
                // Init
                items = new Stack<Details>(initialCapacity);
            }
        }

        public Details Get()
        {
            if (items.Count == 0) {
                ++TotalInstanciatedCount;
                var item = new Details(maxDetailsCountPerChunkPerType, detailObjectsIndexer);
                return item;
            }

            return items.Pop();
        }

        public void Free(Details item)
        {
            if (item != null) {
                item.Reset();
                items.Push(item);
            }
        }
    }
}