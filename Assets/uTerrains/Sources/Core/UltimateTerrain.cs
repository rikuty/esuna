using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

namespace UltimateTerrains
{
    [AddComponentMenu("Ultimate Terrains/Terrain")]
    [RequireComponent(typeof(GeneratorModulesComponent))]
    [RequireComponent(typeof(Orchestrator), typeof(AsyncOperationOrchestrator))]
    public sealed class UltimateTerrain : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] public bool MiscSettingsFoldoutForEditor = true;
        [SerializeField] public bool TerrainSizeFoldoutForEditor;
        [SerializeField] public bool DataLoadingFoldoutForEditor;
        [SerializeField] public bool GeometricErrorFoldoutForEditor;
        [SerializeField] public bool InitialChunkCountFoldoutForEditor;
#endif

        public const string CHUNKS_PARENT_NAME = "Chunks";
        public const string DETAILS_PARENT_NAME = "Details";
        public const string TREES_PARENT_NAME = "Trees";

        // Static
        private static Vector3i mainCameraChunkPosition;
        private Vector3i currentCameraChunkPosition;
        
        // For preview
        public delegate void OverrideParam(Param p);

        // Events
        public delegate void LoadedEventHandler(UltimateTerrain sender);

        public static event LoadedEventHandler OnLoaded;

        public delegate void BeforeLoadEventHandler(UltimateTerrain sender);

        public static event BeforeLoadEventHandler OnBeforeLoad;

        public delegate void ReloadForPreviewEventHandler(UltimateTerrain sender);

        public event ReloadForPreviewEventHandler OnReloadForPreview;
        
        public delegate void CommitEventHandler(UltimateTerrain sender);

        public static event CommitEventHandler OnCommitted;

        // Important fields
        public Transform PlayerTransform;
        private Transform lodOriginTransform;

        private GeneratorModulesComponent generatorModules;
        private Orchestrator orchestrator;
        private AsyncOperationOrchestrator asyncOperationOrchestrator;
        private ChunkGenerator mainThreadChunkGenerator;
        private DetailObjectsIndexer detailObjectsIndexer;
        private Transform chunksParent;
        private Transform detailsParent;
        private Transform treesParent;
        public bool IsStarting;
        private bool isPaused;

        [SerializeField] private Param param;

        private Param paramToUse;
        
        public Param ParamsForEditor {
            get {
                if (param == null) {
                    param = new Param();
                    paramToUse = param;
                }

                return param;
            }
        }

        public Param Params {
            get {
                if (param == null) {
                    param = new Param();
                    paramToUse = param;
                }

                if (paramToUse == null) {
                    paramToUse = param;
                }

                return paramToUse;
            }
        }

        private UnitConverter converter;

        public UnitConverter Converter {
            get { return converter; }
        }

        [SerializeField] private VoxelTypeSet voxelTypeSet;

        public VoxelTypeSet VoxelTypeSet {
            get {
                if (voxelTypeSet == null) {
                    voxelTypeSet = new VoxelTypeSet();
                }

                return voxelTypeSet;
            }
        }

        // Data
        private Chunk[] chunks;
        private TreesChunk[] treesChunks;
        private UltimateOperationsManager operationsManager;


        // Fast access to params
        public int MaxChunkLevel;
        public int ChunkLevelCount;
        private bool loadDynamically;
        private int buildWidth, buildDistance, verticalBuildWidth, verticalBuildDistance, chunkTreeCount, sqrBuildWidth;

        private DateTime startTime;
        private Stopwatch postBuildWatch;

        public bool IsStarted {
            get { return chunks != null; }
        }

        public bool IsLoaded {
            get { return orchestrator != null && orchestrator.IsFirstLoadDone; }
        }

        internal Chunk[] Chunks {
            get { return chunks; }
        }

        internal TreesChunk[] TreesChunks {
            get { return treesChunks; }
        }

        public UltimateOperationsManager OperationsManager {
            get { return operationsManager; }
        }

        public GeneratorModulesComponent GeneratorModules {
            get { return generatorModules; }
            set { generatorModules = value; }
        }

        internal ChunkGenerator MainThreadChunkGenerator {
            get { return mainThreadChunkGenerator; }
        }

        internal DetailObjectsIndexer DetailObjectsIndexer {
            get { return detailObjectsIndexer; }
        }

        public static Vector3i CameraChunkPosition {
            get { return mainCameraChunkPosition; }
        }

        public Vector3i CurrentCameraChunkPosition {
            get { return currentCameraChunkPosition; }
        }

        public Orchestrator Orchestrator {
            get { return orchestrator; }
        }

        internal AsyncOperationOrchestrator AsyncOperationOrchestrator {
            get { return asyncOperationOrchestrator; }
        }

        internal static int ThreadCount {
            get {
#if !NO_MULTITHREAD && !UPROFILE_MEMORY
                return Math.Min(Math.Max(2, Environment.ProcessorCount - 1), 7);
#else
                return 1;
#endif
            }
        }

        // Invoke the OnLoaded event; called when terrain has been finally loaded
        internal void TriggerOnLoaded()
        {
            if (OnLoaded != null)
                OnLoaded(this);
        }
        
        // Invoke the OnCommitted event; called each time terrain has been updated
        internal void TriggerOnCommitted()
        {
            if (OnCommitted != null)
                OnCommitted(this);
        }

        // Don't use this.
        public void OverridePlayerTransform(Transform t)
        {
            lodOriginTransform = t;
        }

        private bool Validate()
        {
            if (!PlayerTransform) {
                UDebug.LogError("'Player' isn't defined.");
                return false;
            }

            if (voxelTypeSet == null || !voxelTypeSet.Validate()) {
                UDebug.LogError("Voxel types are not well defined.");
                return false;
            }

            if (generatorModules == null || !generatorModules.Validate(this, true)) {
                UDebug.LogError("Generator Modules are not well defined.");
                return false;
            }

            return true;
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Unpause()
        {
            isPaused = false;
        }

        public void Reset()
        {
            UDebug.Log("Destroying terrain...");
            var children = new List<GameObject>();
            // Destroy all chunks and their mesh
            if (chunksParent) {
                foreach (Transform child in chunksParent) {
                    children.Add(child.gameObject);
                    var comp = child.GetComponent<ChunkComponent>();
                    if (comp != null) {
                        comp.DestroyMeshAndReset();
                    } else {
                        UDebug.LogWarning("You should never add any object as a child of a Ultimate terrain. All children are destroyed.");
                    }
                }

                children.ForEach(child => DestroyImmediate(child));
            }

            children.Clear();

            // Destroy all direct children of the terrain
            if (this && transform) {
                foreach (Transform child in transform) {
                    children.Add(child.gameObject);
                    if (child.name != CHUNKS_PARENT_NAME && child.name != DETAILS_PARENT_NAME && child.name != TREES_PARENT_NAME) {
                        UDebug.LogWarning("You should never add any object as a child of a Ultimate terrain. All children are destroyed.");
                    }
                }

                children.ForEach(child => DestroyImmediate(child));
            }

            // Reset variables and stop all threads
            chunks = null;
            treesChunks = null;
            if (orchestrator != null) {
                orchestrator.Stop();
                orchestrator = null;
            }

            if (asyncOperationOrchestrator != null) {
                asyncOperationOrchestrator.Stop();
                asyncOperationOrchestrator = null;
            }

            operationsManager = null;
            mainThreadChunkGenerator = null;
            converter = null;
            generatorModules = null;
            detailObjectsIndexer = null;
            if (postBuildWatch != null) {
                postBuildWatch.Stop();
                postBuildWatch = null;
            }

            ChunkObjectPool.Reset();
            TreesObjectPools.Reset();

            GC.Collect();
            UDebug.Log("Terrain destroyed.");
        }

        public void OnDestroy()
        {
            Reset();
        }

        public void InitFastAccessToParams()
        {
            MaxChunkLevel = Params.MaxLevel;
            ChunkLevelCount = Params.LevelCount;
            buildWidth = Params.BuildWidth;
            verticalBuildWidth = Params.VerticalBuildWidth;
            sqrBuildWidth = buildWidth * verticalBuildWidth;
            buildDistance = Params.BuildDistance;
            verticalBuildDistance = Params.VerticalBuildDistance;
            chunkTreeCount = Params.ChunkTreeCount;
            loadDynamically = Params.LoadDynamically;
        }

        public Transform GetChunksParent()
        {
            if (chunksParent == null) {
                var go = new GameObject(CHUNKS_PARENT_NAME) {hideFlags = Param.HideFlags};
                chunksParent = go.transform;
                chunksParent.parent = transform;
            }

            return chunksParent;
        }

        public Transform GetDetailsParent()
        {
            if (detailsParent == null) {
                var go = new GameObject(DETAILS_PARENT_NAME) {hideFlags = Param.HideFlags};
                detailsParent = go.transform;
                detailsParent.parent = transform;
            }

            return detailsParent;
        }

        public Transform GetTreesParent()
        {
            if (treesParent == null) {
                var go = new GameObject(TREES_PARENT_NAME) {hideFlags = Param.HideFlags};
                treesParent = go.transform;
                treesParent.parent = transform;
            }

            return treesParent;
        }

        private void UpdateMainCameraChunkPosition()
        {
            mainCameraChunkPosition = UnitConverter.VoxelToChunkPosition(converter.UnityToVoxelPositionRound(lodOriginTransform.position));
        }

        private void Initialize(OverrideParam overrideParam = null, int? forcedLevel = null)
        {
            Profiler.BeginSample("Initialize");
            IsStarting = true;
            isPaused = false;
            startTime = DateTime.Now;
            postBuildWatch = new Stopwatch();

            // MUST BE DONE BEFORE EVERYTHING ELSE
            if (voxelTypeSet == null || param == null) {
                UDebug.Fatal("Some internal objects of the terrain aren't set properly.");
            }

            if (overrideParam != null) {
                paramToUse = new Param(param);
                overrideParam.Invoke(paramToUse);
            } else {
                paramToUse = param;
            }

            Params.Init();
            InitFastAccessToParams();

            // MUST BE DONE BEFORE EVERYTHING ELSE bis
            lodOriginTransform = PlayerTransform;
            converter = new UnitConverter(Params);
            voxelTypeSet.Init(Params);
            detailObjectsIndexer = new DetailObjectsIndexer(this);
            generatorModules = GetComponent<GeneratorModulesComponent>();
            orchestrator = GetComponent<Orchestrator>();
            asyncOperationOrchestrator = GetComponent<AsyncOperationOrchestrator>();

            // Validation
            if (!Validate()) {
                UDebug.Fatal("Terrain isn't valid.");
            }

            // Load - MUST BE DONE BEFORE INIT
            Profiler.BeginSample("Create UltimateOperationsManager");
            operationsManager = new UltimateOperationsManager(this);
            Profiler.EndSample();
            Profiler.BeginSample("Create main thread ChunkGenerator");
            mainThreadChunkGenerator = new ChunkGenerator(this);
            Profiler.EndSample();

            UpdateMainCameraChunkPosition();
            currentCameraChunkPosition = mainCameraChunkPosition;

            if (OnBeforeLoad != null) {
                OnBeforeLoad(this);
            }

            // Load terrain data synchronously at start
            if (!loadDynamically) {
                operationsManager.LoadAll();
            }

            // Populate chunk pool
            Profiler.BeginSample("Create ChunkObjectPool");
            ChunkObjectPool.Init(this);
            Profiler.EndSample();

            Profiler.BeginSample("Init Chunks");
            InitializeChunksFromScratch();
            Profiler.EndSample();

            Profiler.BeginSample("Init Trees");
            TreesObjectPools.Init(this);
            InitializeTreesChunks();
            Profiler.EndSample();

            Profiler.BeginSample("Init Orchestrator");
            orchestrator.Init(this, forcedLevel);
            Profiler.EndSample();

            Profiler.BeginSample("Init AsyncOperationOrchestrator");
            asyncOperationOrchestrator.Init(this, orchestrator);
            Profiler.EndSample();

            // Finally, call a GC Collect to free some memory
            UDebug.Log("Initialization done in " + (DateTime.Now - startTime) + "s. Generating terrain, please wait...");
            Profiler.EndSample();
            GC.Collect();
        }

        public bool HotReload()
        {
            if (!IsStarted || IsStarting || orchestrator.HotReloading)
                return false;
            
            if (OnReloadForPreview != null)
                OnReloadForPreview(this);
            
            orchestrator.HotReload();
            return true;
        }

        private void InitializeChunksFromScratch()
        {
            var reportBackOperationsTemporarySet = new HashSet<Vector3i>(new Vector3iComparer());
            // Init chunks trees
            chunks = new Chunk[chunkTreeCount];
            for (var cx = -buildDistance; cx < buildDistance; ++cx) {
                for (var cy = -verticalBuildDistance; cy < verticalBuildDistance; ++cy) {
                    for (var cz = -buildDistance; cz < buildDistance; ++cz) {
                        var chunk = new Chunk();
                        var chunkPos = new Vector3i(cx, cy, cz) * MaxChunkLevel;
                        chunk.Init(-1, this, reportBackOperationsTemporarySet, chunkPos, Vector3i.zero, MaxChunkLevel, null);
                        chunks[cx + buildDistance + buildWidth * (cy + verticalBuildDistance) + sqrBuildWidth * (cz + buildDistance)] = chunk;
                    }
                }
            }
        }

        private void InitializeTreesChunks()
        {
            var watch = Stopwatch.StartNew();
            // Init chunks trees
            treesChunks = new TreesChunk[buildWidth * buildWidth];
            for (var cx = -buildDistance; cx < buildDistance; ++cx) {
                for (var cz = -buildDistance; cz < buildDistance; ++cz) {
                    var chunk = new TreesChunk();
                    var chunkPos = new Vector2i(cx, cz) * Params.MaxLevelTrees;
                    chunk.Init(this, chunkPos);
                    treesChunks[cx + buildDistance + buildWidth * (cz + buildDistance)] = chunk;
                }
            }

            watch.Stop();
            UDebug.Log("Initialized trees in " + watch.ElapsedMilliseconds + "ms");
        }

        // Called once at start or when entering edit mode
        public void Start()
        {
            Initialize();

            orchestrator.StartOrchestration(false);

            IsStarting = false;
            GC.Collect();
        }


        public void StartFromEditor(OverrideParam overrideParam = null, int? forcedLevel = null)
        {
            Initialize(overrideParam, forcedLevel);

            orchestrator.StartOrchestration(true);

            IsStarting = false;
            GC.Collect();
        }


        // Called once per frame
        public void Update()
        {
            if (isPaused)
                return;

            // Update static camera position which can be used from any thread. This is compatible with a scene containing several terrains.
            UpdateMainCameraChunkPosition();
        }

        internal void UpdateCurrentCameraChunkPosition()
        {
            currentCameraChunkPosition = mainCameraChunkPosition;
        }

        /// <summary>
        ///     Computes voxel at given voxel-world-position and returns it.
        /// </summary>
        /// <remarks>
        ///     Don't use it to iterate over voxels because it may be slow.
        /// </remarks>
        /// <returns>The voxel at the given world position.</returns>
        public Voxel GetVoxelAt(double x, double y, double z)
        {
            // Get operations that affect the chunk
            var operations = operationsManager.GetOperationsAt(UnitConverter.VoxelToChunkPosition(x, y, z));
            // Get a new cache
            var cache = mainThreadChunkGenerator.ChunkGeneratorPools.ChunkCachePool.Get();
            // Prepare the generator
            mainThreadChunkGenerator.VoxelGenerator.PrepareWithoutChunk(operations, cache);
            // Finally, compute the voxel (or get it from the cache if it exists)
            var voxel = mainThreadChunkGenerator.VoxelGenerator.GenerateWithoutChunk(x, y, z);
            // Free cache
            cache.ResetCache(mainThreadChunkGenerator.ChunkGeneratorPools.Float2DModulesPool);
            mainThreadChunkGenerator.ChunkGeneratorPools.ChunkCachePool.Free(cache);
            return voxel;
        }

        /// <summary>
        ///     Computes voxel at given voxel-world-position and returns it.
        /// </summary>
        /// <remarks>
        ///     Don't use it to iterate over voxels because it may be slow.
        /// </remarks>
        /// <param name="worldVoxelPosition">
        ///     World voxel position (use terrain.Converter.UnityToVoxelPositionRound or
        ///     terrain.Converter.UnityToVoxelPositionFloor if needed)
        /// </param>
        /// <returns>The voxel at the given world position.</returns>
        public Voxel GetVoxelAt(Vector3d worldVoxelPosition)
        {
            return GetVoxelAt(worldVoxelPosition.x, worldVoxelPosition.y, worldVoxelPosition.z);
        }

        /// <summary>
        ///     Computes voxel at given voxel-world-position and returns it.
        /// </summary>
        /// <remarks>
        ///     Don't use it to iterate over voxels because it may be slow.
        /// </remarks>
        /// <param name="worldVoxelPosition">
        ///     World voxel position (use terrain.Converter.UnityToVoxelPositionRound or
        ///     terrain.Converter.UnityToVoxelPositionFloor if needed)
        /// </param>
        /// <returns>The voxel at the given world position.</returns>
        public Voxel GetVoxelAt(Vector3i worldVoxelPosition)
        {
            return GetVoxelAt(worldVoxelPosition.x, worldVoxelPosition.y, worldVoxelPosition.z);
        }


        /// <summary>
        ///     Perform a raycast over all the terrain, even on chunks that don't have colliders.
        /// </summary>
        /// <returns>True if the terrain was hit. False otherwise.</returns>
        public bool Raycast(Ray ray, float distance, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            var alreadyTraversedChunksSet = new HashSet<Chunk>();
            var startPos = converter.UnityToChunkPosition(ray.origin);
            var endPos = converter.UnityToChunkPosition(ray.origin + ray.direction * distance);
            Vector3 _hitPoint, _hitNormal;
            _hitPoint = _hitNormal = Vector3.zero;

            var hit = GridUtils.TraverseCellsIn3DGrid(startPos, endPos, (cx, cy, cz) => {
                var chunk = GetChunkVisible(new Vector3i(cx, cy, cz));
                if (!alreadyTraversedChunksSet.Contains(chunk)) {
                    alreadyTraversedChunksSet.Add(chunk);
                    return chunk != null && chunk.Raycast(ray, distance, out _hitPoint, out _hitNormal);
                }

                return false;
            });
            if (hit) {
                hitPoint = _hitPoint;
                hitNormal = _hitNormal;
                return true;
            }

            hitPoint = Vector3.zero;
            hitNormal = Vector3.zero;
            return false;
        }


        internal Chunk GetChunkRoot(Vector3i chunkTreePos)
        {
            return chunks[UMath.Mod(chunkTreePos.x + buildDistance, buildWidth) + // X
                          buildWidth * UMath.Mod(chunkTreePos.y + verticalBuildDistance, verticalBuildWidth) + // Y
                          sqrBuildWidth * UMath.Mod(chunkTreePos.z + buildDistance, buildWidth)]; // Z
        }

        internal Chunk GetChunkRootFromChunkPos(Vector3i chunkPos)
        {
            return GetChunkRoot(converter.ChunkToChunkTreePosition(chunkPos));
        }

        internal Chunk GetChunkVisible(Vector3i chunkWorldPosition)
        {
            var root = GetChunkRoot(converter.ChunkToChunkTreePosition(chunkWorldPosition));
            return root.TraverseVisibleTree(chunkWorldPosition, false);
        }

        internal Chunk GetChunkThatWillBeVisible(Vector3i chunkWorldPosition)
        {
            var root = GetChunkRoot(converter.ChunkToChunkTreePosition(chunkWorldPosition));
            return root.TraverseVisibleTree(chunkWorldPosition, true);
        }

        internal Chunk GetChunkOfLevel(Vector3i chunkWorldPosition, int level)
        {
            var root = GetChunkRoot(converter.ChunkToChunkTreePosition(chunkWorldPosition));
            return root.TraverseTree(chunkWorldPosition, level);
        }

        
    }
}