using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UltimateTerrains
{
#if CHUNK_DEBUG
    public
#endif
    internal sealed class Chunk
    {
        //===============================================================================


        //-----------------------------------------------------------------------
        // Constants and static fields

        // Used to give a unique ID to each chunk
        private static int lastId;

        //-----------------------------------------------------------------------


        //-----------------------------------------------------------------------
        // Misc

        // Chunk id can be used as a unique key. This is used to compare chunks equality and to build chunk's hashcode.
        private readonly int id;

        // This is the index of this in the parent children array
        private int childIndex;

        // Main map object
        private UltimateTerrain terrain;

        private UltimateOperationsManager operationsManager;

        private Orchestrator orchestrator;

        // Chunk object (can be null)
        private ChunkComponent chunkObject;

        // Mesh data
        private MeshData meshData;
        private MeshData grassMeshData;

        // True if this chunk needs to be built. False if mesh data has been computed and is still valid.
        private volatile bool needBuild = true;
        private volatile bool initialized;

        // True if this chunk may contain some terrain's surface.
        // If false, there is no need to compute children.
        // If false on all children, there is no need to compute itself.
        private bool mayContainSurface = true;

        // True if chunk is not null
        private volatile bool hasChunkObject;

        // Reference to the UnitConverter
        private UnitConverter converter;

        // Reference to parameters
        private Param param;

        // Details objects
        private Details details;

        //-----------------------------------------------------------------------


        //-----------------------------------------------------------------------
        // LOD

        // level of this chunk
        private int level;

        private bool isSubdivided;

        private bool IsSubdivided {
            get { return isSubdivided; }
            set {
                isSubdivided = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.isSubdivided = value;
                    }
                }
            }
        }

        // Reference to root, parent, and children chunks. Used to build the chunk tree.
        private Chunk parent;

        private Chunk Parent {
            get { return parent; }
            set {
                parent = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.parent = value;
                    }
                }
            }
        }

        private Chunk child0;
        private Chunk child1;
        private Chunk child2;
        private Chunk child3;
        private Chunk child4;
        private Chunk child5;
        private Chunk child6;
        private Chunk child7;

        private Chunk Child0 {
            get { return child0; }
            set {
                child0 = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.child0 = value;
                    }
                }
            }
        }

        private Chunk Child1 {
            get { return child1; }
            set {
                child1 = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.child1 = value;
                    }
                }
            }
        }

        private Chunk Child2 {
            get { return child2; }
            set {
                child2 = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.child2 = value;
                    }
                }
            }
        }

        private Chunk Child3 {
            get { return child3; }
            set {
                child3 = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.child3 = value;
                    }
                }
            }
        }

        private Chunk Child4 {
            get { return child4; }
            set {
                child4 = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.child4 = value;
                    }
                }
            }
        }

        private Chunk Child5 {
            get { return child5; }
            set {
                child5 = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.child5 = value;
                    }
                }
            }
        }

        private Chunk Child6 {
            get { return child6; }
            set {
                child6 = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.child6 = value;
                    }
                }
            }
        }

        private Chunk Child7 {
            get { return child7; }
            set {
                child7 = value;
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.child7 = value;
                    }
                }
            }
        }

        // Alternative bitmask is 6 bits long so between 0 and 63. This is why we only need an array of size 64.
        private readonly Chunk[] lodBordersAlternatives = new Chunk[64];

        // Booleans used to know which chunks must be generated or hidden
        private volatile bool waitingBuilding;
        private volatile bool willBeDisplayed;
        private volatile bool isBeingDisplayed;

        // use int instead of bool to be able to use Interlocked. 0 = false, 1 = true
        internal int IsEnqueuedForBuild;

        // use int instead of bool to be able to use Interlocked. 0 = false, 1 = true
        internal int IsEnqueuedForPostBuild;

        private long lastBuildId;
        private long appliedBuildId;

        private long appliedPostBuildId;


        // Position of this chunk in chunk units
        private Vector3i position;

        // Position of this chunk relative to its parent in chunk units 
        private Vector3i relativePosition;

        // Position of this chunk in voxel units
        private Vector3i worldPosition;

        // Position of the center of this chunk in chunk units
        private Vector3i centerPosition;

        private ChunkLODBorderState lodBorderState;

        private int lowerLodDistance;

        private ChunkCache cacheKeeped;
        private bool createLodTransitions;

        //-----------------------------------------------------------------------


        //-----------------------------------------------------------------------
        // Thread synchronization

        private readonly object syncId = new object();
        private readonly object syncApplyBuild = new object();

        //-----------------------------------------------------------------------


        //===============================================================================

        public int Level {
            get { return level; }
        }

        public bool IsRoot {
            get { return level == param.MaxLevel; }
        }

        public int LowerLevel {
            get { return level * 2; }
        }

        public bool HasChunkObject {
            get { return hasChunkObject; }
        }

        public int Id {
            get { return id; }
        }

        public bool ChunkObjectIsNull {
            get { return ReferenceEquals(chunkObject, null); }
        }

        public Vector3i Position {
            get { return position; }
            set {
                position = value;
                worldPosition = new Vector3i(value.x * Param.SIZE_X, value.y * Param.SIZE_Y, value.z * Param.SIZE_Z);
                centerPosition = position + Vector3i.one * (level / 2);
            }
        }

        public Vector3i WorldPosition {
            get { return worldPosition; }
        }

        public Vector3i CenterPosition {
            get { return centerPosition; }
        }

        internal MeshData MeshData {
            get { return meshData; }
        }

        internal MeshData GrassMeshData {
            get { return grassMeshData; }
        }

        internal Details Details {
            get { return details; }
        }

        public int LevelIndex {
            get { return UnitConverter.LevelToLevelIndex(level); }
        }

        private void ReplaceChildOfParent(int i, Chunk newChild)
        {
            if (newChild != null) {
                newChild.childIndex = i;
            }

            switch (i) {
                case 0:
                    Parent.Child0 = newChild;
                    return;
                case 1:
                    Parent.Child1 = newChild;
                    return;
                case 2:
                    Parent.Child2 = newChild;
                    return;
                case 3:
                    Parent.Child3 = newChild;
                    return;
                case 4:
                    Parent.Child4 = newChild;
                    return;
                case 5:
                    Parent.Child5 = newChild;
                    return;
                case 6:
                    Parent.Child6 = newChild;
                    return;
                case 7:
                    Parent.Child7 = newChild;
                    return;
            }

            // ReSharper disable once HeapView.ObjectAllocation.Evident
            throw new ArgumentOutOfRangeException(string.Format("There is no value at {0} index.", i));
        }

        private void SetAlternativeAsNewParentOfChildren(Chunk newParent)
        {
            Child0.Parent = newParent;
            Child1.Parent = newParent;
            Child2.Parent = newParent;
            Child3.Parent = newParent;
            Child4.Parent = newParent;
            Child5.Parent = newParent;
            Child6.Parent = newParent;
            Child7.Parent = newParent;
        }

        internal bool IsChunkObjectEqualTo(ChunkComponent other)
        {
            return chunkObject == other;
        }

        public bool Raycast(Ray ray, float distance, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            var obj = chunkObject;
            if (obj != null && !obj.IsFree && ReferenceEquals(obj.Chunk, this)) {
                return obj.Raycast(ray, distance, out hitPoint, out hitNormal);
            }

            hitPoint = Vector3.zero;
            hitNormal = Vector3.zero;
            return false;
        }


        //===============================================================================


        //-----------------------------------------------------------------------
        // Constructors

        /**
		 * Create Chunk
		 *
		 * @param map The map of this chunk
		 * @param column The column of this chunk
		 * @param position The position of this chunk (map coordinates)
		 */
        public Chunk()
        {
            lock (syncId)
                id = lastId++;
        }

        internal void Init(int index, UltimateTerrain terr, HashSet<Vector3i> reportBackOperationsTemporarySet, Vector3i pos, Vector3i relativePos, int lvl, Chunk parentChunk)
        {
            UProfiler.Begin("Init");
            if (initialized) {
                UDebug.LogError("Trying to initialize a chunk that as already been initialized");
            }

            if (parentChunk == null) {
                terr.OnReloadForPreview += MarkDirtyForHotReload;
            }

            childIndex = index;
            terrain = terr;
            operationsManager = terr.OperationsManager;
            orchestrator = terr.Orchestrator;
            param = terr.Params;
            level = lvl;
            createLodTransitions = param.HasProperlyConnectedSeams;
            isSubdivided = false;
            relativePosition = relativePos;
            Position = pos + relativePos;
            parent = parentChunk;
            waitingBuilding = false;
            willBeDisplayed = false;
            isBeingDisplayed = false;
            needBuild = true;

            chunkObject = null;
            hasChunkObject = false;
            appliedBuildId = ++lastBuildId; // invalidate any upcoming build
            appliedPostBuildId = 0; // invalidate any upcoming post-build

            converter = terrain.Converter;

            lowerLodDistance = converter.LevelToLowerLodDistance(level);

            child0 = child1 = child2 = child3 = child4 = child5 = child6 = child7 = null;

            if (param.LoadDynamically && level == Math.Min(param.MaxLevel, Param.REGION_LEVEL)) {
                operationsManager.LoadRegionFromBackgroundThread(
                    UnitConverter.ChunkToLeveledChunkPosition(Position, Param.REGION_LEVEL),
                    reportBackOperationsTemporarySet);
            }

            initialized = true;
            UProfiler.End();
        }

        private void Subdivide(ChunkTreePool chunkTreePool)
        {
            UProfiler.Begin("Subdivide");
            var nextLevel = level / 2;
            var chunkPool = chunkTreePool.ChunkPool;
            UProfiler.Begin("Get children from pool");
            Child0 = chunkPool.Get();
            Child1 = chunkPool.Get();
            Child2 = chunkPool.Get();
            Child3 = chunkPool.Get();
            Child4 = chunkPool.Get();
            Child5 = chunkPool.Get();
            Child6 = chunkPool.Get();
            Child7 = chunkPool.Get();
            UProfiler.End();
            Child0.Init(0, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position, Vector3i.zero, nextLevel, this);
            Child1.Init(1, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position, nextLevel * Vector3i.right, nextLevel, this);
            Child2.Init(2, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position, nextLevel * Vector3i.forward_right, nextLevel, this);
            Child3.Init(3, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position, nextLevel * Vector3i.forward, nextLevel, this);
            Child4.Init(4, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position, nextLevel * Vector3i.up, nextLevel, this);
            Child5.Init(5, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position, nextLevel * Vector3i.up_right, nextLevel, this);
            Child6.Init(6, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position, nextLevel * Vector3i.forward_right_up, nextLevel, this);
            Child7.Init(7, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position, nextLevel * Vector3i.forward_up, nextLevel, this);
            IsSubdivided = true;
            UProfiler.End();
        }


        //-----------------------------------------------------------------------
        // Misc

        public override bool Equals(object other)
        {
            if (!(other is Chunk))
                return false;
            var c = (Chunk) other;
            return id == c.id;
        }

        public bool Equals(Chunk c)
        {
            return id == c.id;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return "Chunk-#" + id + "-L" + level + "-(" + position.x + " " + position.y + " " + position.z + ")";
        }


        //-----------------------------------------------------------------------
        // World tools

        // This is called on infinite terrains only, when the player moved enough
        internal bool UpdatePositionOfRoot(ChunkTreePool chunkTreePool, ChunkGeneratorPools chunkGeneratorPools, Vector3i playerChunkPos)
        {
            var np = position;
            var dx = np.x - playerChunkPos.x;
            var dy = np.y - playerChunkPos.y;
            var dz = np.z - playerChunkPos.z;

            var doChangePosition = false;
            var buildDistanceChunk = terrain.Params.BuildDistanceChunk;
            var verticalBuildDistanceChunk = terrain.Params.VerticalBuildDistanceChunk;

            while (dx < -buildDistanceChunk || dx >= buildDistanceChunk ||
                   dy < -verticalBuildDistanceChunk || dy >= verticalBuildDistanceChunk ||
                   dz < -buildDistanceChunk || dz >= buildDistanceChunk) {
                if (dx < -buildDistanceChunk)
                    np.x += 2 * buildDistanceChunk;
                else if (dx >= buildDistanceChunk)
                    np.x -= 2 * buildDistanceChunk;

                if (dy < -verticalBuildDistanceChunk)
                    np.y += 2 * verticalBuildDistanceChunk;
                else if (dy >= verticalBuildDistanceChunk)
                    np.y -= 2 * verticalBuildDistanceChunk;

                if (dz < -buildDistanceChunk)
                    np.z += 2 * buildDistanceChunk;
                else if (dz >= buildDistanceChunk)
                    np.z -= 2 * buildDistanceChunk;

                doChangePosition = true;
                dx = np.x - playerChunkPos.x;
                dy = np.y - playerChunkPos.y;
                dz = np.z - playerChunkPos.z;
            }

            if (doChangePosition) {
                AffectNewPosition(chunkTreePool, chunkGeneratorPools, np);
            }

            return doChangePosition;
        }

        private void AffectNewPosition(ChunkTreePool chunkTreePool, ChunkGeneratorPools chunkGeneratorPools, Vector3i newPosition)
        {
            if (!IsRoot || relativePosition != Vector3i.zero) {
                UDebug.LogError("Affecting the position of a non-root chunk.");
            }

            FreeChunk(chunkTreePool, chunkGeneratorPools, false, true, false);
            Position = newPosition;
            if (param.LoadDynamically && level == Math.Min(param.MaxLevel, Param.REGION_LEVEL)) {
                operationsManager.LoadRegionFromBackgroundThread(
                    UnitConverter.ChunkToLeveledChunkPosition(Position, Param.REGION_LEVEL), 
                    chunkTreePool.ReportBackOperationsTemporarySet);
            }
        }

        // Called from ChunkTreeWorker thread
        private void FreeChunk(ChunkTreePool chunkTreePool, ChunkGeneratorPools chunkGeneratorPools, bool freeChunkItself, bool freeChildrenAndAlternatives, bool freeChunkObjectImmediately)
        {
            DeepDebug.LogChunkInfo(this, "FreeChunk");

            freeChunkItself = freeChunkItself && !IsRoot;
            initialized = initialized && !freeChunkItself;
            Position = Vector3i.zero;
            needBuild = true;
            mayContainSurface = true;
            waitingBuilding = false;
            willBeDisplayed = false;
            isBeingDisplayed = false;
            parent = null;
            FreeData(chunkGeneratorPools);
            lock (syncApplyBuild) {
                appliedBuildId = ++lastBuildId; // invalidate any upcoming build
                appliedPostBuildId = 0; // invalidate any upcoming post-build
            }

            if (hasChunkObject) {
                if (freeChunkObjectImmediately) {
                    DeepDebug.LogChunkInfo(this, "FreeChunk -> Free");
                    hasChunkObject = false;
                    ChunkObjectPool.Free(chunkObject);
                } else {
                    DeepDebug.LogChunkInfo(this, "FreeChunk -> EnqueueForFree");
                    EnqueueChunkObjectForFree();
                }
            }

            if (freeChildrenAndAlternatives) {
                if (IsSubdivided) {
                    FreeChildren(chunkTreePool, chunkGeneratorPools, freeChunkObjectImmediately);
                }

                foreach (var alternative in lodBordersAlternatives) {
                    if (alternative != null) {
                        alternative.FreeChunk(chunkTreePool, chunkGeneratorPools, true, false, freeChunkObjectImmediately);
                    }
                }
            }

            // Clear lodBordersAlternatives
            for (var i = 0; i < lodBordersAlternatives.Length; ++i) {
                lodBordersAlternatives[i] = null;
            }

            if (IsSubdivided || Child0 != null ||
                Child1 != null ||
                Child2 != null ||
                Child3 != null ||
                Child4 != null ||
                Child5 != null ||
                Child6 != null ||
                Child7 != null) {
                UDebug.LogError("[Chunk#" + Id + "] Chunk has non-null children after free");
            }

            if (freeChunkItself) {
                chunkTreePool.ChunkPool.Free(this);
            }
        }

        private void FreeChildren(ChunkTreePool chunkTreePool, ChunkGeneratorPools chunkGeneratorPools, bool freeChunkObjectImmediately)
        {
            isSubdivided = false;
            foreach (var alternative in lodBordersAlternatives) {
                if (alternative != null) {
                    alternative.ClearChildren();
                }
            }

            child0.FreeChunk(chunkTreePool, chunkGeneratorPools, true, true, freeChunkObjectImmediately);
            child1.FreeChunk(chunkTreePool, chunkGeneratorPools, true, true, freeChunkObjectImmediately);
            child2.FreeChunk(chunkTreePool, chunkGeneratorPools, true, true, freeChunkObjectImmediately);
            child3.FreeChunk(chunkTreePool, chunkGeneratorPools, true, true, freeChunkObjectImmediately);
            child4.FreeChunk(chunkTreePool, chunkGeneratorPools, true, true, freeChunkObjectImmediately);
            child5.FreeChunk(chunkTreePool, chunkGeneratorPools, true, true, freeChunkObjectImmediately);
            child6.FreeChunk(chunkTreePool, chunkGeneratorPools, true, true, freeChunkObjectImmediately);
            child7.FreeChunk(chunkTreePool, chunkGeneratorPools, true, true, freeChunkObjectImmediately);
            child0 = child1 = child2 = child3 = child4 = child5 = child6 = child7 = null;
        }

        private void ClearChildren()
        {
            isSubdivided = false;
            child0 = child1 = child2 = child3 = child4 = child5 = child6 = child7 = null;
        }

        internal void EnqueueChunkObjectForFree()
        {
            lock (syncApplyBuild) {
                if (hasChunkObject) {
                    // Invalidate chunk object (can't nullify it because we are in a thread and Unity doesn't like it)
                    hasChunkObject = false;
                    orchestrator.EnqueueChunkToFree(chunkObject);
                }
            }
        }

        public bool IsOnLodBorder()
        {
            return ComputeIsOnLodBorderFront() || ComputeIsOnLodBorderBack() || ComputeIsOnLodBorderRight() ||
                   ComputeIsOnLodBorderLeft() || ComputeIsOnLodBorderTop() || ComputeIsOnLodBorderBottom();
        }

        private bool ComputeIsOnLodBorderFront()
        {
            return !IsRoot &&
                   Parent.CenterPosition.z <= CenterPosition.z &&
                   IsOnBorder(CenterPosition.z - terrain.CurrentCameraChunkPosition.z, lowerLodDistance * LowerLevel);
        }

        private bool ComputeIsOnLodBorderBack()
        {
            return !IsRoot &&
                   Parent.CenterPosition.z > CenterPosition.z &&
                   IsOnBorder(CenterPosition.z - terrain.CurrentCameraChunkPosition.z, -lowerLodDistance * LowerLevel);
        }

        private bool ComputeIsOnLodBorderRight()
        {
            return !IsRoot &&
                   Parent.CenterPosition.x <= CenterPosition.x &&
                   IsOnBorder(CenterPosition.x - terrain.CurrentCameraChunkPosition.x, lowerLodDistance * LowerLevel);
        }

        private bool ComputeIsOnLodBorderLeft()
        {
            return !IsRoot &&
                   Parent.CenterPosition.x > CenterPosition.x &&
                   IsOnBorder(CenterPosition.x - terrain.CurrentCameraChunkPosition.x, -lowerLodDistance * LowerLevel);
        }

        private bool ComputeIsOnLodBorderTop()
        {
            return !IsRoot &&
                   Parent.CenterPosition.y <= CenterPosition.y &&
                   IsOnBorder(CenterPosition.y - terrain.CurrentCameraChunkPosition.y, lowerLodDistance * LowerLevel);
        }

        private bool ComputeIsOnLodBorderBottom()
        {
            return !IsRoot &&
                   Parent.CenterPosition.y > CenterPosition.y &&
                   IsOnBorder(CenterPosition.y - terrain.CurrentCameraChunkPosition.y, -lowerLodDistance * LowerLevel);
        }

        private bool IsOnBorder(int p, int cubeRadius)
        {
            if (level == 1) {
                return cubeRadius > 0 && cubeRadius - 1 <= p ||
                       cubeRadius < 0 && cubeRadius + 1 > p;
            }

            return cubeRadius > 0 && p + 3 * level / 2 > cubeRadius ||
                   cubeRadius < 0 && p - 3 * level / 2 < cubeRadius;
        }

        private bool IsInsideTerrainLimits()
        {
            if (terrain.GeneratorModules.HasMinX && worldPosition.x < terrain.GeneratorModules.MinX) {
                return false;
            }

            if (terrain.GeneratorModules.HasMinY && worldPosition.y < terrain.GeneratorModules.MinY) {
                return false;
            }

            if (terrain.GeneratorModules.HasMinZ && worldPosition.z < terrain.GeneratorModules.MinZ) {
                return false;
            }

            var worldPositionMax = worldPosition;
            worldPositionMax.x += Param.SIZE_X * level;
            worldPositionMax.y += Param.SIZE_Y * level;
            worldPositionMax.z += Param.SIZE_Z * level;
            if (terrain.GeneratorModules.HasMaxX && worldPositionMax.x > terrain.GeneratorModules.MaxX) {
                return false;
            }

            if (terrain.GeneratorModules.HasMaxY && worldPositionMax.y > terrain.GeneratorModules.MaxY) {
                return false;
            }

            if (terrain.GeneratorModules.HasMaxZ && worldPositionMax.z > terrain.GeneratorModules.MaxZ) {
                return false;
            }

            return true;
        }


        //-----------------------------------------------------------------------
        // State

        internal void FillStateData(ChunkState outState)
        {
            lock (syncApplyBuild) {
                outState.buildId = ++lastBuildId;
                outState.level = level;
                outState.Position = position;
                outState.lodBorderState = lodBorderState;
                outState.IsRoot = IsRoot;
            }
        }


        //-----------------------------------------------------------------------
        // Cache

        internal ChunkCache GetCacheCopy(ChunkGeneratorPools generatorPool)
        {
            ChunkCache cacheCopy = null;
            lock (syncApplyBuild) {
                if (cacheKeeped != null) {
                    cacheCopy = generatorPool.ChunkCachePool.Get();
                    cacheKeeped.CopyTo(cacheCopy, generatorPool.Float2DModulesPool);
                }
            }

            return cacheCopy;
        }

        internal void OnPostBuildDone(ChunkGeneratorPools generatorPool)
        {
            MeshData meshDataToFree;
            MeshData grassMeshDataToFree;
            Details detailsToFree;

            lock (syncApplyBuild) {
                DeepDebug.LogChunkInfoWithChunkObject(this, "OnPostBuildDone", "chunk-object");
                if (!initialized || appliedPostBuildId != appliedBuildId) {
                    DeepDebug.LogChunkInfoWithChunkObject(this, "OnPostBuildDone canceled", "chunk-object");
                    // Prevents meshData and grassMeshData to be free if they are more recent than what has been post-builded
                    return;
                }

                meshDataToFree = meshData;
                meshData = null;

                grassMeshDataToFree = grassMeshData;
                grassMeshData = null;

                detailsToFree = details;
                details = null;

                needBuild = true;
            }

            if (meshDataToFree != null) {
                meshDataToFree.Free(generatorPool, generatorPool.MeshDataListsPools, generatorPool.MeshDataPool);
            }

            if (grassMeshDataToFree != null) {
                grassMeshDataToFree.Free(generatorPool, generatorPool.GrassMeshDataListsPools, generatorPool.GrassMeshDataPool);
            }

            if (detailsToFree != null) {
                generatorPool.DetailsPool.Free(detailsToFree);
            }
        }

        private void FreeData(ChunkGeneratorPools generatorPool)
        {
            ChunkCache cacheToFree;
            MeshData meshDataToFree;
            MeshData grassMeshDataToFree;
            Details detailsToFree;

            lock (syncApplyBuild) {
                cacheToFree = cacheKeeped;
                meshDataToFree = meshData;
                grassMeshDataToFree = grassMeshData;
                detailsToFree = details;

                cacheKeeped = null;
                meshData = null;
                grassMeshData = null;
                details = null;
            }

            if (cacheToFree != null) {
                cacheToFree.ResetCache(generatorPool.Float2DModulesPool);
                generatorPool.ChunkCachePool.Free(cacheToFree);
            }

            if (meshDataToFree != null) {
                meshDataToFree.Free(generatorPool, generatorPool.MeshDataListsPools, generatorPool.MeshDataPool);
            }

            if (grassMeshDataToFree != null) {
                grassMeshDataToFree.Free(generatorPool, generatorPool.GrassMeshDataListsPools, generatorPool.GrassMeshDataPool);
            }

            if (detailsToFree != null) {
                generatorPool.DetailsPool.Free(detailsToFree);
            }
        }


        //-----------------------------------------------------------------------
        // Building

        internal bool ApplyBuildResult(ChunkGenerator.Result result, ChunkGeneratorPools generatorPool)
        {
            var needPostBuild = false;

            ChunkCache cacheToFree;
            MeshData meshDataToFree;
            MeshData grassMeshDataToFree;
            Details detailsToFree;

            lock (syncApplyBuild) {
                DeepDebug.LogChunkInfoWithChunkObject(this, "ApplyBuildResult", "chunk-object");
                if (!initialized || result.buildId < appliedBuildId) {
                    // Prevents older ChunkGenerator.Result from overriding more recent data
                    DeepDebug.LogChunkInfoWithChunkObject(this, "ApplyBuildResult canceled", "chunk-object");
                    return false;
                }

                if (result.buildId == appliedBuildId) {
                    UDebug.LogError("Trying to apply a ChunkGenerator.Result that has already been applied.");
                }

                appliedBuildId = result.buildId;

                cacheToFree = cacheKeeped;
                meshDataToFree = meshData;
                grassMeshDataToFree = grassMeshData;
                detailsToFree = details;

                cacheKeeped = null;
                meshData = null;
                grassMeshData = null;
                details = null;

                mayContainSurface = result.mayContainSurface;

                if (result.meshData != null && result.meshData.HasTriangles) {
                    cacheKeeped = result.chunkCache;
                    meshData = result.meshData;
                    grassMeshData = result.grassMeshData;
                    details = result.details;
                    needPostBuild = true;
                } else {
                    waitingBuilding = false;
                    if (result.chunkCache != null || result.meshData != null || result.grassMeshData != null || result.details != null) {
                        UDebug.LogError("Chunk generation result should be zeroed has it produced no mesh triangles");
                    }
                }
            }

            if (cacheToFree != null) {
                cacheToFree.ResetCache(generatorPool.Float2DModulesPool);
                generatorPool.ChunkCachePool.Free(cacheToFree);
            }

            if (meshDataToFree != null) {
                meshDataToFree.Free(generatorPool, generatorPool.MeshDataListsPools, generatorPool.MeshDataPool);
            }

            if (grassMeshDataToFree != null) {
                grassMeshDataToFree.Free(generatorPool, generatorPool.GrassMeshDataListsPools, generatorPool.GrassMeshDataPool);
            }

            if (detailsToFree != null) {
                generatorPool.DetailsPool.Free(detailsToFree);
            }

            return needPostBuild;
        }

        // Called on main thread
        public ChunkComponent PostBuild()
        {
            if (!initialized) {
                return null;
            }

            // Check post building ID so we don't post build deprecated mesh data
            // and we don't post built the same chunk several times in the same 
            // chunk-tree-iteration
            lock (syncApplyBuild) {
                appliedPostBuildId = appliedBuildId;
                if (meshData != null && meshData.HasTriangles) {
                    // Check again to be thread safe
                    // Get a new chunk object when needed. Basically, hasChunkObject is true if and only if 
                    // the current chunk has an attached chunkObject that is not enqueued for free.
                    if (!hasChunkObject || ChunkObjectIsNull) {
                        DeepDebug.LogChunkObjectUseInPostBuild(this, "PostBuild");
                        chunkObject = ChunkObjectPool.Use(this);
                        hasChunkObject = true;
                    }

                    chunkObject.PostBuild(false);
                    waitingBuilding = false;
                    DeepDebug.LogChunkInfoWithChunkObject(this, "PostBuild -> Do PostBuild", "Just post-builded");
                }

                return hasChunkObject ? chunkObject : null;
            }
        }

        // Called on main thread
        public bool PostBuildOrFreeImmediately()
        {
            if (!initialized) {
                return false;
            }

            var postBuildDone = false;
            lock (syncApplyBuild) {
                appliedPostBuildId = appliedBuildId;
                if (meshData != null && meshData.HasTriangles) {
                    if (isBeingDisplayed) {
                        // Chunk is currently being displayed, so as long as it references a chunk-object, we use it,
                        // even if it's enqueued for free.
                        if (!ChunkObjectIsNull && ReferenceEquals(chunkObject.Chunk, this) && !chunkObject.IsFree) {
                            chunkObject.PostBuild(isBeingDisplayed);
                            postBuildDone = true;
                            DeepDebug.LogChunkInfoWithChunkObject(this, "PostBuildImmediately -> Do PostBuild (A)", "Just post-builded");
                        } else {
                            if (waitingBuilding) {
                                UDebug.Fatal(string.Format("waitingBuilding is true but the chunk #{0} {1} isBeingDisplayed and has no chunkObject. This is incoherent.", Id, Position));
                            }

                            // There is no chunkObject attached to this chunk, but we need to render it immediately.
                            // This typically happens when the chunk is displayed but is empty (has no vertex).
                            // So we grab a new one and build it right away.
                            DeepDebug.LogChunkObjectUseInPostBuild(this, "PostBuildImmediately (A)");
                            chunkObject = ChunkObjectPool.Use(this);
                            hasChunkObject = true;
                            if (!willBeDisplayed) {
                                EnqueueChunkObjectForFree();
                            }

                            chunkObject.PostBuild(isBeingDisplayed);
                            postBuildDone = true;
                            DeepDebug.LogChunkInfoWithChunkObject(this, "PostBuildImmediately -> Do PostBuild (B)", "Just post-builded");
                        }
                    } else if (willBeDisplayed) {
                        UDebug.LogError("PostBuildOrFreeImmediately should be called only on chunks that are being displayed");
                    } else {
                        UDebug.LogError("Trying to PostBuildOrFreeImmediately a chunk that is neither beingDisplayed nor willBeDisplayed");
                    }
                } else if (hasChunkObject) {
                    hasChunkObject = false;
                    ChunkObjectPool.Free(chunkObject);
                }
            }

            return postBuildDone;
        }


        //-----------------------------------------------------------------------
        // Chunk tree

        private Chunk SelectAlternative(ChunkTreePool chunkTreePool, bool isOnLodBorderFront, bool isOnLodBorderBack, bool isOnLodBorderRight, bool isOnLodBorderLeft, bool isOnLodBorderTop, bool isOnLodBorderBottom)
        {
            UProfiler.Begin("SelectAlternative");
            var bitmask = GetLodBitmask(isOnLodBorderFront, isOnLodBorderBack, isOnLodBorderRight, isOnLodBorderLeft, isOnLodBorderTop, isOnLodBorderBottom);
            var alternative = lodBordersAlternatives[bitmask];
            if (alternative == null) {
                alternative = CreateAlternative(chunkTreePool, isOnLodBorderFront, isOnLodBorderBack, isOnLodBorderRight, isOnLodBorderLeft, isOnLodBorderTop, isOnLodBorderBottom);
                AddAlternative(alternative);
            }

            UProfiler.End();
            return alternative;
        }

        private void AddAlternative(Chunk alternative)
        {
            var bitmask = alternative.GetSelfLodBitmask();
            foreach (var alt in lodBordersAlternatives) {
                if (alt != null) {
                    alt.lodBordersAlternatives[bitmask] = alternative;
                }
            }

            lodBordersAlternatives[bitmask] = alternative;
        }

        private Chunk CreateAlternative(ChunkTreePool chunkTreePool, bool isOnLodBorderFront, bool isOnLodBorderBack, bool isOnLodBorderRight, bool isOnLodBorderLeft, bool isOnLodBorderTop, bool isOnLodBorderBottom)
        {
            UProfiler.Begin("CreateAlternative");
            var selfBitmask = GetSelfLodBitmask();

            UProfiler.Begin("Get alternative from pool");
            var alternative = chunkTreePool.ChunkPool.Get();
            UProfiler.End();
            alternative.Init(childIndex, terrain, chunkTreePool.ReportBackOperationsTemporarySet, Position - relativePosition, relativePosition, level, Parent);
            alternative.mayContainSurface = mayContainSurface;
            alternative.isSubdivided = IsSubdivided;
            if (IsSubdivided) {
                alternative.child0 = Child0;
                alternative.child1 = Child1;
                alternative.child2 = Child2;
                alternative.child3 = Child3;
                alternative.child4 = Child4;
                alternative.child5 = Child5;
                alternative.child6 = Child6;
                alternative.child7 = Child7;
            }

            alternative.lodBordersAlternatives[selfBitmask] = this;
            for (var i = 0; i < lodBordersAlternatives.Length; ++i) {
                var alt = lodBordersAlternatives[i];
                if (alt != null) {
                    alternative.lodBordersAlternatives[i] = alt;
                }
            }

            alternative.lodBorderState.isOnLodBorderFront = isOnLodBorderFront;
            alternative.lodBorderState.isOnLodBorderBack = isOnLodBorderBack;
            alternative.lodBorderState.isOnLodBorderRight = isOnLodBorderRight;
            alternative.lodBorderState.isOnLodBorderLeft = isOnLodBorderLeft;
            alternative.lodBorderState.isOnLodBorderTop = isOnLodBorderTop;
            alternative.lodBorderState.isOnLodBorderBottom = isOnLodBorderBottom;

            UProfiler.End();
            return alternative;
        }

        private void ChooseAlternativeToSetVisible(ChunkTreePool chunkTreePool, ChunkGeneratorPools chunkGeneratorPools)
        {
            var isOnLodBorderFront = ComputeIsOnLodBorderFront();
            var isOnLodBorderBack = ComputeIsOnLodBorderBack();
            var isOnLodBorderRight = ComputeIsOnLodBorderRight();
            var isOnLodBorderLeft = ComputeIsOnLodBorderLeft();
            var isOnLodBorderTop = ComputeIsOnLodBorderTop();
            var isOnLodBorderBottom = ComputeIsOnLodBorderBottom();

            var hasBorderLodChanged = isOnLodBorderFront != lodBorderState.isOnLodBorderFront ||
                                      isOnLodBorderBack != lodBorderState.isOnLodBorderBack ||
                                      isOnLodBorderRight != lodBorderState.isOnLodBorderRight ||
                                      isOnLodBorderLeft != lodBorderState.isOnLodBorderLeft ||
                                      isOnLodBorderTop != lodBorderState.isOnLodBorderTop ||
                                      isOnLodBorderBottom != lodBorderState.isOnLodBorderBottom;

            if (createLodTransitions && !IsRoot && hasBorderLodChanged /*&& (postBuildingId == 0 || (meshData != null && meshData.HasTriangles))*/) {
                var alternative = SelectAlternative(chunkTreePool, isOnLodBorderFront, isOnLodBorderBack, isOnLodBorderRight, isOnLodBorderLeft, isOnLodBorderTop, isOnLodBorderBottom);
                ReplaceChildOfParent(childIndex, alternative);
                if (IsSubdivided) {
                    SetAlternativeAsNewParentOfChildren(alternative);
                }

                alternative.SetVisible(chunkTreePool, chunkGeneratorPools);
                SetInvisible(chunkGeneratorPools, false);
            } else {
                SetVisible(chunkTreePool, chunkGeneratorPools);
            }
        }

        private void SetVisible(ChunkTreePool chunkTreePool, ChunkGeneratorPools chunkGeneratorPools)
        {
            UProfiler.Begin("SetVisible");

            // If it was already being displayed, there is nothing more to do
            if (!willBeDisplayed) {
                willBeDisplayed = true;
                DeepDebug.LogChunkInfo(this, "SetVisible -> willBeDisplayed was FALSE and is now TRUE");

                if (needBuild) {
                    if (IsSubdivided && !Child0.needBuild) {
                        mayContainSurface = Child0.mayContainSurface ||
                                            Child1.mayContainSurface ||
                                            Child2.mayContainSurface ||
                                            Child3.mayContainSurface ||
                                            Child4.mayContainSurface ||
                                            Child5.mayContainSurface ||
                                            Child6.mayContainSurface ||
                                            Child7.mayContainSurface;
                    } else if (Parent != null && !Parent.needBuild) {
                        mayContainSurface = Parent.mayContainSurface;
                    } else {
                        mayContainSurface = true;
                    }
                }

                if (needBuild && mayContainSurface) {
                    needBuild = false;
                    waitingBuilding = true;
                    if (IsInsideTerrainLimits()) {
                        DeepDebug.LogChunkInfo(this, "SetVisible -> EnqueueForGeneratingChunk");
                        orchestrator.EnqueueChunkToBuild(this);
                    } else {
                        waitingBuilding = false;
                        if (cacheKeeped != null || hasChunkObject || meshData != null) {
                            UDebug.LogError("[Chunk#" + Id + "] Chunk is outside terrain limits but it has some data");
                        }
                    }
                }

                // If this one becomes visible, all its children should be invisible
                if (IsSubdivided) {
                    Child0.SetInvisible(chunkGeneratorPools, true);
                    Child1.SetInvisible(chunkGeneratorPools, true);
                    Child2.SetInvisible(chunkGeneratorPools, true);
                    Child3.SetInvisible(chunkGeneratorPools, true);
                    Child4.SetInvisible(chunkGeneratorPools, true);
                    Child5.SetInvisible(chunkGeneratorPools, true);
                    Child6.SetInvisible(chunkGeneratorPools, true);
                    Child7.SetInvisible(chunkGeneratorPools, true);
                }
            } else if (IsSubdivided) {
                if (Child0.willBeDisplayed || Child1.willBeDisplayed || Child2.willBeDisplayed || Child3.willBeDisplayed
                    || Child4.willBeDisplayed || Child5.willBeDisplayed || Child6.willBeDisplayed || Child7.willBeDisplayed) {
                    UDebug.LogError("Chunk is already being displayed but its children too");
                }

                // Why don't we free children here? Couldn't we free them directly instead of making them invisible?
                // No, because a chunk that is currently visible and becomes invisible must not be free before the commit.
                // Otherwise, Operations won't be able to affect them while they should.
                // So, first, we make children invisible. Then, if this chunk was already visible we can be sure
                // its children were already invisible => now we can free them safely.
                FreeChildren(chunkTreePool, chunkGeneratorPools, false);
            }

            UProfiler.End();
        }

        private void SetInvisible(ChunkGeneratorPools chunkGeneratorPools, bool propagateToChildrenAndAlternatives)
        {
            UProfiler.Begin("SetInvisible");

            waitingBuilding = false;
            if (willBeDisplayed) {
                willBeDisplayed = false;
                DeepDebug.LogChunkInfo(this, "SetInvisible -> willBeDisplayed was TRUE and is now FALSE");
            }

            // the chunk object becomes free to avoid using too many objects on the scene
            if (hasChunkObject) {
                DeepDebug.LogChunkInfo(this, "SetInvisible -> EnqueueForFree");
                EnqueueChunkObjectForFree();
            }

            needBuild = true;
            FreeData(chunkGeneratorPools);

            if (propagateToChildrenAndAlternatives) {
                // Ensure cache and meshdata of alternatives become free if needed
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.SetInvisible(chunkGeneratorPools, false);
                    }
                }

                if (IsSubdivided) {
                    Child0.SetInvisible(chunkGeneratorPools, true);
                    Child1.SetInvisible(chunkGeneratorPools, true);
                    Child2.SetInvisible(chunkGeneratorPools, true);
                    Child3.SetInvisible(chunkGeneratorPools, true);
                    Child4.SetInvisible(chunkGeneratorPools, true);
                    Child5.SetInvisible(chunkGeneratorPools, true);
                    Child6.SetInvisible(chunkGeneratorPools, true);
                    Child7.SetInvisible(chunkGeneratorPools, true);
                }
            }

            UProfiler.End();
        }

        internal void GenerateChunkTree(ChunkTreePool chunkTreePool, ChunkGeneratorPools chunkGeneratorPools, int? forcedLevel)
        {
            if (level == 1 || (forcedLevel.HasValue && level == forcedLevel.Value) || !CenterPosition.IsInCubeArea(terrain.CurrentCameraChunkPosition, converter.LevelToLodDistance(level) * level)) {
                ChooseAlternativeToSetVisible(chunkTreePool, chunkGeneratorPools);
            } else {
                // This chunk won't be visible: it is above (in terms of LOD) the chunks that will be visible
                if (willBeDisplayed) {
                    willBeDisplayed = false;
                    DeepDebug.LogChunkInfo(this, "GenerateChunkTree -> willBeDisplayed was TRUE and is now FALSE");
                }

                FreeDataOfChunkAboveVisibleLod(chunkGeneratorPools);
                // Ensure cache and chunkobject of alternatives become free if needed
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null) {
                        alt.willBeDisplayed = false;
                        alt.FreeDataOfChunkAboveVisibleLod(chunkGeneratorPools);
                    }
                }

                // Compute children chunks
                if (!IsSubdivided) {
                    Subdivide(chunkTreePool);
                }

                Child0.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
                Child1.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
                Child2.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
                Child3.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
                Child4.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
                Child5.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
                Child6.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
                Child7.GenerateChunkTree(chunkTreePool, chunkGeneratorPools, forcedLevel);
            }
        }

        private void FreeDataOfChunkAboveVisibleLod(ChunkGeneratorPools chunkGeneratorPools)
        {
            waitingBuilding = false;
            needBuild = true;
            FreeData(chunkGeneratorPools);
            if (hasChunkObject) {
                DeepDebug.LogChunkInfo(this, "FreeDataOfChunkAboveVisibleLOD -> EnqueueForFree");
                EnqueueChunkObjectForFree();
            }

            // Note that we do NOT free meshdata here because we want to keep it in memory for faster build
            // if this chunk becomes visible again. Instead, meshdata is free only when its level is too low, not too high like here.
        }

        public void MarkDirty()
        {
            // Mark this chunk and its alternatives dirty (need build = true)
            needBuild = true;
            foreach (var alt in lodBordersAlternatives) {
                if (alt != null) {
                    alt.needBuild = true;
                }
            }

            // Traverse tree
            if (IsSubdivided) {
                Child0.MarkDirty();
                Child1.MarkDirty();
                Child2.MarkDirty();
                Child3.MarkDirty();
                Child4.MarkDirty();
                Child5.MarkDirty();
                Child6.MarkDirty();
                Child7.MarkDirty();
            }
        }
        
        private void MarkDirtyForHotReload(UltimateTerrain sender)
        {
            needBuild = true;
            willBeDisplayed = false;
            isBeingDisplayed = false;
            foreach (var alt in lodBordersAlternatives) {
                if (alt != null) {
                    alt.needBuild = true;
                    alt.willBeDisplayed = false;
                    alt.isBeingDisplayed = false;
                }
            }

            // Traverse tree
            if (IsSubdivided) {
                Child0.MarkDirtyForHotReload(sender);
                Child1.MarkDirtyForHotReload(sender);
                Child2.MarkDirtyForHotReload(sender);
                Child3.MarkDirtyForHotReload(sender);
                Child4.MarkDirtyForHotReload(sender);
                Child5.MarkDirtyForHotReload(sender);
                Child6.MarkDirtyForHotReload(sender);
                Child7.MarkDirtyForHotReload(sender);
            }
        }

        /// <summary>
        ///     Executed on main thread.
        /// </summary>
        public void UpdateTreeVisibility()
        {
            if (isBeingDisplayed != willBeDisplayed) {
                DeepDebug.LogChunkInfo(this, "UpdateTreeVisibility");
            }

            isBeingDisplayed = willBeDisplayed;
            foreach (var alt in lodBordersAlternatives) {
                if (alt != null) {
                    alt.isBeingDisplayed = alt.willBeDisplayed;
                }
            }

            if (IsSubdivided) {
                Child0.UpdateTreeVisibility();
                Child1.UpdateTreeVisibility();
                Child2.UpdateTreeVisibility();
                Child3.UpdateTreeVisibility();
                Child4.UpdateTreeVisibility();
                Child5.UpdateTreeVisibility();
                Child6.UpdateTreeVisibility();
                Child7.UpdateTreeVisibility();
            }
        }


        public Chunk TraverseVisibleTree(Vector3i pos, bool notVisibleButWillBeVisibleOnly)
        {
            if (notVisibleButWillBeVisibleOnly) {
                // First, check this chunk: if it isn't visible but will be, returns it.
                if (!isBeingDisplayed && willBeDisplayed) {
                    return this;
                }

                // Second, check alternatives: if it isn't visible but will be, returns it.
                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null && !alt.isBeingDisplayed && alt.willBeDisplayed) {
                        return alt;
                    }
                }
            } else {
                if (isBeingDisplayed) {
                    return this;
                }

                foreach (var alt in lodBordersAlternatives) {
                    if (alt != null && alt.isBeingDisplayed) {
                        return alt;
                    }
                }
            }

            if (!IsSubdivided) {
                return null;
            }

            // Traverse tree
            if (pos.x >= CenterPosition.x) {
                if (pos.y >= CenterPosition.y) {
                    if (pos.z >= CenterPosition.z) {
                        return Child6.TraverseVisibleTree(pos, notVisibleButWillBeVisibleOnly);
                    }

                    return Child5.TraverseVisibleTree(pos, notVisibleButWillBeVisibleOnly);
                }

                if (pos.z >= CenterPosition.z) {
                    return Child2.TraverseVisibleTree(pos, notVisibleButWillBeVisibleOnly);
                }

                return Child1.TraverseVisibleTree(pos, notVisibleButWillBeVisibleOnly);
            }

            if (pos.y >= CenterPosition.y) {
                if (pos.z >= CenterPosition.z) {
                    return Child7.TraverseVisibleTree(pos, notVisibleButWillBeVisibleOnly);
                }

                return Child4.TraverseVisibleTree(pos, notVisibleButWillBeVisibleOnly);
            }

            if (pos.z >= CenterPosition.z) {
                return Child3.TraverseVisibleTree(pos, notVisibleButWillBeVisibleOnly);
            }

            return Child0.TraverseVisibleTree(pos, notVisibleButWillBeVisibleOnly);
        }

        public Chunk TraverseTree(Vector3i pos, int lvl)
        {
            // This is a leaf or this is a traversed node
            if (!IsSubdivided || level == lvl) {
                return this;
            }

            // Traverse tree
            if (pos.x >= CenterPosition.x) {
                if (pos.y >= CenterPosition.y) {
                    if (pos.z >= CenterPosition.z) {
                        return Child6.TraverseTree(pos, lvl);
                    }

                    return Child5.TraverseTree(pos, lvl);
                }

                if (pos.z >= CenterPosition.z) {
                    return Child2.TraverseTree(pos, lvl);
                }

                return Child1.TraverseTree(pos, lvl);
            }

            if (pos.y >= CenterPosition.y) {
                if (pos.z >= CenterPosition.z) {
                    return Child7.TraverseTree(pos, lvl);
                }

                return Child4.TraverseTree(pos, lvl);
            }

            if (pos.z >= CenterPosition.z) {
                return Child3.TraverseTree(pos, lvl);
            }

            return Child0.TraverseTree(pos, lvl);
        }


        private int GetSelfLodBitmask()
        {
            return GetLodBitmask(
                lodBorderState.isOnLodBorderFront,
                lodBorderState.isOnLodBorderBack,
                lodBorderState.isOnLodBorderRight,
                lodBorderState.isOnLodBorderLeft,
                lodBorderState.isOnLodBorderTop,
                lodBorderState.isOnLodBorderBottom
            );
        }

        private static int GetLodBitmask(bool lodBorderFront, bool lodBorderBack, bool lodBorderRight, bool lodBorderLeft, bool lodBorderTop, bool lodBorderBottom)
        {
            return GetBitmask(lodBorderFront, lodBorderBack, lodBorderRight, lodBorderLeft, lodBorderTop, lodBorderBottom, false, false, false);
        }

        private static int GetBitmask(bool a, bool b, bool c, bool d, bool e, bool f, bool g, bool h, bool i)
        {
            return (a ? 1 << 0 : 0) | (b ? 1 << 1 : 0) | (c ? 1 << 2 : 0) |
                   (d ? 1 << 3 : 0) | (e ? 1 << 4 : 0) | (f ? 1 << 5 : 0) |
                   (g ? 1 << 6 : 0) | (h ? 1 << 7 : 0) | (i ? 1 << 8 : 0);
        }


        //-----------------------------------------------------------------------
        // UDebug
        public static class DeepDebug
        {
            [Conditional("CHUNK_DEBUG")]
            internal static void LogChunkInfo(Chunk chunk, string prefix)
            {
                UDebug.Log(prefix + "#" + chunk.Id +
                           " hasChunkObject = " + chunk.hasChunkObject +
                           " isBeingDisplayed = " + chunk.isBeingDisplayed +
                           " willBeDisplayed = " + chunk.willBeDisplayed +
                           " lastBuildId = " + chunk.lastBuildId +
                           " appliedBuildId = " + chunk.appliedBuildId +
                           " appliedPostBuildId = " + chunk.appliedPostBuildId);
            }

            [Conditional("CHUNK_DEBUG")]
            internal static void LogChunkInfoWithChunkObject(Chunk chunk, string prefix, string prefixObj)
            {
                if (chunk.ChunkObjectIsNull) {
                    LogChunkInfo(chunk, prefix);
                } else {
                    UDebug.Log(prefix + "#" + chunk.Id +
                               prefixObj + " chunkObject#" + chunk.chunkObject.Id +
                               " chunkObject.Chunk = " + (chunk.chunkObject.Chunk != null ? chunk.chunkObject.Chunk.Id.ToString() : "null") +
                               " chunkObject.IsFree = " + chunk.chunkObject.IsFree +
                               " hasChunkObject = " + chunk.hasChunkObject +
                               " isBeingDisplayed = " + chunk.isBeingDisplayed +
                               " willBeDisplayed = " + chunk.willBeDisplayed +
                               " lastBuildId = " + chunk.lastBuildId +
                               " appliedBuildId = " + chunk.appliedBuildId +
                               " appliedPostBuildId = " + chunk.appliedPostBuildId);
                }
            }

            [Conditional("CHUNK_DEBUG")]
            internal static void LogChunkInfoWithChunkObjectERROR(Chunk chunk, string prefix, string prefixObj)
            {
                UDebug.LogError(prefix + "#" + chunk.Id +
                                prefixObj + " chunkObject#" + chunk.chunkObject.Id +
                                " chunkObject.Chunk = " + (chunk.chunkObject.Chunk != null ? chunk.chunkObject.Chunk.Id.ToString() : "null") +
                                " chunkObject.IsFree = " + chunk.chunkObject.IsFree +
                                " hasChunkObject = " + chunk.hasChunkObject +
                                " isBeingDisplayed = " + chunk.isBeingDisplayed +
                                " willBeDisplayed = " + chunk.willBeDisplayed +
                                " lastBuildId = " + chunk.lastBuildId +
                                " appliedBuildId = " + chunk.appliedBuildId +
                                " appliedPostBuildId = " + chunk.appliedPostBuildId);
            }

            [Conditional("CHUNK_DEBUG")]
            internal static void LogChunkObjectUseInPostBuild(Chunk chunk, string prefix)
            {
                if (!chunk.ChunkObjectIsNull) {
                    LogChunkInfoWithChunkObject(chunk, prefix + " -> Use", "There's already a");
                }
            }

            [Conditional("CHUNK_DEBUG")]
            public static void CheckTreeVisibility(Chunk chunk, Chunk above = null, bool aboveIsBeingDisplayed = false)
            {
                if (aboveIsBeingDisplayed && chunk.isBeingDisplayed) {
                    UDebug.LogError("CheckTreeVisibility#" + chunk.Id +
                                    " isBeingDisplayed = " + chunk.isBeingDisplayed +
                                    " willBeDisplayed = " + chunk.willBeDisplayed +
                                    " ABOVE IS ALREADY DISPLAYED id#" + (above != null ? above.Id : -1) +
                                    " ABOVE isBeingDisplayed = " + (above != null && above.isBeingDisplayed) +
                                    " ABOVE willBeDisplayed = " + (above != null && above.willBeDisplayed));
                }

                if (!ReferenceEquals(above, chunk.parent)) {
                    UDebug.LogError("CheckTreeVisibility#" + chunk.Id + " BAD PARENT. Is #" + chunk.parent.Id + " instead of #" + (above != null ? above.Id : -1));
                }

                if (chunk.IsSubdivided) {
                    CheckTreeVisibility(chunk.Child0, chunk, chunk.isBeingDisplayed);
                    CheckTreeVisibility(chunk.Child1, chunk, chunk.isBeingDisplayed);
                    CheckTreeVisibility(chunk.Child2, chunk, chunk.isBeingDisplayed);
                    CheckTreeVisibility(chunk.Child3, chunk, chunk.isBeingDisplayed);
                    CheckTreeVisibility(chunk.Child4, chunk, chunk.isBeingDisplayed);
                    CheckTreeVisibility(chunk.Child5, chunk, chunk.isBeingDisplayed);
                    CheckTreeVisibility(chunk.Child6, chunk, chunk.isBeingDisplayed);
                    CheckTreeVisibility(chunk.Child7, chunk, chunk.isBeingDisplayed);
                }
            }
        }

        //-----------------------------------------------------------------------
    }
}