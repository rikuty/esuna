using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
#if NET_4_6
using System.Collections.Concurrent;
#else
using UltimateTerrains.Concurrent.NET3x;

#endif

namespace UltimateTerrains
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class UltimateOperationsManager
    {
        private readonly int mainThreadId;
        private readonly UltimateTerrain terrain;
        private readonly AsyncOperationOrchestrator asyncOperationOrchestrator;
        private readonly HashSet<Vector3i> loadedRegions;
        private readonly HashSet<Vector3i> regionsToSave;

        private readonly ConcurrentPool<HashSet<Vector3i>> regionsToSaveCopies =
            new ConcurrentPool<HashSet<Vector3i>>(() => new HashSet<Vector3i>(new Vector3iComparer()), 8);

        private readonly ConcurrentDictionary<Vector3i, OperationList>[] operations;
        private readonly Queue<OperationHandler> pendingOperations;
        private readonly Stack<OperationHandler> doneOperations;


        private readonly JsonSerializer serializer;
        private readonly object syncFileAccess = new object();
        private readonly string operationDataPath;
        private readonly string operationRuntimeDataPath;

        private readonly HashSet<Vector3i> mainThreadReportBackOperationsTemporarySet = new HashSet<Vector3i>(new Vector3iComparer());
        private readonly ConcurrentListPool<IOperation> iOperationListPool = new ConcurrentListPool<IOperation>(1, 32);

        private int lastMergeId;

        internal Collider[] AffectedCollidersBuffer { get; private set; }

        /// <summary>
        ///     True is the terrain is ready to perform operations asynchronously. You MUST check this before calling PerformAll
        ///     method.
        /// </summary>
        public bool IsReadyToComputeAsync {
            get { return !asyncOperationOrchestrator.IsComputing; }
        }

        /// <summary>
        ///     Path of the directory in which Operations will be saved when you call PersistModifiedRegionsAsync or
        ///     PersistModifiedRegions methods.
        ///     Will be &lt;Application.persistentDataPath&gt;/uTerrainsData/&lt;runtime-data-path&gt;/runtime_operations
        /// </summary>
        public string RuntimeDataPath {
            get { return operationRuntimeDataPath; }
        }

        internal UltimateOperationsManager(UltimateTerrain uTerrain)
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            terrain = uTerrain;
            asyncOperationOrchestrator = uTerrain.AsyncOperationOrchestrator;
            loadedRegions = new HashSet<Vector3i>(new Vector3iComparer());
            regionsToSave = new HashSet<Vector3i>(new Vector3iComparer());
            pendingOperations = new Queue<OperationHandler>();
            doneOperations = new Stack<OperationHandler>();
            operations = new ConcurrentDictionary<Vector3i, OperationList>[Param.REGION_LEVEL_INDEX + 1];
            for (var i = 0; i < operations.Length; ++i) {
                operations[i] = new ConcurrentDictionary<Vector3i, OperationList>(new Vector3iComparer());
            }

            AffectedCollidersBuffer = new Collider[1024];
            serializer = JsonSerializer.Create(GlobalJsonSettings.Settings);
            operationDataPath = terrain.Params.GetOperationsDataPath();
            operationRuntimeDataPath = terrain.Params.GetOperationsRuntimeDataPath();
        }

        private void CheckIsOnMainThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != mainThreadId) {
                // ReSharper disable once HeapView.ObjectAllocation.Evident
                throw new UltimateTerrainException("This method can only be called from the main thread.");
            }
        }

        /// <summary>
        ///     Persist all operations of all the world at the default data path.
        ///     This method is not suitable for dynamic saving at runtime as it can be slow. It is NOT thread safe.
        /// </summary>
        /// <remarks>
        ///     Must be called from the main thread.
        ///     Terrain data will be stored in one or more files (one file per region).
        ///     If the directory doesn't exist, it will be created.
        /// </remarks>
        /// <param name="clearOldData">
        ///     If true, any existing file will be moved to a backup folder and all operations will be saved again.
        ///     If false, existing files will be preserved and eventually overridden, which is suitable for incremental save.
        /// </param>
        public void Persist(bool clearOldData)
        {
            Persist(operationDataPath, clearOldData);
        }

        /// <summary>
        ///     Persist all operations of all the world at the given path.
        ///     This method is not suitable for dynamic saving at runtime as it can be slow. It is NOT thread safe.
        /// </summary>
        /// <remarks>
        ///     Must be called from the main thread.
        ///     Terrain data will be stored in one or more files (one file per region).
        ///     If the directory doesn't exist, it will be created.
        /// </remarks>
        /// <param name="path">Path where files must be written.null This must be a directory.</param>
        /// <param name="clearOldData">
        ///     If true, any existing file will be moved to a backup folder and all operations will be saved again.
        ///     If false, existing files will be preserved and eventually overridden, which is suitable for incremental save.
        /// </param>
        public void Persist(string path, bool clearOldData)
        {
            CheckIsOnMainThread();

            if (clearOldData) {
                // Load everything before clearing to be sure all regions will be saved again
                LoadAll();
                // "Delete" existing data (actually move it in a backup directory)
                var dir = new DirectoryInfo(path);
                if (dir.Exists) {
                    var bkpPath = path + "_backup";
                    var bkpDir = new DirectoryInfo(bkpPath);
                    if (bkpDir.Exists) {
                        bkpDir.Delete(true);
                    }

                    dir.MoveTo(bkpPath);
                }
            }

            // Create directory if needed
            Directory.CreateDirectory(path);

            // Write file(s)
            var regions = GetAllOperationsByRegions();
            lock (regions) {
                foreach (var pair in regions) {
                    var ops = new List<IOperation>();
                    for (var i = 0; i < pair.Value.Count; i++) {
                        ops.Add(pair.Value[i].Operation);
                    }

                    SerializeOperationsToFile(ops, Path.Combine(path, GetFilenameFromRegion(pair.Key)));
                }
            }
        }

        /// <summary>
        ///     Persist all operations of the region at the given path. A region is an area of 1024x1024x1024 voxels (which
        ///     corresponds to the size of a chunk of LOD 7).
        ///     This method is suitable for dynamic saving at runtime, but it does GC allocation.
        ///     Try to avoid using this too frequently (maybe saving operations each minute is enough for example).
        ///     It is thread safe.
        /// </summary>
        /// <remarks>
        ///     Can be called from a background thread.
        ///     Region data will be stored in one file.
        ///     If the directory doesn't exist, it will be created.
        /// </remarks>
        /// <param name="path">Path where files must be written.null This must be a directory.</param>
        /// <param name="region">
        ///     The region to be persisted. This must be the position of a root chunk, that you can get with
        ///     UnitConverter.ChunkToLeveledChunkPosition(position, Param.MAX_LEVEL).
        /// </param>
        public void PersistRegion(Vector3i region, string path)
        {
            List<IOperation> ops = null;
            OperationList regionOps;
            if (GetOperationsInRegion(region, out regionOps)) {
                ops = iOperationListPool.Get(regionOps.Count);
                for (var i = 0; i < regionOps.Count; i++) {
                    ops.Add(regionOps[i].Operation);
                }
            }

            if (ops != null) {
                // Create directory if needed
                Directory.CreateDirectory(path);
                // Write file
                SerializeOperationsToFile(ops, Path.Combine(path, GetFilenameFromRegion(region)));
                iOperationListPool.Free(ops);
            }
        }

        /// <summary>
        ///     Asynchronously persist all operations of all regions that have been modified since last save, at data path in the
        ///     "dynamic" subfolder.
        ///     This method is suitable for dynamic saving at runtime, but it does some GC allocation.
        ///     Try to avoid using this too frequently (maybe saving operations each minute is enough for example).
        /// </summary>
        /// <remarks>
        ///     Operations will be persisted asynchronously.
        ///     Terrain data will be stored in one or more files (one file per region).
        ///     If the directory doesn't exist, it will be created.
        /// </remarks>
        /// <seealso cref="RuntimeDataPath" />
        public void PersistModifiedRegionsAsync()
        {
            asyncOperationOrchestrator.EnqueuePersistNewOperations();
        }

        /// <summary>
        ///     Persist all operations of all regions that have been modified since last save, at runtime data path.
        ///     This method is suitable for dynamic saving at runtime, but it does some GC allocation.
        ///     Try to avoid using this too frequently (maybe saving operations each minute is enough for example).
        /// </summary>
        /// <remarks>
        ///     Can be called from a background thread.
        ///     Operations will be persisted synchronously so you can be sure saving is done when the method returns.
        ///     Terrain data will be stored in one or more files (one file per region).
        ///     If the directory doesn't exist, it will be created.
        /// </remarks>
        /// <seealso cref="RuntimeDataPath" />
        public void PersistModifiedRegions()
        {
            if (regionsToSave.Count == 0) {
                return; // quick win without thread safety but we don't care as we lock after
            }

            HashSet<Vector3i> regionsToSaveCopy;
            lock (regionsToSave) {
                regionsToSaveCopy = regionsToSaveCopies.Get();
                foreach (var r in regionsToSave) {
                    regionsToSaveCopy.Add(r);
                }

                regionsToSave.Clear();
            }

            foreach (var r in regionsToSaveCopy) {
                PersistRegion(r, operationRuntimeDataPath);
            }

            regionsToSaveCopy.Clear();
            regionsToSaveCopies.Free(regionsToSaveCopy);
        }

        /// <summary>
        ///     Loads ALL operations regardless they are far away from the player or not.
        /// </summary>
        /// <remarks>
        ///     Must be called from the main thread.
        /// </remarks>
        public void LoadAll()
        {
            CheckIsOnMainThread();
            var regions = GetAllRegions(operationDataPath);
            foreach (var region in regions) {
                LoadRegion(region, mainThreadReportBackOperationsTemporarySet);
            }
        }

        /// <summary>
        ///     Loads all operations of a single region.
        /// </summary>
        /// <remarks>
        ///     Must be called from the main thread.
        /// </remarks>
        /// <param name="region">
        ///     The region to be loaded. This must be the position of a root chunk, that you can get with
        ///     UnitConverter.ChunkToLeveledChunkPosition(position, Param.MAX_LEVEL).
        /// </param>
        public void LoadRegionFromMainThread(Vector3i region)
        {
            CheckIsOnMainThread();
            LoadRegion(region, mainThreadReportBackOperationsTemporarySet);
        }

        /// <summary>
        ///     Loads all operations of a single region.
        /// </summary>
        /// <remarks>
        ///     Can be called from a background thread.
        /// </remarks>
        /// <param name="region">
        ///     The region to be loaded. This must be the position of a root chunk, that you can get with
        ///     UnitConverter.ChunkToLeveledChunkPosition(position, Param.MAX_LEVEL).
        /// </param>
        /// <param name="tmpSet">An HashSet to be used internally. Passing this allows to avoid some GC allocation.</param>
        public void LoadRegionFromBackgroundThread(Vector3i region, HashSet<Vector3i> tmpSet)
        {
            LoadRegion(region, tmpSet);
        }

        private void LoadRegion(Vector3i region, HashSet<Vector3i> tmpSet)
        {
            lock (loadedRegions) {
                // prevent a region that has already been loaded from being reloaded
                if (loadedRegions.Contains(region)) {
                    return;
                }

                loadedRegions.Add(region);
            }

            var fpath = Path.Combine(operationRuntimeDataPath, GetFilenameFromRegion(region));
            if (!File.Exists(fpath)) {
                fpath = Path.Combine(operationDataPath, GetFilenameFromRegion(region));
            }

            if (File.Exists(fpath)) {
                DeserializeAndAddOperationsFromFile(region, fpath, tmpSet);
            }
        }

        /// <summary>
        ///     Serializes operation to JSON string and returns it.
        /// </summary>
        /// <param name="operation">The operation to be serialized.</param>
        public string SerializeOperation(IOperation operation)
        {
            return JsonConvert.SerializeObject(operation, GlobalJsonSettings.SettingsLite);
        }

        /// <summary>
        ///     Deserializes operation from JSON and returns it.
        /// </summary>
        /// <param name="json">The JSON string representing the operation.</param>
        public IOperation DeserializeOperation(string json)
        {
            var op = JsonConvert.DeserializeObject<IOperation>(json, GlobalJsonSettings.SettingsLite);
            op.AfterDeserialize(terrain);
            return op;
        }

        /// <summary>
        ///     Serializes operations to JSON and returns it.
        /// </summary>
        /// <param name="operationList">The list of operations to be serialized.</param>
        public string SerializeOperations(List<IOperation> operationList)
        {
            return JsonConvert.SerializeObject(operationList, GlobalJsonSettings.SettingsLite);
        }

        /// <summary>
        ///     Deserializes operations from JSON and returns it.
        /// </summary>
        /// <param name="json">The JSON representing the list of operations.</param>
        public List<IOperation> DeserializeOperations(string json)
        {
            var ops = JsonConvert.DeserializeObject<List<IOperation>>(json, GlobalJsonSettings.SettingsLite);
            foreach (var op in ops) {
                op.AfterDeserialize(terrain);
            }

            return ops;
        }

        private void SerializeOperationsToFile(List<IOperation> ops, string path)
        {
            lock (syncFileAccess) {
                using (var file = File.Create(path))
                using (var sw = new StreamWriter(file))
                using (JsonWriter writer = new JsonTextWriter(sw)) {
                    serializer.Serialize(writer, ops);
                }
            }
        }

        private void DeserializeAndAddOperationsFromFile(Vector3i region, string path, HashSet<Vector3i> tmpSet)
        {
            List<IOperation> ops;
            lock (syncFileAccess) {
                using (var file = File.OpenRead(path))
                using (var sw = new StreamReader(file))
                using (JsonReader reader = new JsonTextReader(sw)) {
                    ops = serializer.Deserialize<List<IOperation>>(reader);
                }
            }

            AddOperationsAfterDeserialize(region, ops, tmpSet);
        }

        private void AddOperationsAfterDeserialize(Vector3i region, List<IOperation> ops, HashSet<Vector3i> tmpSet)
        {
            foreach (var op in ops) {
                op.AfterDeserialize(terrain);
                ReportBackOperationAfterLoading(new OperationHandler(op, terrain, false), region, tmpSet);
            }
        }

        /// <summary>
        ///     Adds operation to the pending queue, making it ready to be performed.
        ///     Operations added with param 'tryMerge' set to true cannot be undone.
        /// </summary>
        /// <remarks>
        ///     Must be called from the main thread.
        /// </remarks>
        /// <param name="operation">Operation to be added to pending queue.</param>
        /// <param name="tryMerge">Let Ultimate Terrains try to merge this operation with others to improve performance.
        /// Caution: operations added with param 'tryMerge' set to true cannot be undone.</param>
        /// <seealso cref="PerformAll" />
        public UltimateOperationsManager Add(IOperation operation, bool tryMerge)
        {
            CheckIsOnMainThread();
            var op = new OperationHandler(operation, terrain, tryMerge);
            pendingOperations.Enqueue(op);
            return this;
        }

        /// <summary>
        ///     Adds operation to the internal list of operations as if it had been loaded.
        ///     Should be called only from OnBeforeLoad events.
        /// </summary>
        /// <remarks>
        ///     Must be called from the main thread.
        /// </remarks>
        /// <param name="operation">Operation to be added to the internal list.</param>
        /// <seealso cref="UltimateTerrain.OnBeforeLoad" />
        public UltimateOperationsManager AddBeforeLoad(IOperation operation)
        {
            CheckIsOnMainThread();
            var op = new OperationHandler(operation, terrain, false);
            ReportBackOperationImmediately(op, op.TryMerge);
            return this;
        }

        /// <summary>
        ///     Performs all pending operations.
        /// </summary>
        /// <remarks>
        ///     Must be called from the main thread.
        /// </remarks>
        /// <param name="performAsync">If true, operations will be performed asynchronously. Recommended at runtime to avoid lags.</param>
        public void PerformAll(bool performAsync)
        {
            CheckIsOnMainThread();
            if (performAsync && !IsReadyToComputeAsync) {
                throw new UltimateTerrainException("Cannot perform operations asynchronously when Operation Manager is not ready. " +
                                                   "Did you forget to check 'if (terrain.OperationsManager.IsReadyToComputeAsync)' before calling 'PerformAll' method?");
            }

            var alreadyBuildChunksSet = new HashSet<Chunk>();
            var alreadyRecomputedChunksSet = new HashSet<Chunk>();
            while (pendingOperations.Count > 0) {
                OperationHandler op;
                op = pendingOperations.Dequeue();
                ReportBackOperationImmediately(op, op.TryMerge);
                op.Perform(performAsync, alreadyBuildChunksSet, alreadyRecomputedChunksSet, false, AffectedCollidersBuffer);
                doneOperations.Push(op);
            }

            terrain.Orchestrator.RefreshBuildChunks();
            if (performAsync) {
                terrain.AsyncOperationOrchestrator.StartOrchestration();
            }
        }

        /// <summary>
        ///     Undo latest operation(s). Operations that was added with param 'tryMerge' set to true cannot be undone.
        /// </summary>
        /// <remarks>
        ///     Must be called from the main thread.
        /// </remarks>
        /// <param name="undoCount">The number of operations to undo.</param>
        /// <param name="performAsync">If true, operations will be undone asynchronously. Recommended at runtime to avoid lags.</param>
        public void Undo(int undoCount, bool performAsync)
        {
            CheckIsOnMainThread();

            var removedOperations = new Queue<OperationHandler>();
            var endCount = Math.Max(0, doneOperations.Count - undoCount);

            while (doneOperations.Count > endCount) {
                var op = doneOperations.Peek();
                if (op.TryMerge)
                    break;
                op = doneOperations.Pop();
                RemoveLastOperation(op);
                removedOperations.Enqueue(op);
            }

            var alreadyBuildChunksSet = new HashSet<Chunk>();
            var alreadyRecomputedChunksSet = new HashSet<Chunk>();
            while (removedOperations.Count > 0) {
                // Recompute affected chunks
                removedOperations.Dequeue().Perform(performAsync, alreadyBuildChunksSet, alreadyRecomputedChunksSet, true, AffectedCollidersBuffer);
            }

            terrain.Orchestrator.RefreshBuildChunks();
            if (performAsync) {
                terrain.AsyncOperationOrchestrator.StartOrchestration();
            }
        }

        internal OperationList GetOperations(Vector3i chunkPosition, int levelIndex)
        {
            OperationList operationList;
            if (levelIndex < operations.Length && operations[levelIndex].TryGetValue(chunkPosition, out operationList)) {
                return operationList;
            }

            return null;
        }

        internal OperationList GetOperationsAt(Vector3i chunkPosition)
        {
            return GetOperations(chunkPosition, 0);
        }

        internal static bool IsAffectedByOperation(OperationList operationList, OctreeNode node, int operationCount, Vector3i chunkWorldPos, int chunkLevel, out bool isCompletelyInsideOperation)
        {
            isCompletelyInsideOperation = false;
            var worldFrom = chunkWorldPos + node.From * chunkLevel * Param.SIZE_OFFSET_INVERSE;
            var worldTo = chunkWorldPos + node.To * chunkLevel * Param.SIZE_OFFSET_INVERSE;
            for (var i = operationCount - 1; i >= 0; --i) {
                if (operationList[i].HasEffectOnArea(worldFrom, worldTo)) {
                    isCompletelyInsideOperation = operationList[i].IsCompletelyInsideAreaOfEffect(worldFrom, worldTo);
                    return true;
                }
            }

            return false;
        }

        private void RemoveLastOperation(OperationHandler operation)
        {
            ReportBackOperation(operation, true, false, null, mainThreadReportBackOperationsTemporarySet);
        }

        private void ReportBackOperationImmediately(OperationHandler operation, bool tryMerge)
        {
            ReportBackOperation(operation, false, tryMerge, null, mainThreadReportBackOperationsTemporarySet);
        }

        private void ReportBackOperationAfterLoading(OperationHandler operation, Vector3i region, HashSet<Vector3i> tmpSet)
        {
            ReportBackOperation(operation, false, false, region, tmpSet);
        }

        private void ReportBackOperation(OperationHandler operation, bool undo, bool tryMerge, Vector3i? limitToRegion, HashSet<Vector3i> tmpSet)
        {
            var mergeId = Interlocked.Increment(ref lastMergeId);

            for (var i = 0; i < operations.Length; ++i) {
                var ops = operations[i];
                tmpSet.Clear();
                var lvl = UnitConverter.LevelIndexToLevel(i);

                foreach (var chunkWorldPos in operation.AffectedVirtualChunks) {
                    if (limitToRegion.HasValue) {
                        var regionedWorldPos = UnitConverter.ChunkToLeveledChunkPosition(chunkWorldPos, Param.REGION_LEVEL);
                        if (regionedWorldPos != limitToRegion.Value)
                            continue;
                    }
                    
                    var leveledWorldPos = UnitConverter.ChunkToLeveledChunkPosition(chunkWorldPos, lvl);

                    if (i == 0 || !tmpSet.Contains(leveledWorldPos)) {
                        if (i != 0) {
                            tmpSet.Add(leveledWorldPos);
                            if (i == Param.REGION_LEVEL_INDEX) {
                                AddRegionToSave(leveledWorldPos);
                            }
                        }

                        OperationList operationList;
                        if (!ops.TryGetValue(leveledWorldPos, out operationList)) {
                            if (!undo) {
                                operationList = ops.GetOrAdd(leveledWorldPos, new OperationList());
                            } else {
                                operationList = null;
                            }
                        }

                        if (!undo) {
                            if (tryMerge) {
                                operationList.AddOrMerge(mergeId, operation);
                            } else {
                                operationList.Add(operation);
                            }
                        } else if (operationList != null) {
                            operationList.RemoveLastWhenEqual(operation);
                        }
                    }
                }
            }
        }

        private void AddRegionToSave(Vector3i region)
        {
            lock (regionsToSave)
                regionsToSave.Add(region);
        }

        private ConcurrentDictionary<Vector3i, OperationList> GetAllOperationsByRegions()
        {
            return operations[Param.REGION_LEVEL_INDEX];
        }

        private bool GetOperationsInRegion(Vector3i region, out OperationList ops)
        {
            return GetAllOperationsByRegions().TryGetValue(region, out ops);
        }

        private string GetFilenameFromRegion(Vector3i region, bool addJsonExt = true)
        {
            return Param.FILE_PREFIX +
                   region.x.ToString(CultureInfo.InvariantCulture) + "_" +
                   region.y.ToString(CultureInfo.InvariantCulture) + "_" +
                   region.z.ToString(CultureInfo.InvariantCulture) +
                   (addJsonExt ? Param.FILE_EXTENSION : Param.FILE_SUFFIX);
        }

        private Vector3i GetRegionFromFilename(string filename)
        {
            var coords = filename.Replace(Param.FILE_PREFIX, "").Replace(Param.FILE_EXTENSION, "")
                                 .Replace(Param.FILE_SUFFIX, "").Split('_');
            return new Vector3i(int.Parse(coords[0], CultureInfo.InvariantCulture),
                                int.Parse(coords[1], CultureInfo.InvariantCulture),
                                int.Parse(coords[2], CultureInfo.InvariantCulture));
        }

        private List<Vector3i> GetAllRegions(string path)
        {
            var regions = new List<Vector3i>();
            if (Directory.Exists(path)) {
                var dir = new DirectoryInfo(path);
                foreach (var file in dir.GetFiles()) {
                    if (file.Name.EndsWith(Param.FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)) {
                        regions.Add(GetRegionFromFilename(file.Name));
                    }
                }
            }

            return regions;
        }
    }
}