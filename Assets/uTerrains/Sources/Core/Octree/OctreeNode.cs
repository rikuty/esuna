using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    /// <summary>
    ///     This is octree node containing voxel information at its level.
    /// </summary>
    internal sealed class OctreeNode
    {
        // Relative position array used for interpolation
        private static readonly Vector3d[] InterpArray =
        {
            new Vector3d(0.5, 0.0, 0.0),
            new Vector3d(0.0, 0.0, 0.5),
            new Vector3d(0.5, 0.0, 0.5),
            new Vector3d(1.0, 0.0, 0.5),
            new Vector3d(0.5, 0.0, 1.0),

            new Vector3d(0.0, 0.5, 0.0),
            new Vector3d(0.5, 0.5, 0.0),
            new Vector3d(1.0, 0.5, 0.0),
            new Vector3d(0.0, 0.5, 0.5),
            new Vector3d(0.5, 0.5, 0.5),
            new Vector3d(1.0, 0.5, 0.5),
            new Vector3d(0.0, 0.5, 1.0),
            new Vector3d(0.5, 0.5, 1.0),
            new Vector3d(1.0, 0.5, 1.0),

            new Vector3d(0.5, 1.0, 0.0),
            new Vector3d(0.0, 1.0, 0.5),
            new Vector3d(0.5, 1.0, 0.5),
            new Vector3d(1.0, 1.0, 0.5),
            new Vector3d(0.5, 1.0, 1.0)
        };

        // Level in the tree. Lowest level is 1. Level is divided by 2 between a child and its parent.
        private int level;

        // Root node
        private OctreeNode root;

        private ChunkState chunkState;
        private ChunkLODBorderState chunkLODBorderState;
        private Param param;
        private VoxelTypeSet voxelTypeSet;

        private UnitConverter unitConverter;

        // Pools to be used by this octree
        private ChunkGeneratorPools chunkGeneratorPools;

        // The 8 children
        private OctreeNode child0;
        private OctreeNode child1;
        private OctreeNode child2;
        private OctreeNode child3;
        private OctreeNode child4;
        private OctreeNode child5;
        private OctreeNode child6;
        private OctreeNode child7;

        // From local position (lower left corner)
        private Vector3i from;

        // To local position (upper right corner)
        private Vector3i to;

        // (to - from) / 2
        private Vector3i halfSize;

        // True if it has some children. False if this is a leaf.
        private bool isSubdivided;

        // True if it is the root node.
        private bool isRoot;

        private bool isChunkLevelOfLowestLOD;

        // Data containing all virtual vertices
        private readonly OctreeNodeVertexData vData;

        // Mesh computed from data of this octree
        private MeshData mesh;

        // List of operations that affect the chunk of this octree
        private OperationList operations;

        // Operations count when octree root generation starts (this prevents incoherent generation
        // in case an operation is added during generation)
        private int operationsCount;

        private bool createProperlyConnectedSeams;

        private bool hasNodesNearSurface;


        // -------------------------------------------------------------------------------------------
        // Voxel Data
        public Voxel CenterBackBottomVoxel;
        public Voxel CenterLeftBottomVoxel;
        public Voxel CenterBottomVoxel;
        public Voxel CenterRightBottomVoxel;
        public Voxel CenterFrontBottomVoxel;

        public Voxel CenterBackLeftVoxel;
        public Voxel CenterBackVoxel;
        public Voxel CenterBackRightVoxel;
        public Voxel CenterLeftVoxel;
        public Voxel CenterVoxel;
        public Voxel CenterRightVoxel;
        public Voxel CenterFrontLeftVoxel;
        public Voxel CenterFrontVoxel;
        public Voxel CenterFrontRightVoxel;

        public Voxel CenterBackTopVoxel;
        public Voxel CenterLeftTopVoxel;
        public Voxel CenterTopVoxel;
        public Voxel CenterRightTopVoxel;
        public Voxel CenterFrontTopVoxel;

        public Voxel Corner0Voxel;
        public Voxel Corner1Voxel;
        public Voxel Corner2Voxel;
        public Voxel Corner3Voxel;
        public Voxel Corner4Voxel;
        public Voxel Corner5Voxel;
        public Voxel Corner6Voxel;

        public Voxel Corner7Voxel;
        // -------------------------------------------------------------------------------------------

        public bool HasBorderYContourForEdgeXBottom,
                    HasBorderYContourForEdgeXTop,
                    HasBorderZContourForEdgeXBack,
                    HasBorderZContourForEdgeXFront,
                    HasBorderXContourForEdgeYLeft,
                    HasBorderXContourForEdgeYRight,
                    HasBorderZContourForEdgeYBack,
                    HasBorderZContourForEdgeYFront,
                    HasBorderXContourForEdgeZLeft,
                    HasBorderXContourForEdgeZRight,
                    HasBorderYContourForEdgeZBottom,
                    HasBorderYContourForEdgeZTop;

        public readonly bool[] HasCornerOfEdge = new bool[12];

        // -------------------------------------------------------------------------------------------

        public int Level {
            get { return level; }
        }

        public bool IsChunkLevelOfLowestLOD {
            get { return isChunkLevelOfLowestLOD; }
        }

        public Vector3i From {
            get { return from; }
        }

        public Vector3i To {
            get { return to; }
        }

        public bool IsSubdivided {
            get { return isSubdivided; }
        }

        public bool HasNodesNearSurface {
            get { return hasNodesNearSurface; }
        }

        public OctreeNodeVertexData VertexData {
            get { return vData; }
        }

        public MeshData Mesh {
            get { return mesh; }
        }

        public OctreeNode Child0 {
            get { return child0; }
        }

        public OctreeNode Child1 {
            get { return child1; }
        }

        public OctreeNode Child2 {
            get { return child2; }
        }

        public OctreeNode Child3 {
            get { return child3; }
        }

        public OctreeNode Child4 {
            get { return child4; }
        }

        public OctreeNode Child5 {
            get { return child5; }
        }

        public OctreeNode Child6 {
            get { return child6; }
        }

        public OctreeNode Child7 {
            get { return child7; }
        }

        public int QEFi {
            get { return vData.QEFi; }
        }

        public int QEFiBack {
            get { return vData.QEFiBack; }
        }

        public int QEFiFront {
            get { return vData.QEFiFront; }
        }

        public int QEFiLeft {
            get { return vData.QEFiLeft; }
        }

        public int QEFiRight {
            get { return vData.QEFiRight; }
        }

        public int QEFiTop {
            get { return vData.QEFiTop; }
        }

        public int QEFiBottom {
            get { return vData.QEFiBottom; }
        }

        public int[] EdgesVerticesIndexes {
            get { return vData.EdgesVerticesIndexes; }
        }

        public int[] CornersVerticesIndexes {
            get { return vData.CornersVerticesIndexes; }
        }

        public int DupQEFi {
            get { return vData.DuplicatedQEFi; }
        }

        public int DupQEFiBack {
            get { return vData.DuplicatedQEFiBack; }
        }

        public int DupQEFiFront {
            get { return vData.DuplicatedQEFiFront; }
        }

        public int DupQEFiLeft {
            get { return vData.DuplicatedQEFiLeft; }
        }

        public int DupQEFiRight {
            get { return vData.DuplicatedQEFiRight; }
        }

        public int DupQEFiTop {
            get { return vData.DuplicatedQEFiTop; }
        }

        public int DupQEFiBottom {
            get { return vData.DuplicatedQEFiBottom; }
        }

        public int DupEdgesVerticesIndexes(int edge)
        {
            return vData.DuplicatedEdgesVerticesIndexes(edge);
        }

        public int DupCornersVerticesIndexes(int corners)
        {
            return vData.DuplicatedCornersVerticesIndexes(corners);
        }

        public bool HasMiddleEdgeVertexBackX {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderBack &&
                       IsBorderBack && From.x == Param.HALF_SIZE_X - 1;
            }
        }

        public bool HasMiddleEdgeVertexBackY {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderBack &&
                       IsBorderBack && From.y == Param.HALF_SIZE_Y - 1;
            }
        }

        public bool HasMiddleEdgeVertexFrontX {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderFront &&
                       IsBorderFront && From.x == Param.HALF_SIZE_X - 1;
            }
        }

        public bool HasMiddleEdgeVertexFrontY {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderFront &&
                       IsBorderFront && From.y == Param.HALF_SIZE_Y - 1;
            }
        }

        public bool HasMiddleEdgeVertexLeftZ {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderLeft &&
                       IsBorderLeft && From.z == Param.HALF_SIZE_Z - 1;
            }
        }

        public bool HasMiddleEdgeVertexLeftY {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderLeft &&
                       IsBorderLeft && From.y == Param.HALF_SIZE_Y - 1;
            }
        }

        public bool HasMiddleEdgeVertexRightZ {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderRight &&
                       IsBorderRight && From.z == Param.HALF_SIZE_Z - 1;
            }
        }

        public bool HasMiddleEdgeVertexRightY {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderRight &&
                       IsBorderRight && From.y == Param.HALF_SIZE_Y - 1;
            }
        }

        public bool HasMiddleEdgeVertexBottomX {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderBottom &&
                       IsBorderBottom && From.x == Param.HALF_SIZE_X - 1;
            }
        }

        public bool HasMiddleEdgeVertexBottomZ {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderBottom &&
                       IsBorderBottom && From.z == Param.HALF_SIZE_Z - 1;
            }
        }

        public bool HasMiddleEdgeVertexTopX {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderTop &&
                       IsBorderTop && From.x == Param.HALF_SIZE_X - 1;
            }
        }

        public bool HasMiddleEdgeVertexTopZ {
            get {
                return !IsChunkLevelOfLowestLOD && !chunkLODBorderState.isOnLodBorderTop &&
                       IsBorderTop && From.z == Param.HALF_SIZE_Z - 1;
            }
        }

        public bool HasVertices {
            get { return mesh.VertexCount > 0; }
        }

        private bool AreThereCornersNearSurface {
            get {
                var dist = param.NearSurfacePrecision * chunkState.level * level;
                return Corner0Voxel.IsOnSurface(dist) || Corner1Voxel.IsOnSurface(dist) || Corner2Voxel.IsOnSurface(dist) || Corner3Voxel.IsOnSurface(dist) || Corner4Voxel.IsOnSurface(dist) || Corner5Voxel.IsOnSurface(dist) || Corner6Voxel.IsOnSurface(dist) || Corner7Voxel.IsOnSurface(dist);
            }
        }

        /// <summary>
        ///     Construct an OctreeData node.
        /// </summary>
        public OctreeNode()
        {
            vData = new OctreeNodeVertexData(this);
        }

        /// <summary>
        ///     Pre-init the OctreeData node.
        /// </summary>
        internal void PreInitRoot(ChunkState chunkState, UnitConverter unitConverter, Param param, VoxelTypeSet voxelTypeSet, ChunkGeneratorPools pools, VoxelGenerator generator, ChunkCache cache, OctreeNode rootNode)
        {
            if (child0 != null) {
                UDebug.LogError("OctreeNode has children in PreInitRoot");
            }

            child0 = null;
            child1 = null;
            child2 = null;
            child3 = null;
            child4 = null;
            child5 = null;
            child6 = null;
            child7 = null;
            HasBorderYContourForEdgeXBottom = HasBorderYContourForEdgeXTop = HasBorderZContourForEdgeXBack = HasBorderZContourForEdgeXFront =
                HasBorderXContourForEdgeYLeft = HasBorderXContourForEdgeYRight = HasBorderZContourForEdgeYBack = HasBorderZContourForEdgeYFront =
                    HasBorderXContourForEdgeZLeft = HasBorderXContourForEdgeZRight = HasBorderYContourForEdgeZBottom = HasBorderYContourForEdgeZTop = false;
            for (var i = 0; i < HasCornerOfEdge.Length; ++i) {
                HasCornerOfEdge[i] = false;
            }

            isSubdivided = false;
            hasNodesNearSurface = false;
            level = Param.SIZE_TOTAL;
            this.param = param;
            this.voxelTypeSet = voxelTypeSet;
            this.unitConverter = unitConverter;
            this.chunkState = chunkState;
            chunkLODBorderState = chunkState.lodBorderState;
            isChunkLevelOfLowestLOD = chunkState.level == 1;
            chunkGeneratorPools = pools;
            root = rootNode;
            isRoot = true;
            from = Vector3i.zero;
            to = from + Param.SIZE_TOTAL * Vector3i.forward_right_up;
            operations = generator.OperationManager.GetOperations(chunkState.Position, chunkState.GetLevelIndex());
            if (operations != null) {
                operationsCount = operations.Count;
            } else {
                operationsCount = 0;
            }

            generator.Prepare(chunkState, operations, operationsCount, cache);
            mesh = pools.MeshDataPool.Get();
            createProperlyConnectedSeams = param.HasProperlyConnectedSeams;

            vData.PreInit(mesh, generator, unitConverter, param, voxelTypeSet, chunkState, pools.VertexDataPool);

            // Compute half size = (to - from)/2
            halfSize = to - from;
            halfSize.x = halfSize.x >> 1;
            halfSize.y = halfSize.y >> 1;
            halfSize.z = halfSize.z >> 1;

            // Compute sizeMagnitudeSquared = (to - from).magnitudeSquared

            // Get voxel data at the center
            generator.PrepareNode(Level >= Param.SIZE_OFFSET);
            CenterVoxel = generator.GenerateNoGradient(Center);
        }

        private void PreInit(int nodeLevel, Vector3i nodeFrom, OctreeNode rootNode, VoxelGenerator generator)
        {
            if (child0 != null) {
                UDebug.LogError("OctreeNode has children in PreInit");
            }

            child0 = null;
            child1 = null;
            child2 = null;
            child3 = null;
            child4 = null;
            child5 = null;
            child6 = null;
            child7 = null;
            HasBorderYContourForEdgeXBottom = HasBorderYContourForEdgeXTop = HasBorderZContourForEdgeXBack = HasBorderZContourForEdgeXFront =
                HasBorderXContourForEdgeYLeft = HasBorderXContourForEdgeYRight = HasBorderZContourForEdgeYBack = HasBorderZContourForEdgeYFront =
                    HasBorderXContourForEdgeZLeft = HasBorderXContourForEdgeZRight = HasBorderYContourForEdgeZBottom = HasBorderYContourForEdgeZTop = false;
            for (var i = 0; i < 12; ++i) {
                HasCornerOfEdge[i] = false;
            }

            isSubdivided = false;
            hasNodesNearSurface = false;
            level = nodeLevel;
            isChunkLevelOfLowestLOD = rootNode.isChunkLevelOfLowestLOD;
            chunkState = rootNode.chunkState;
            chunkLODBorderState = rootNode.chunkLODBorderState;
            param = rootNode.param;
            unitConverter = rootNode.unitConverter;
            chunkGeneratorPools = rootNode.chunkGeneratorPools;
            root = rootNode;
            isRoot = nodeLevel == Param.SIZE_TOTAL;
            from = nodeFrom;
            to = nodeFrom + nodeLevel * Vector3i.forward_right_up;
            operations = rootNode.operations;
            operationsCount = rootNode.operationsCount;
            mesh = rootNode.mesh;
            createProperlyConnectedSeams = param.HasProperlyConnectedSeams;
            vData.PreInit(mesh, generator, unitConverter, param, root.voxelTypeSet, chunkState, chunkGeneratorPools.VertexDataPool);

            // Compute half size = (to - from)/2
            halfSize = to - from;
            halfSize.x = halfSize.x >> 1;
            halfSize.y = halfSize.y >> 1;
            halfSize.z = halfSize.z >> 1;

            // Compute sizeMagnitudeSquared = (to - from).magnitudeSquared

            // Get voxel data at the center
            generator.PrepareNode(Level >= Param.SIZE_OFFSET);
            CenterVoxel = generator.GenerateNoGradient(Center);
        }

        internal static void BuildTree(OctreeNode rootNode, UnsafeStackPool<OctreeNode> stackPool, VoxelGenerator generator, ChunkBuilder chunkBuilder)
        {
            OctreePopulator.Populate(rootNode, generator, 0, null, null, null, null, null, null, null, null);

            var nodes = stackPool.Get();
            nodes.Push(rootNode);
            while (nodes.Count > 0) {
                var node = nodes.Pop();
                // Test if we create children or not
                if (node.NeedSplit()) {
                    node.DoSplit(nodes, generator);
                } else {
                    node.vData.ComputeVertices(chunkBuilder);
                }
            }

            stackPool.Free(nodes);
        }

        private void DoSplit(Stack<OctreeNode> nodes, VoxelGenerator generator)
        {
            if (child0 != null) {
                UDebug.LogError("OctreeNode already has children before DoSplit");
            }

            var octreeNodePool = chunkGeneratorPools.OctreeNodePool;
            isSubdivided = true;

            // Next level = level/2
            var nextLevel = level >> 1;

            // child 0
            child0 = octreeNodePool.Get();
            child0.PreInit(nextLevel, from, root, generator);
            OctreePopulator.Populate(child0, generator, 0, this, null, null, null, null, null, null, null);

            // child 1
            child1 = octreeNodePool.Get();
            child1.PreInit(nextLevel, from + nextLevel * Vector3i.right, root, generator);
            OctreePopulator.Populate(child1, generator, 1, this, child0, null, null, null, null, null, null);

            // child 2
            child2 = octreeNodePool.Get();
            child2.PreInit(nextLevel, from + nextLevel * Vector3i.forward_right, root, generator);
            OctreePopulator.Populate(child2, generator, 2, this, child0, child1, null, null, null, null, null);

            // child 3
            child3 = octreeNodePool.Get();
            child3.PreInit(nextLevel, from + nextLevel * Vector3i.forward, root, generator);
            OctreePopulator.Populate(child3, generator, 3, this, child0, child1, child2, null, null, null, null);

            // child 4
            child4 = octreeNodePool.Get();
            child4.PreInit(nextLevel, from + nextLevel * Vector3i.up, root, generator);
            OctreePopulator.Populate(child4, generator, 4, this, child0, child1, child2, child3, null, null, null);

            // child 5
            child5 = octreeNodePool.Get();
            child5.PreInit(nextLevel, from + nextLevel * Vector3i.up_right, root, generator);
            OctreePopulator.Populate(child5, generator, 5, this, child0, child1, child2, child3, child4, null, null);

            // child 6
            child6 = octreeNodePool.Get();
            child6.PreInit(nextLevel, from + nextLevel * Vector3i.forward_right_up, root, generator);
            OctreePopulator.Populate(child6, generator, 6, this, child0, child1, child2, child3, child4, child5, null);

            // child 7
            child7 = octreeNodePool.Get();
            child7.PreInit(nextLevel, from + nextLevel * Vector3i.forward_up, root, generator);
            OctreePopulator.Populate(child7, generator, 7, this, child0, child1, child2, child3, child4, child5, child6);

            nodes.Push(child0);
            nodes.Push(child1);
            nodes.Push(child2);
            nodes.Push(child3);
            nodes.Push(child4);
            nodes.Push(child5);
            nodes.Push(child6);
            nodes.Push(child7);
        }

        /// <summary>
        ///     Determines if node should be splitted.
        /// </summary>
        /// <return>
        ///     True if children must be created. False otherwise.
        /// </return>
        private bool NeedSplit()
        {
            // We have a highest resolution.
            if (level == Param.SIZE_OFFSET) {
                if (!root.hasNodesNearSurface && AreThereCornersNearSurface) {
                    root.hasNodesNearSurface = true;
                }

                return false;
            }

            // Do NOT spit if node is on border of higher LOD and level is 2
            if (level == Param.SIZE_OFFSET * 2 && !chunkState.IsRoot &&
                (IsBorderLeft && chunkLODBorderState.isOnLodBorderLeft ||
                 IsBorderRight && chunkLODBorderState.isOnLodBorderRight ||
                 IsBorderBottom && chunkLODBorderState.isOnLodBorderBottom ||
                 IsBorderTop && chunkLODBorderState.isOnLodBorderTop ||
                 IsBorderBack && chunkLODBorderState.isOnLodBorderBack ||
                 IsBorderFront && chunkLODBorderState.isOnLodBorderFront)) {
                return false;
            }

            var isVoxel0Inside = Corner0Voxel.IsInside;
            var signChangeOnCorners = isVoxel0Inside != Corner1Voxel.IsInside ||
                                      isVoxel0Inside != Corner2Voxel.IsInside ||
                                      isVoxel0Inside != Corner3Voxel.IsInside ||
                                      isVoxel0Inside != Corner4Voxel.IsInside ||
                                      isVoxel0Inside != Corner5Voxel.IsInside ||
                                      isVoxel0Inside != Corner6Voxel.IsInside ||
                                      isVoxel0Inside != Corner7Voxel.IsInside;
            var signChangeOnCenters = false;
            for (var i = 0; i < 19; ++i) {
                if (isVoxel0Inside != this[i].IsInside) {
                    signChangeOnCenters = true;
                    break;
                }
            }

            // Split if voxels on corners have the same sign but voxels in centers have a different sign.
            // If we didn't split that would mean we could miss some surface!
            if (!signChangeOnCorners && signChangeOnCenters) {
                return true;
            }

            // Initialize max geometric error
            var maxError = param.GetBaseError(chunkState.level);

            // Spit if there is a sign change and node is on a border
            if (isRoot || IsBorderTop || IsBorderRight || IsBorderLeft || IsBorderFront || IsBorderBottom || IsBorderBack) {
                if (signChangeOnCorners && (createProperlyConnectedSeams || isRoot) || AreThereCornersNearSurface) {
                    return true;
                }

                maxError *= param.GetBorderErrorMultiplicator(chunkState.level);
            }

            // Force split if we are affected by an action
            bool isCompletelyInsideOperation;
            if (operations != null &&
                operationsCount > 0 &&
                UltimateOperationsManager.IsAffectedByOperation(operations, this, operationsCount, chunkState.WorldPosition, chunkState.level, out isCompletelyInsideOperation)) {
                if (!isCompletelyInsideOperation) {
                    return true;
                }

                //maxError *= Param.INSIDE_ACTION_ERROR_MULTIPLICATEUR;
            }

            // Apply coef on nodes with sign change
            if (signChangeOnCorners) {
                maxError *= param.GetSignChangesErrorMultiplicator(chunkState.level);
            }

            // Error metric of http://www.andrew.cmu.edu/user/jessicaz/publication/meshing/
            var f000 = Corner0Voxel.GetClampedValue(chunkState.level);
            var f001 = Corner3Voxel.GetClampedValue(chunkState.level);
            var f010 = Corner4Voxel.GetClampedValue(chunkState.level);
            var f011 = Corner7Voxel.GetClampedValue(chunkState.level);
            var f100 = Corner1Voxel.GetClampedValue(chunkState.level);
            var f101 = Corner2Voxel.GetClampedValue(chunkState.level);
            var f110 = Corner5Voxel.GetClampedValue(chunkState.level);
            var f111 = Corner6Voxel.GetClampedValue(chunkState.level);

            var error = 0.0;
            for (var i = 0; i < InterpArray.Length; ++i) {
                var interpolated = UMath.TrilinearInterpolate(f000, f001, f010, f011, f100, f101, f110, f111, InterpArray[i]);
                error += Math.Abs(this[i].GetClampedValue(chunkState.level) - interpolated);
                if (error >= maxError) {
                    return true;
                }
            }

            return false;
        }


        public void Free(UnsafePool<OctreeNode> octreeNodePool)
        {
            if (isSubdivided) {
                child0.Free(octreeNodePool);
                child1.Free(octreeNodePool);
                child2.Free(octreeNodePool);
                child3.Free(octreeNodePool);
                child4.Free(octreeNodePool);
                child5.Free(octreeNodePool);
                child6.Free(octreeNodePool);
                child7.Free(octreeNodePool);
            } else if (child0 != null) {
                UDebug.LogError("Octree has children but isSubdivided is false");
            }

            isSubdivided = false;
            child0 = null;
            child1 = null;
            child2 = null;
            child3 = null;
            child4 = null;
            child5 = null;
            child6 = null;
            child7 = null;

            octreeNodePool.Free(this);
        }


        public void DrawGizmos(bool deepest)
        {
            if (!deepest || isSubdivided) {
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner0).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner1).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner1).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner2).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner2).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner3).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner3).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner0).ToUnityOrigin());

                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner4).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner5).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner5).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner6).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner6).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner7).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner7).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner4).ToUnityOrigin());

                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner0).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner4).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner1).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner5).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner2).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner6).ToUnityOrigin());
                Gizmos.DrawLine(unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner3).ToUnityOrigin(), unitConverter.ChunkVoxelToUnityPosition(chunkState.Position, Corner7).ToUnityOrigin());
            }

            if (isSubdivided) {
                child0.DrawGizmos(deepest);
                child1.DrawGizmos(deepest);
                child2.DrawGizmos(deepest);
                child3.DrawGizmos(deepest);
                child4.DrawGizmos(deepest);
                child5.DrawGizmos(deepest);
                child6.DrawGizmos(deepest);
                child7.DrawGizmos(deepest);
            }
        }


        public bool IsBorderLeft {
            get { return from.x == root.from.x; }
        }

        public bool IsBorderRight {
            get { return to.x == root.to.x; }
        }

        public bool IsBorderBottom {
            get { return from.y == root.from.y; }
        }

        public bool IsBorderTop {
            get { return to.y == root.to.y; }
        }

        public bool IsBorderBack {
            get { return from.z == root.from.z; }
        }

        public bool IsBorderFront {
            get { return to.z == root.to.z; }
        }


        public bool IsBorderLeftWithLODChange {
            get { return IsBorderLeft && chunkLODBorderState.isOnLodBorderLeft; }
        }

        public bool IsBorderRightWithLODChange {
            get { return IsBorderRight && chunkLODBorderState.isOnLodBorderRight; }
        }

        public bool IsBorderBottomWithLODChange {
            get { return IsBorderBottom && chunkLODBorderState.isOnLodBorderBottom; }
        }

        public bool IsBorderTopWithLODChange {
            get { return IsBorderTop && chunkLODBorderState.isOnLodBorderTop; }
        }

        public bool IsBorderBackWithLODChange {
            get { return IsBorderBack && chunkLODBorderState.isOnLodBorderBack; }
        }

        public bool IsBorderFrontWithLODChange {
            get { return IsBorderFront && chunkLODBorderState.isOnLodBorderFront; }
        }

        public bool IsBorderWithLODChange {
            get {
                return IsBorderLeftWithLODChange ||
                       IsBorderRightWithLODChange ||
                       IsBorderBottomWithLODChange ||
                       IsBorderTopWithLODChange ||
                       IsBorderBackWithLODChange ||
                       IsBorderFrontWithLODChange;
            }
        }


        public Vector3d CenterReal {
            get { return (from + to) * 0.5 * Param.SIZE_OFFSET_INVERSE; }
        }

        public Vector3i Center {
            get { return new Vector3i(from.x + halfSize.x, from.y + halfSize.y, from.z + halfSize.z); }
        }

        public Vector3i CenterBack {
            get { return new Vector3i(from.x + halfSize.x, from.y + halfSize.y, from.z); }
        }

        /** Gets the center of the corners 2, 3, 6, 7.
@return
The center.
*/
        public Vector3i CenterFront {
            get { return new Vector3i(from.x + halfSize.x, from.y + halfSize.y, to.z); }
        }

        /** Gets the center of the corners 0, 3, 4, 6.
@return
The center.
*/
        public Vector3i CenterLeft {
            get { return new Vector3i(from.x, from.y + halfSize.y, from.z + halfSize.z); }
        }

        /** Gets the center of the corners 1, 2, 5, 6.
@return
The center.
*/
        public Vector3i CenterRight {
            get { return new Vector3i(to.x, from.y + halfSize.y, from.z + halfSize.z); }
        }

        /** Gets the center of the corners 4, 5, 6, 7.
@return
The center.
*/
        public Vector3i CenterTop {
            get { return new Vector3i(from.x + halfSize.x, to.y, from.z + halfSize.z); }
        }

        /** Gets the center of the corners 0, 1, 2, 3.
@return
The center.
*/
        public Vector3i CenterBottom {
            get { return new Vector3i(from.x + halfSize.x, from.y, from.z + halfSize.z); }
        }

        /** Gets the center of the corners 4, 5.
@return
The center.
*/
        public Vector3i CenterBackTop {
            get { return new Vector3i(from.x + halfSize.x, to.y, from.z); }
        }

        /** Gets the center of the corners 0, 1.
@return
The center.
*/
        public Vector3i CenterBackBottom {
            get { return new Vector3i(from.x + halfSize.x, from.y, from.z); }
        }

        /** Gets the center of the corners 6, 7.
@return
The center.
*/
        public Vector3i CenterFrontTop {
            get { return new Vector3i(from.x + halfSize.x, to.y, to.z); }
        }

        /** Gets the center of the corners 2, 3.
@return
The center.
*/
        public Vector3i CenterFrontBottom {
            get { return new Vector3i(from.x + halfSize.x, from.y, to.z); }
        }

        /** Gets the center of the corners 4, 7.
@return
The center.
*/
        public Vector3i CenterLeftTop {
            get { return new Vector3i(from.x, to.y, from.z + halfSize.z); }
        }

        /** Gets the center of the corners 0, 3.
@return
The center.
*/
        public Vector3i CenterLeftBottom {
            get { return new Vector3i(from.x, from.y, from.z + halfSize.z); }
        }

        /** Gets the center of the corners 5, 6.
@return
The center.
*/
        public Vector3i CenterRightTop {
            get { return new Vector3i(to.x, to.y, from.z + halfSize.z); }
        }

        /** Gets the center of the corners 1, 2.
@return
The center.
*/
        public Vector3i CenterRightBottom {
            get { return new Vector3i(to.x, from.y, from.z + halfSize.z); }
        }

        /** Gets the center of the corners 0, 4.
@return
The center.
*/
        public Vector3i CenterBackLeft {
            get { return new Vector3i(from.x, from.y + halfSize.y, from.z); }
        }

        /** Gets the center of the corners 3, 7.
@return
The center.
*/
        public Vector3i CenterFrontLeft {
            get { return new Vector3i(from.x, from.y + halfSize.y, to.z); }
        }

        /** Gets the center of the corners 1, 5.
@return
The center.
*/
        public Vector3i CenterBackRight {
            get { return new Vector3i(to.x, from.y + halfSize.y, from.z); }
        }

        /** Gets the center of the corners 2, 6.
@return
The center.
*/
        public Vector3i CenterFrontRight {
            get { return new Vector3i(to.x, from.y + halfSize.y, to.z); }
        }

        /** Gets the coordinate of corner 0.
@return
The corner.
*/
        public Vector3i Corner0 {
            get { return from; }
        }

        /** Gets the coordinate of corner 1.
@return
The corner.
*/
        public Vector3i Corner1 {
            get { return new Vector3i(to.x, from.y, from.z); }
        }

        /** Gets the coordinate of corner 2.
@return
The corner.
*/
        public Vector3i Corner2 {
            get { return new Vector3i(to.x, from.y, to.z); }
        }

        /** Gets the coordinate of corner 3.
@return
The corner.
*/
        public Vector3i Corner3 {
            get { return new Vector3i(from.x, from.y, to.z); }
        }

        /** Gets the coordinate of corner 4.
@return
The corner.
*/
        public Vector3i Corner4 {
            get { return new Vector3i(from.x, to.y, from.z); }
        }

        /** Gets the coordinate of corner 5.
@return
The corner.
*/
        public Vector3i Corner5 {
            get { return new Vector3i(to.x, to.y, from.z); }
        }

        /** Gets the coordinate of corner 6.
@return
The corner.
*/
        public Vector3i Corner6 {
            get { return to; }
        }

        /** Gets the coordinate of corner 7.
@return
The corner.
*/
        public Vector3i Corner7 {
            get { return new Vector3i(from.x, to.y, to.z); }
        }


        private Voxel this[int i] {
            get {
                switch (i) {
                    case 0:
                        return CenterBackBottomVoxel;
                    case 1:
                        return CenterLeftBottomVoxel;
                    case 2:
                        return CenterBottomVoxel;
                    case 3:
                        return CenterRightBottomVoxel;
                    case 4:
                        return CenterFrontBottomVoxel;

                    case 5:
                        return CenterBackLeftVoxel;
                    case 6:
                        return CenterBackVoxel;
                    case 7:
                        return CenterBackRightVoxel;
                    case 8:
                        return CenterLeftVoxel;
                    case 9:
                        return CenterVoxel;
                    case 10:
                        return CenterRightVoxel;
                    case 11:
                        return CenterFrontLeftVoxel;
                    case 12:
                        return CenterFrontVoxel;
                    case 13:
                        return CenterFrontRightVoxel;

                    case 14:
                        return CenterBackTopVoxel;
                    case 15:
                        return CenterLeftTopVoxel;
                    case 16:
                        return CenterTopVoxel;
                    case 17:
                        return CenterRightTopVoxel;
                    case 18:
                        return CenterFrontTopVoxel;
                    default:
                        throw new ArgumentOutOfRangeException(string.Format("There is no value at {0} index.", i));
                }
            }
        }
    }
}