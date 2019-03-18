using System.Diagnostics.CodeAnalysis;

namespace UltimateTerrains
{
    internal sealed class TreesSpawnerPool
    {
        private readonly UnsafePool<TreeEntry> treeEntryPool = new UnsafePool<TreeEntry>(() => new TreeEntry(), 1024, 16);

        internal UnsafePool<TreeEntry> TreeEntryPool {
            get { return treeEntryPool; }
        }

        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
        public void DebugLog()
        {
            UDebug.Log("-------------------------------------------------------------------------------");
            UDebug.Log("There are currently " + treeEntryPool.Count + "/" + treeEntryPool.TotalInstanciatedCount + " Tree Entries in pool.");
            UDebug.Log("-------------------------------------------------------------------------------");
        }
    }
}