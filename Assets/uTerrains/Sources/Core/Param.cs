using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public sealed class Param
    {
        // Misc
        public const double PRECISION = 1E-09;
        public const string FILE_PREFIX = "ops_"; // Ultimate Terrain Data
        public const string FILE_SUFFIX = ".utd"; // Ultimate Terrain Data
        public const string FILE_EXTENSION = ".utd.json"; // Ultimate Terrain Data
        public const int MAX_LEVEL_COUNT = 11;
        public const int REGION_LEVEL_INDEX = 6;
        public const int REGION_LEVEL = 1 << REGION_LEVEL_INDEX;
        public const int MAX_LEVEL = 1 << (MAX_LEVEL_COUNT - 1);

        public Param()
        {
        }

        public Param(Param other)
        {
            this.DataPath = other.DataPath;
            this.RuntimeDataPath = other.RuntimeDataPath;
            this.buildWidth = other.buildWidth;
            this.verticalBuildWidth = other.verticalBuildWidth;
            this.chunkTreeCount = other.chunkTreeCount;
            this.buildDistanceChunk = other.buildDistanceChunk;
            this.verticalBuildDistanceChunk = other.verticalBuildDistanceChunk;
            this.treesBuildDistanceChunk = other.treesBuildDistanceChunk;
            this.regionSize = other.regionSize;
            this.LoadDynamically = other.LoadDynamically;
            this.maxLevel = other.maxLevel;
            this.maxLevelBits = other.maxLevelBits;
            this.leveledLod1BaseError = other.leveledLod1BaseError;
            this.leveledLod2BaseError = other.leveledLod2BaseError;
            this.leveledLod4BaseError = other.leveledLod4BaseError;
            this.leveledLod8BaseError = other.leveledLod8BaseError;
            this.leveledLod16BaseError = other.leveledLod16BaseError;
            this.leveledLod32BaseError = other.leveledLod32BaseError;
            this.leveledLod64BaseError = other.leveledLod64BaseError;
            this.ChunkConnectionMode = other.ChunkConnectionMode;
            this.GrassLayer = other.GrassLayer;
            this.GrassCastShadows = other.GrassCastShadows;
            this.DetailsParams = other.DetailsParams;
            this.TreesParams = other.TreesParams;
            this.ChunkLayer = other.ChunkLayer;
            this.chunkLayerMask = other.chunkLayerMask;
            this.maxLevelCollider = other.maxLevelCollider;
            this.maxLevelGrass = other.maxLevelGrass;
            this.maxLevelShadows = other.maxLevelShadows;
            this.maxLevelDetails = other.maxLevelDetails;
            this.maxLevelTrees = other.maxLevelTrees;
            this.SizeXVoxelF = other.SizeXVoxelF;
            this.SizeYVoxelF = other.SizeYVoxelF;
            this.SizeZVoxelF = other.SizeZVoxelF;
            this.SizeXTotal = other.SizeXTotal;
            this.SizeYTotal = other.SizeYTotal;
            this.SizeZTotal = other.SizeZTotal;
            this.SizeXTotalF = other.SizeXTotalF;
            this.SizeYTotalF = other.SizeYTotalF;
            this.SizeZTotalF = other.SizeZTotalF;
            this.DistanceBetweenVoxelsOfHighestLOD_X = other.DistanceBetweenVoxelsOfHighestLOD_X;
            this.DistanceBetweenVoxelsOfHighestLOD_Y = other.DistanceBetweenVoxelsOfHighestLOD_Y;
            this.DistanceBetweenVoxelsOfHighestLOD_Z = other.DistanceBetweenVoxelsOfHighestLOD_Z;
            this.DistanceBetweenVoxelsOfHighestLOD_X_Inverse = other.DistanceBetweenVoxelsOfHighestLOD_X_Inverse;
            this.DistanceBetweenVoxelsOfHighestLOD_Y_Inverse = other.DistanceBetweenVoxelsOfHighestLOD_Y_Inverse;
            this.DistanceBetweenVoxelsOfHighestLOD_Z_Inverse = other.DistanceBetweenVoxelsOfHighestLOD_Z_Inverse;
#if UNITY_EDITOR
            this.detailsFoldoutForEditor = other.detailsFoldoutForEditor;
            this.treesFoldoutForEditor = other.treesFoldoutForEditor;
#endif
        }


        //-----------------------------------------------------------------------
        // Map

        public void Init()
        {
            SizeXVoxelF = (float) SizeXVoxel;
            SizeYVoxelF = (float) SizeYVoxel;
            SizeZVoxelF = (float) SizeZVoxel;
            SizeXTotal = SIZE_X * SizeXVoxel;
            SizeYTotal = SIZE_Y * SizeYVoxel;
            SizeZTotal = SIZE_Z * SizeZVoxel;
            SizeXTotalF = (float) SizeXTotal;
            SizeYTotalF = (float) SizeYTotal;
            SizeZTotalF = (float) SizeZTotal;
            DistanceBetweenVoxelsOfHighestLOD_X = SizeXVoxel * (MAX_LEVEL_COUNT + 1);
            DistanceBetweenVoxelsOfHighestLOD_Y = SizeYVoxel * (MAX_LEVEL_COUNT + 1);
            DistanceBetweenVoxelsOfHighestLOD_Z = SizeZVoxel * (MAX_LEVEL_COUNT + 1);
            DistanceBetweenVoxelsOfHighestLOD_X_Inverse = 1.0 / DistanceBetweenVoxelsOfHighestLOD_X;
            DistanceBetweenVoxelsOfHighestLOD_Y_Inverse = 1.0 / DistanceBetweenVoxelsOfHighestLOD_Y;
            DistanceBetweenVoxelsOfHighestLOD_Z_Inverse = 1.0 / DistanceBetweenVoxelsOfHighestLOD_Z;
            maxLevelBits = LevelCount - 1;
            maxLevel = 1 << MaxLevelBits; // = pow(2, MAX_LEVEL_BITS)
            buildWidth = 2 * BuildDistance;
            verticalBuildWidth = 2 * VerticalBuildDistance;
            chunkTreeCount = buildWidth * verticalBuildWidth * buildWidth;
            buildDistanceChunk = BuildDistance * MaxLevel;
            verticalBuildDistanceChunk = VerticalBuildDistance * MaxLevel;
            chunkLayerMask = 1 << ChunkLayer;

            leveledLod1BaseError = 1 * Lod1BaseError;
            leveledLod2BaseError = 2 * Lod2BaseError;
            leveledLod4BaseError = 4 * Lod4BaseError;
            leveledLod8BaseError = 8 * Lod8BaseError;
            leveledLod16BaseError = 16 * Lod16BaseError;
            leveledLod32BaseError = 32 * Lod32BaseError;
            leveledLod64BaseError = 64 * Lod64BaseError;

            // As terrain data is loaded asynchronously, regions must be loaded BEFORE they will be used.
            // In other words, regions must be loaded while they are still far enough to be NOT computed yet.
            regionSize = Vector3i.one * buildDistanceChunk * 2;

            maxLevelCollider = UnitConverter.LevelIndexToLevel(MaxLevelIndexCollider - 1);
            maxLevelGrass = UnitConverter.LevelIndexToLevel(MaxLevelIndexGrass - 1);
            maxLevelShadows = UnitConverter.LevelIndexToLevel(MaxLevelIndexShadows - 1);
            maxLevelDetails = UnitConverter.LevelIndexToLevel(MaxLevelIndexDetails - 1);
            maxLevelTrees = UnitConverter.LevelIndexToLevel(MaxLevelIndexTrees - 1);
            treesBuildDistanceChunk = BuildDistance * maxLevelTrees;

            if (DetailsParams != null) {
                for (var i = 0; i < DetailsParams.Length; ++i) {
                    DetailsParams[i].Init(this);
                }
            }

            if (TreesParams != null) {
                for (var i = 0; i < TreesParams.Length; ++i) {
                    TreesParams[i].Init(this);
                }
            }

            Validate();
        }

        private void Validate()
        {
            if (SizeXVoxel < 0.01f) {
                UDebug.Fatal("SizeXVoxel must be greater than or equal to 0.01");
            }

            if (SizeYVoxel < 0.01f) {
                UDebug.Fatal("SizeYVoxel must be greater than or equal to 0.01");
            }

            if (SizeZVoxel < 0.01f) {
                UDebug.Fatal("SizeZVoxel must be greater than or equal to 0.01");
            }

            if (LevelCount < 1) {
                UDebug.Fatal("LevelCount must be greater than or equal to 1");
            }

            if (LevelCount > MAX_LEVEL_COUNT) {
                UDebug.Fatal("LevelCount must be lower than or equal to " + MAX_LEVEL_COUNT);
            }

            if (PostBuildMaxElapsedTimePerFrame < 1) {
                UDebug.Fatal("PostBuildMaxElapsedTimePerFrame must be greater than or equal to 1");
            }

            if (PostBuildMaxCountPerFrame < 1) {
                UDebug.Fatal("PostBuildMaxCountPerFrame must be greater than or equal to 1");
            }

            if (TreeSpawningMaxCountPerFrame < 1) {
                UDebug.Fatal("TreeSpawningMaxCountPerFrame must be greater than or equal to 1");
            }

            if (BuildDistance < 1) {
                UDebug.Fatal("BuildDistance must be greater than or equal to 1");
            }

            if (Lod2Distance < 1) {
                UDebug.Fatal("LOD2_DISTANCE must be greater than or equal to 1");
            }

            if (Lod4Distance < 2) {
                UDebug.Fatal("LOD4_DISTANCE must be greater than or equal to 2");
            }

            if (Lod8Distance < 2) {
                UDebug.Fatal("LOD8_DISTANCE must be greater than or equal to 2");
            }

            if (Lod16Distance < 2) {
                UDebug.Fatal("LOD16_DISTANCE must be greater than or equal to 2");
            }

            if (Lod32Distance < 2) {
                UDebug.Fatal("LOD32_DISTANCE must be greater than or equal to 2");
            }

            if (Lod64Distance < 2) {
                UDebug.Fatal("LOD64_DISTANCE must be greater than or equal to 2");
            }

            if (ChunkLayer == 0) {
                UDebug.Fatal("Chunk layer must be defined");
            }

            if (maxLevelDetails > maxLevelCollider) {
                UDebug.Fatal("Max LOD with details must be lower or equal to max LOD with colliders.");
            }

            if (MaxLevelIndexTrees > MAX_LEVEL) {
                UDebug.Fatal("Max LOD with trees must be lower or equal to max LOD.");
            }

            foreach (var initialChunkCount in InitialChunkCountLevel) {
                if (initialChunkCount < 0) {
                    UDebug.Fatal("Initial chunk count per level must be greater than or equal to 0");
                }
            }


            if (TreeVerticalStep < 1) {
                UDebug.Fatal("Trees height (TreeVerticalStep) must be greater than or equal to 1.");
            }
        }

        public int BuildDistance = 4;
        public int VerticalBuildDistance = 4;
        private int buildWidth;

        public int BuildWidth {
            get { return buildWidth; }
        }

        private int verticalBuildWidth;

        public int VerticalBuildWidth {
            get { return verticalBuildWidth; }
        }

        private int chunkTreeCount;

        public int ChunkTreeCount {
            get { return chunkTreeCount; }
        }

        private int buildDistanceChunk;

        public int BuildDistanceChunk {
            get { return buildDistanceChunk; }
        }

        private int verticalBuildDistanceChunk;

        public int VerticalBuildDistanceChunk {
            get { return verticalBuildDistanceChunk; }
        }

        private int treesBuildDistanceChunk;

        public int TreesBuildDistanceChunk {
            get { return treesBuildDistanceChunk; }
        }

        public int PostBuildMaxElapsedTimePerFrame = 12;
        public int PostBuildMaxCountPerFrame = 10;
        public int TreeSpawningMaxCountPerFrame = 10;

        public bool IsTerrainInfinite = true;

        public static HideFlags HideFlags {
            get { return HideFlags.HideAndDontSave; }
        }

        //-----------------------------------------------------------------------
        // Load & Save
        private Vector3i regionSize;

        public Vector3i RegionSize {
            get { return regionSize; }
        }

        public string BasePath {
            get { return Path.Combine(Application.streamingAssetsPath, "uTerrainsData"); }
        }

        public string RuntimeBasePath {
            get { return Path.Combine(Application.persistentDataPath, "uTerrainsData"); }
        }

        /// <summary>
        ///     Subfolder in StreamingAssets where operations will be loaded from. MUST not be changed at runtime.
        /// </summary>
        public string DataPath = Guid.NewGuid().ToString();

        /// <summary>
        ///     Subfolder in persistent Unity folder where operations saved at runtime will be loaded from.
        ///     Can be change at runtime but ONLY in an Awake() method!
        /// </summary>
        public string RuntimeDataPath = Guid.NewGuid().ToString();

        public bool LoadDynamically;

        public string GetOperationsDataPath()
        {
            return Path.Combine(Path.Combine(BasePath, DataPath), "operations");
        }

        public string GetOperationsRuntimeDataPath()
        {
            return Path.Combine(Path.Combine(RuntimeBasePath, RuntimeDataPath), "runtime_operations");
        }


        //-----------------------------------------------------------------------
        // Generator
        public const int Max2DModulesCount = 28; // Chunk cache size in memory will be 17*17*8*Max2DModulesCount *2


        //-----------------------------------------------------------------------
        // Chunk

        // Max LOD (must be a power of 2)
        public int LevelCount = 1 + 2;
        private int maxLevel;

        public int MaxLevel {
            get { return maxLevel; }
        }

        private int maxLevelBits;

        public int MaxLevelBits {
            get { return maxLevelBits; }
        }

        // Geometric acceptable error for octree generation
        public double Lod1BaseError = 0.3f;
        public double Lod2BaseError = 0.3f;
        public double Lod4BaseError = 0.3f;
        public double Lod8BaseError = 0.3f;
        public double Lod16BaseError = 0.5f;
        public double Lod32BaseError = 0.5f;
        public double Lod64BaseError = 0.5f;
        private double leveledLod1BaseError;
        private double leveledLod2BaseError;
        private double leveledLod4BaseError;
        private double leveledLod8BaseError;
        private double leveledLod16BaseError;
        private double leveledLod32BaseError;
        private double leveledLod64BaseError;

        public double NearSurfacePrecision = 1.0;

        public ChunkConnectionMode ChunkConnectionMode;

        public bool HasProperlyConnectedSeams {
            get { return ChunkConnectionMode == ChunkConnectionMode.ProperlyConnectedSeams || ChunkConnectionMode == ChunkConnectionMode.ProperlyConnectedSeamsAndSkirts; }
        }

        public bool HasSkirts {
            get { return ChunkConnectionMode == ChunkConnectionMode.Skirts || ChunkConnectionMode == ChunkConnectionMode.ProperlyConnectedSeamsAndSkirts; }
        }

        // Geometric acceptable error coef. when there are sign changes in the node
        public double Lod1SignChangesErrorMultiplicator = 0;
        public double Lod2SignChangesErrorMultiplicator = 0;
        public double Lod4SignChangesErrorMultiplicator = 0;
        public double Lod8SignChangesErrorMultiplicator = 0;
        public double Lod16SignChangesErrorMultiplicator = 0;
        public double Lod32SignChangesErrorMultiplicator = 0;
        public double Lod64SignChangesErrorMultiplicator = 0;

        // Geometric acceptable error coef. on borders of chunks
        public double Lod1BorderErrorMultiplicator = 1.0;
        public double Lod2BorderErrorMultiplicator = 1.0;
        public double Lod4BorderErrorMultiplicator = 1.0;
        public double Lod8BorderErrorMultiplicator = 1.0;
        public double Lod16BorderErrorMultiplicator = 1.0;
        public double Lod32BorderErrorMultiplicator = 1.0;
        public double Lod64BorderErrorMultiplicator = 1.0;

        // Distance of LODs (this value will be multiplied by the level)
        public int Lod2Distance = 1;
        public int Lod4Distance = 2;
        public int Lod8Distance = 2;
        public int Lod16Distance = 2;
        public int Lod32Distance = 2;
        public int Lod64Distance = 2;

        public double GetBaseError(int level)
        {
            switch (level) {
                case 1:
                    return leveledLod1BaseError;
                case 2:
                    return leveledLod2BaseError;
                case 4:
                    return leveledLod4BaseError;
                case 8:
                    return leveledLod8BaseError;
                case 16:
                    return leveledLod16BaseError;
                case 32:
                    return leveledLod32BaseError;
                default:
                    return leveledLod64BaseError;
            }
        }

        public double GetSignChangesErrorMultiplicator(int level)
        {
            switch (level) {
                case 1:
                    return Lod1SignChangesErrorMultiplicator;
                case 2:
                    return Lod2SignChangesErrorMultiplicator;
                case 4:
                    return Lod4SignChangesErrorMultiplicator;
                case 8:
                    return Lod8SignChangesErrorMultiplicator;
                case 16:
                    return Lod16SignChangesErrorMultiplicator;
                case 32:
                    return Lod32SignChangesErrorMultiplicator;
                default:
                    return Lod64SignChangesErrorMultiplicator;
            }
        }

        public double GetBorderErrorMultiplicator(int level)
        {
            switch (level) {
                case 1:
                    return Lod1BorderErrorMultiplicator;
                case 2:
                    return Lod2BorderErrorMultiplicator;
                case 4:
                    return Lod4BorderErrorMultiplicator;
                case 8:
                    return Lod8BorderErrorMultiplicator;
                case 16:
                    return Lod16BorderErrorMultiplicator;
                case 32:
                    return Lod32BorderErrorMultiplicator;
                default:
                    return Lod64BorderErrorMultiplicator;
            }
        }


        //-----------------------------------------------------------------------
        // Meshes
        public bool DuplicateVertices = false;
        public bool ComputeTangentsOnMeshes = true;


        //-----------------------------------------------------------------------
        // Grass
        public int GrassLayer;
        public bool GrassCastShadows;
        public bool GrassReceiveShadows = true;

        //-----------------------------------------------------------------------
        // Details
        public DetailsParam[] DetailsParams;
        public int MaxDetailsCountPerChunkPerType = 512;
#if UNITY_EDITOR
        [SerializeField] private List<bool> detailsFoldoutForEditor;

        public List<bool> DetailsFoldoutForEditor {
            get {
                if (detailsFoldoutForEditor == null)
                    detailsFoldoutForEditor = new List<bool>();
                return detailsFoldoutForEditor;
            }
        }
#endif

        //-----------------------------------------------------------------------
        // Trees
        public TreesParam[] TreesParams;

        public string[] TreesParamsNames {
            get {
                if (TreesParams == null) {
                    return null;
                }

                var names = new string[TreesParams.Length + 1];
                names[0] = "- None -";
                for (var i = 0; i < TreesParams.Length; i++) {
                    names[i + 1] = TreesParams[i].Name;
                }

                return names;
            }
        }

        public int[] TreesParamsIndices {
            get {
                if (TreesParams == null) {
                    return null;
                }

                var indices = new int[TreesParams.Length + 1];
                indices[0] = -1;
                for (var i = 0; i < TreesParams.Length; i++) {
                    indices[i + 1] = i;
                }

                return indices;
            }
        }

#if UNITY_EDITOR
        [SerializeField] private List<bool> treesFoldoutForEditor;

        public List<bool> TreesFoldoutForEditor {
            get {
                if (treesFoldoutForEditor == null)
                    treesFoldoutForEditor = new List<bool>();
                return treesFoldoutForEditor;
            }
        }
#endif

        //-----------------------------------------------------------------------
        // Chunk
        public int ChunkLayer;
        private int chunkLayerMask;

        public int ChunkLayerMask {
            get { return chunkLayerMask; }
        }

        public const int SIZE_X_BITS = 4;
        public const int SIZE_Y_BITS = 4;
        public const int SIZE_Z_BITS = 4;
        public const int SIZE_X = 1 << SIZE_X_BITS;
        public const int SIZE_Y = 1 << SIZE_Y_BITS;
        public const int SIZE_Z = 1 << SIZE_Z_BITS;
        public const int SIZE_3D = SIZE_X * SIZE_Y * SIZE_Z;

        public const int HALF_SIZE_X = SIZE_X / 2;
        public const int HALF_SIZE_Y = SIZE_Y / 2;
        public const int HALF_SIZE_Z = SIZE_Z / 2;

        public const int CACHE_SIZE_X = SIZE_X + 1;
        public const int CACHE_SIZE_Y = SIZE_Y + 1;
        public const int CACHE_SIZE_Z = SIZE_Z + 1;

        public const int CACHE_2D_SIZE = CACHE_SIZE_X * CACHE_SIZE_Z;
        public const int CACHE_3D_SIZE = CACHE_SIZE_X * CACHE_SIZE_Y * CACHE_SIZE_Z;

        public const int SIZE_OFFSET_BITS = 0;
        public const int SIZE_OFFSET = 1 << SIZE_OFFSET_BITS;
        public const double SIZE_OFFSET_INVERSE = 1.0 / SIZE_OFFSET;
        public const int SIZE_TOTAL = 1 << (SIZE_OFFSET_BITS + SIZE_X_BITS);
        public const int SIZE_3D_TOTAL = SIZE_TOTAL * SIZE_TOTAL * SIZE_TOTAL;

        public static readonly double ChunkDiagonalSize = Math.Sqrt(SIZE_X * SIZE_X + SIZE_Y * SIZE_Y + SIZE_Z * SIZE_Z);
        public static readonly Vector3i ChunkVoxelSize = new Vector3i(SIZE_X, SIZE_Y, SIZE_Z);

        public double SizeXVoxel = 1.0;
        public double SizeYVoxel = 1.0;
        public double SizeZVoxel = 1.0;
        public float SizeXVoxelF { get; private set; }
        public float SizeYVoxelF { get; private set; }
        public float SizeZVoxelF { get; private set; }

        public double SizeXTotal { get; private set; }
        public double SizeYTotal { get; private set; }
        public double SizeZTotal { get; private set; }
        public float SizeXTotalF { get; private set; }
        public float SizeYTotalF { get; private set; }
        public float SizeZTotalF { get; private set; }

        public double DistanceBetweenVoxelsOfHighestLOD_X { get; private set; }
        public double DistanceBetweenVoxelsOfHighestLOD_Y { get; private set; }
        public double DistanceBetweenVoxelsOfHighestLOD_Z { get; private set; }
        public double DistanceBetweenVoxelsOfHighestLOD_X_Inverse { get; private set; }
        public double DistanceBetweenVoxelsOfHighestLOD_Y_Inverse { get; private set; }
        public double DistanceBetweenVoxelsOfHighestLOD_Z_Inverse { get; private set; }

        public int MaxLevelWithCache = 1;
        public int MaxLevelIndexCollider = 3;
        public int MaxLevelIndexShadows = 2;
        public int MaxLevelIndexGrass = 1;
        public int MaxLevelIndexDetails = 1;
        public int MaxLevelIndexTrees = 4;

        private int maxLevelCollider;

        public int MaxLevelCollider {
            get { return maxLevelCollider; }
        }

        private int maxLevelGrass;

        public int MaxLevelGrass {
            get { return maxLevelGrass; }
        }

        private int maxLevelShadows;

        public int MaxLevelShadows {
            get { return maxLevelShadows; }
        }

        private int maxLevelDetails;

        public int MaxLevelDetails {
            get { return maxLevelDetails; }
        }

        private int maxLevelTrees;

        public int MaxLevelTrees {
            get { return maxLevelTrees; }
        }

        public int TreeDensityStep = 20;
        public double TreeDensityStepNoiseFrequency = 0.008;
        public int TreeVerticalStep = 15;

        public readonly int[] InitialChunkCountLevel = new int[MAX_LEVEL_COUNT]
        {
            512,
            512,
            512,
            256,
            256,
            128,
            128,
            128,
            128,
            128,
            128
        };

        public int TotalInitialChunkCount {
            get {
                var c = 0;
                for (var i = 0; i < InitialChunkCountLevel.Length; ++i) {
                    c += InitialChunkCountLevel[i];
                }

                return c;
            }
        }

        public bool PreventTreesSpawning { get; set; }
    }
}