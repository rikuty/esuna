using System;
using UnityEngine;

namespace UltimateTerrains
{
    internal sealed class OctreeNodeVertexData
    {
        public const int MiddleEdgeVertexBackXIndex = 9;
        public const int MiddleEdgeVertexBackYIndex = 4;
        public const int MiddleEdgeVertexFrontXIndex = 10;
        public const int MiddleEdgeVertexFrontYIndex = 6;
        public const int MiddleEdgeVertexLeftZIndex = 11;
        public const int MiddleEdgeVertexLeftYIndex = 7;
        public const int MiddleEdgeVertexRightZIndex = 10;
        public const int MiddleEdgeVertexRightYIndex = 5;
        public const int MiddleEdgeVertexBottomXIndex = 1;
        public const int MiddleEdgeVertexBottomZIndex = 2;
        public const int MiddleEdgeVertexTopXIndex = 5;
        public const int MiddleEdgeVertexTopZIndex = 6;

        // Node this data belongs to
        private readonly OctreeNode node;

        // Mesh computed from data of this octree
        private MeshData mesh;

        private UnsafePool<VertexData> vertexPool;

        // Fast access to converter
        private UnitConverter converter;
        private VoxelGenerator voxelGenerator;
        private VoxelTypeSet voxelTypeSet;
        private int level;
        private bool duplicateVertices;
        private Vector3d chunkUnityWorldPosition;

        private readonly double[][] matrix;
        private readonly double[] vector;

        private int minQEFIndex;
        private int minQEFBackIndex;
        private int minQEFFrontIndex;
        private int minQEFLeftIndex;
        private int minQEFRightIndex;
        private int minQEFTopIndex;
        private int minQEFBottomIndex;
        private readonly int[] edgesVerticesIndexes;
        private readonly int[] cornersVerticesIndexes;

        private VertexData minQEF;
        private VertexData minQEFBack;
        private VertexData minQEFFront;
        private VertexData minQEFLeft;
        private VertexData minQEFRight;
        private VertexData minQEFTop;
        private VertexData minQEFBottom;
        private readonly VertexData[] edgesVertices;
        private readonly VertexData[] cornersVertices;

        private bool isUsedMinQEFIndex;
        private bool isUsedMinQEFBackIndex;
        private bool isUsedMinQEFFrontIndex;
        private bool isUsedMinQEFLeftIndex;
        private bool isUsedMinQEFRightIndex;
        private bool isUsedMinQEFTopIndex;
        private bool isUsedMinQEFBottomIndex;
        private readonly bool[] isUsedEdgesVerticesIndexes;
        private readonly bool[] isUsedCornersVerticesIndexes;


        public int QEFi {
            get { return minQEFIndex; }
        }

        public int QEFiBack {
            get { return minQEFBackIndex; }
        }

        public int QEFiFront {
            get { return minQEFFrontIndex; }
        }

        public int QEFiLeft {
            get { return minQEFLeftIndex; }
        }

        public int QEFiRight {
            get { return minQEFRightIndex; }
        }

        public int QEFiTop {
            get { return minQEFTopIndex; }
        }

        public int QEFiBottom {
            get { return minQEFBottomIndex; }
        }

        public int[] EdgesVerticesIndexes {
            get { return edgesVerticesIndexes; }
        }

        public int[] CornersVerticesIndexes {
            get { return cornersVerticesIndexes; }
        }

        public int DuplicatedQEFi {
            get {
                if (duplicateVertices) {
                    if (!isUsedMinQEFIndex) {
                        if (minQEFIndex != -1) {
                            isUsedMinQEFIndex = true;
                        }

                        return minQEFIndex;
                    }

                    return Duplicate(minQEF);
                }

                return minQEFIndex;
            }
        }

        public int DuplicatedQEFiBack {
            get {
                if (duplicateVertices) {
                    if (!isUsedMinQEFBackIndex) {
                        if (minQEFBackIndex != -1) {
                            isUsedMinQEFBackIndex = true;
                        }

                        return minQEFBackIndex;
                    }

                    return Duplicate(minQEFBack);
                }

                return minQEFBackIndex;
            }
        }

        public int DuplicatedQEFiFront {
            get {
                if (duplicateVertices) {
                    if (!isUsedMinQEFFrontIndex) {
                        if (minQEFFrontIndex != -1) {
                            isUsedMinQEFFrontIndex = true;
                        }

                        return minQEFFrontIndex;
                    }

                    return Duplicate(minQEFFront);
                }

                return minQEFFrontIndex;
            }
        }

        public int DuplicatedQEFiLeft {
            get {
                if (duplicateVertices) {
                    if (!isUsedMinQEFLeftIndex) {
                        if (minQEFLeftIndex != -1) {
                            isUsedMinQEFLeftIndex = true;
                        }

                        return minQEFLeftIndex;
                    }

                    return Duplicate(minQEFLeft);
                }

                return minQEFLeftIndex;
            }
        }

        public int DuplicatedQEFiRight {
            get {
                if (duplicateVertices) {
                    if (!isUsedMinQEFRightIndex) {
                        if (minQEFRightIndex != -1) {
                            isUsedMinQEFRightIndex = true;
                        }

                        return minQEFRightIndex;
                    }

                    return Duplicate(minQEFRight);
                }

                return minQEFRightIndex;
            }
        }

        public int DuplicatedQEFiTop {
            get {
                if (duplicateVertices) {
                    if (!isUsedMinQEFTopIndex) {
                        if (minQEFTopIndex != -1) {
                            isUsedMinQEFTopIndex = true;
                        }

                        return minQEFTopIndex;
                    }

                    return Duplicate(minQEFTop);
                }

                return minQEFTopIndex;
            }
        }

        public int DuplicatedQEFiBottom {
            get {
                if (duplicateVertices) {
                    if (!isUsedMinQEFBottomIndex) {
                        if (minQEFBottomIndex != -1) {
                            isUsedMinQEFBottomIndex = true;
                        }

                        return minQEFBottomIndex;
                    }

                    return Duplicate(minQEFBottom);
                }

                return minQEFBottomIndex;
            }
        }

        public int DuplicatedEdgesVerticesIndexes(int edge)
        {
            if (duplicateVertices) {
                if (!isUsedEdgesVerticesIndexes[edge]) {
                    var iv = edgesVerticesIndexes[edge];
                    if (iv != -1) {
                        isUsedEdgesVerticesIndexes[edge] = true;
                    }

                    return iv;
                }

                return Duplicate(edgesVertices[edge]);
            }

            return edgesVerticesIndexes[edge];
        }

        public int DuplicatedCornersVerticesIndexes(int corner)
        {
            if (duplicateVertices) {
                if (!isUsedCornersVerticesIndexes[corner]) {
                    var iv = cornersVerticesIndexes[corner];
                    if (iv != -1) {
                        isUsedCornersVerticesIndexes[corner] = true;
                    }

                    return iv;
                }

                return Duplicate(cornersVertices[corner]);
            }

            return cornersVerticesIndexes[corner];
        }


        internal OctreeNodeVertexData(OctreeNode octreeNode)
        {
            node = octreeNode;
            matrix = new double[12][];
            for (var i = 0; i < 12; ++i) {
                matrix[i] = new double[3];
            }

            vector = new double[12];

            edgesVertices = new VertexData[12];
            edgesVerticesIndexes = new int[12];
            isUsedEdgesVerticesIndexes = new bool[12];

            cornersVertices = new VertexData[8];
            cornersVerticesIndexes = new int[8];
            isUsedCornersVerticesIndexes = new bool[8];
        }


        internal void PreInit(MeshData nodeMesh, 
                              VoxelGenerator generator, 
                              UnitConverter unitConverter, 
                              Param param, 
                              VoxelTypeSet voxelTypeSet, 
                              ChunkState chunkState, 
                              UnsafePool<VertexData> vertexDataPool)
        {
            this.voxelTypeSet = voxelTypeSet;
            voxelGenerator = generator;
            mesh = nodeMesh;
            vertexPool = vertexDataPool;
            chunkUnityWorldPosition = unitConverter.VoxelToUnityPosition(chunkState.WorldPosition);

            minQEFIndex = -1;
            minQEFBackIndex = -1;
            minQEFFrontIndex = -1;
            minQEFLeftIndex = -1;
            minQEFRightIndex = -1;
            minQEFTopIndex = -1;
            minQEFBottomIndex = -1;

            minQEF = null;
            minQEFBack = null;
            minQEFFront = null;
            minQEFLeft = null;
            minQEFRight = null;
            minQEFTop = null;
            minQEFBottom = null;

            isUsedMinQEFIndex = false;
            isUsedMinQEFBackIndex = false;
            isUsedMinQEFFrontIndex = false;
            isUsedMinQEFLeftIndex = false;
            isUsedMinQEFRightIndex = false;
            isUsedMinQEFTopIndex = false;
            isUsedMinQEFBottomIndex = false;

            for (var i = 0; i < 12; ++i) {
                edgesVertices[i] = null;
                edgesVerticesIndexes[i] = -1;
                isUsedEdgesVerticesIndexes[i] = false;
            }

            for (var i = 0; i < 8; ++i) {
                cornersVertices[i] = null;
                cornersVerticesIndexes[i] = -1;
                isUsedCornersVerticesIndexes[i] = false;
            }

            converter = unitConverter;
            level = chunkState.level;
            duplicateVertices = param.DuplicateVertices;
        }


        internal void ComputeVertices(ChunkBuilder chunkBuilder)
        {
            //UnityEngine.Profiler.BeginSample ("ComputeVertices");
            ComputeMinQEFVertexForVoxels(chunkBuilder, ref minQEF, ref minQEFIndex, true,
                                         node.Corner0Voxel, node.Corner1Voxel, node.Corner2Voxel, node.Corner3Voxel,
                                         node.Corner4Voxel, node.Corner5Voxel, node.Corner6Voxel, node.Corner7Voxel);

            // back
            if (node.IsBorderBack) {
                ComputeMinQEFVertexForVoxels(chunkBuilder, ref minQEFBack, ref minQEFBackIndex, false,
                                             node.Corner0Voxel, node.Corner1Voxel, node.Corner1Voxel, node.Corner0Voxel,
                                             node.Corner4Voxel, node.Corner5Voxel, node.Corner5Voxel, node.Corner4Voxel);
            }

            // front
            if (node.IsBorderFront) {
                ComputeMinQEFVertexForVoxels(chunkBuilder, ref minQEFFront, ref minQEFFrontIndex, false,
                                             node.Corner3Voxel, node.Corner2Voxel, node.Corner2Voxel, node.Corner3Voxel,
                                             node.Corner7Voxel, node.Corner6Voxel, node.Corner6Voxel, node.Corner7Voxel);
            }

            // right
            if (node.IsBorderRight) {
                ComputeMinQEFVertexForVoxels(chunkBuilder, ref minQEFRight, ref minQEFRightIndex, false,
                                             node.Corner1Voxel, node.Corner1Voxel, node.Corner2Voxel, node.Corner2Voxel,
                                             node.Corner5Voxel, node.Corner5Voxel, node.Corner6Voxel, node.Corner6Voxel);
            }

            // left
            if (node.IsBorderLeft) {
                ComputeMinQEFVertexForVoxels(chunkBuilder, ref minQEFLeft, ref minQEFLeftIndex, false,
                                             node.Corner0Voxel, node.Corner0Voxel, node.Corner3Voxel, node.Corner3Voxel,
                                             node.Corner4Voxel, node.Corner4Voxel, node.Corner7Voxel, node.Corner7Voxel);
            }

            // top
            if (node.IsBorderTop) {
                ComputeMinQEFVertexForVoxels(chunkBuilder, ref minQEFTop, ref minQEFTopIndex, false,
                                             node.Corner4Voxel, node.Corner5Voxel, node.Corner6Voxel, node.Corner7Voxel,
                                             node.Corner4Voxel, node.Corner5Voxel, node.Corner6Voxel, node.Corner7Voxel);
            }

            // bottom
            if (node.IsBorderBottom) {
                ComputeMinQEFVertexForVoxels(chunkBuilder, ref minQEFBottom, ref minQEFBottomIndex, false,
                                             node.Corner0Voxel, node.Corner1Voxel, node.Corner2Voxel, node.Corner3Voxel,
                                             node.Corner0Voxel, node.Corner1Voxel, node.Corner2Voxel, node.Corner3Voxel);
            }

            //UnityEngine.Profiler.EndSample ();
        }


        private void ComputeMinQEFVertexForVoxels(ChunkBuilder chunkBuilder, ref VertexData vertex, ref int index, bool compEdges,
                                                  Voxel v0, Voxel v1, Voxel v2, Voxel v3, Voxel v4, Voxel v5, Voxel v6, Voxel v7)
        {
            var mc = chunkBuilder.MarchingCubesQEF;
            mc.SetVoxel(0, v0);
            mc.SetVoxel(1, v1);
            mc.SetVoxel(2, v2);
            mc.SetVoxel(3, v3);
            mc.SetVoxel(4, v4);
            mc.SetVoxel(5, v5);
            mc.SetVoxel(6, v6);
            mc.SetVoxel(7, v7);
            var nbIntersect = mc.ComputeIntersections();

            if (nbIntersect > 0) {
                var minQEFPoint = MinimizeQEF(mc, chunkBuilder.QEF);
                ConstrainMinQEFPoint(v0, v1, v3, v4, ref minQEFPoint);

                minQEFPoint = converter.ToLeveledPosition(minQEFPoint, level);
                var normal = mc.QEFVertexNormal;
                if (mc.Blockiness > 0f) {
                    minQEFPoint = Vector3d.Lerp(minQEFPoint, converter.ToLeveledPosition(mc.Center, level), mc.Blockiness);
                    var n = Vector3d.zero;
                    if (v0.IsInside)
                        n += mc.Center - v0.RealPosition;
                    if (v1.IsInside)
                        n += mc.Center - v1.RealPosition;
                    if (v2.IsInside)
                        n += mc.Center - v2.RealPosition;
                    if (v3.IsInside)
                        n += mc.Center - v3.RealPosition;
                    if (v4.IsInside)
                        n += mc.Center - v4.RealPosition;
                    if (v5.IsInside)
                        n += mc.Center - v5.RealPosition;
                    if (v6.IsInside)
                        n += mc.Center - v6.RealPosition;
                    if (v7.IsInside)
                        n += mc.Center - v7.RealPosition;

                    if (!n.IsZero)
                        normal = Vector3d.Lerp(normal, n.Normalized, mc.Blockiness);
                }

                vertex = vertexPool.Get();
                vertex.Vertex = (Vector3) minQEFPoint;
                vertex.SetNormalAndVoxelType(chunkUnityWorldPosition, (Vector3) normal, mc.QEFVoxelType);

                index = mesh.Vertices.Count;
                mesh.Vertices.Add(vertex);

                if (compEdges) {
                    ComputeEdgeVertices(mc);
                    if (!node.IsChunkLevelOfLowestLOD) {
                        ComputeMiddleEdgeVerticesForLowerLOD(mc);
                    }
                }
            }
        }

        private void ConstrainMinQEFPoint(Voxel v0, Voxel v1, Voxel v3, Voxel v4, ref Vector3d minQEFPoint)
        {
            var constraint = node.IsBorderWithLODChange ? 0.2 : 0.1;
            if (minQEFPoint.x < v0.RealPosition.x - constraint)
                minQEFPoint.x = v0.RealPosition.x - constraint;
            if (minQEFPoint.x > v1.RealPosition.x + constraint)
                minQEFPoint.x = v1.RealPosition.x + constraint;
            if (minQEFPoint.y < v0.RealPosition.y - constraint)
                minQEFPoint.y = v0.RealPosition.y - constraint;
            if (minQEFPoint.y > v4.RealPosition.y + constraint)
                minQEFPoint.y = v4.RealPosition.y + constraint;
            if (minQEFPoint.z < v0.RealPosition.z - constraint)
                minQEFPoint.z = v0.RealPosition.z - constraint;
            if (minQEFPoint.z > v3.RealPosition.z + constraint)
                minQEFPoint.z = v3.RealPosition.z + constraint;
        }


        private Vector3d MinimizeQEF(MarchingCubesQEF mc, QEF qef)
        {
            var vertices = mc.EdgeVertices;
            var normals = mc.EdgeNormals;
            var edgeSignChanges = mc.EdgeSignChanges;
            Vector3d p, n;
            var row = 0;

            for (var i = 0; i < 12; ++i) {
                if (edgeSignChanges[i]) {
                    n = normals[i];
                    if (n.MagnitudeSquared > 0.01) {
                        p = vertices[i] - mc.MassPoint;
                        vector[row] = n.x * p.x + n.y * p.y + n.z * p.z;
                        matrix[row][0] = n.x;
                        matrix[row][1] = n.y;
                        matrix[row][2] = n.z;

                        ++row;
                    }
                }
            }

            // if min qef cannot be determined, return the center of the node
            if (row < 2) {
                return mc.Center;
            }

            return qef.Evaluate(matrix, vector, row) + mc.MassPoint;
        }


        private void ComputeEdgeVertices(MarchingCubesQEF mc)
        {
            if (node.IsBorderTop) {
                if (node.IsBorderBack) {
                    MakeEdgeVertex(mc, 4);
                }

                if (node.IsBorderFront) {
                    MakeEdgeVertex(mc, 6);
                }

                if (node.IsBorderLeft) {
                    MakeEdgeVertex(mc, 7);
                }

                if (node.IsBorderRight) {
                    MakeEdgeVertex(mc, 5);
                }
            }

            if (node.IsBorderBottom) {
                if (node.IsBorderBack) {
                    MakeEdgeVertex(mc, 0);
                }

                if (node.IsBorderFront) {
                    MakeEdgeVertex(mc, 2);
                }

                if (node.IsBorderLeft) {
                    MakeEdgeVertex(mc, 3);
                }

                if (node.IsBorderRight) {
                    MakeEdgeVertex(mc, 1);
                }
            }

            if (node.IsBorderFront) {
                if (node.IsBorderLeft) {
                    MakeEdgeVertex(mc, 11);
                }

                if (node.IsBorderRight) {
                    MakeEdgeVertex(mc, 10);
                }
            }

            if (node.IsBorderBack) {
                if (node.IsBorderLeft) {
                    MakeEdgeVertex(mc, 8);
                }

                if (node.IsBorderRight) {
                    MakeEdgeVertex(mc, 9);
                }
            }
        }

        private void ComputeMiddleEdgeVerticesForLowerLOD(MarchingCubesQEF mc)
        {
            if (node.HasMiddleEdgeVertexBackX) {
                MakeEdgeVertex(mc, MiddleEdgeVertexBackXIndex);
            }

            if (node.HasMiddleEdgeVertexBackY) {
                MakeEdgeVertex(mc, MiddleEdgeVertexBackYIndex);
            }

            if (node.HasMiddleEdgeVertexFrontX) {
                MakeEdgeVertex(mc, MiddleEdgeVertexFrontXIndex);
            }

            if (node.HasMiddleEdgeVertexFrontY) {
                MakeEdgeVertex(mc, MiddleEdgeVertexFrontYIndex);
            }

            if (node.HasMiddleEdgeVertexLeftZ) {
                MakeEdgeVertex(mc, MiddleEdgeVertexLeftZIndex);
            }

            if (node.HasMiddleEdgeVertexLeftY) {
                MakeEdgeVertex(mc, MiddleEdgeVertexLeftYIndex);
            }

            if (node.HasMiddleEdgeVertexRightZ) {
                MakeEdgeVertex(mc, MiddleEdgeVertexRightZIndex);
            }

            if (node.HasMiddleEdgeVertexRightY) {
                MakeEdgeVertex(mc, MiddleEdgeVertexRightYIndex);
            }

            if (node.HasMiddleEdgeVertexBottomX) {
                MakeEdgeVertex(mc, MiddleEdgeVertexBottomXIndex);
            }

            if (node.HasMiddleEdgeVertexBottomZ) {
                MakeEdgeVertex(mc, MiddleEdgeVertexBottomZIndex);
            }

            if (node.HasMiddleEdgeVertexTopX) {
                MakeEdgeVertex(mc, MiddleEdgeVertexTopXIndex);
            }

            if (node.HasMiddleEdgeVertexTopZ) {
                MakeEdgeVertex(mc, MiddleEdgeVertexTopZIndex);
            }
        }


        private void MakeEdgeVertex(MarchingCubesQEF mc, int edge)
        {
            if (mc.EdgeSignChanges[edge]) {
                var vertex = vertexPool.Get();
                var minQEFPoint = converter.ToLeveledPosition(mc.EdgeVertices[edge], level);
                var normal = mc.EdgeNormals[edge];
                if (mc.Blockiness > 0) {
                    minQEFPoint = Vector3d.Lerp(minQEFPoint, converter.ToLeveledPosition(GetRealCenterOfEdge(edge), level), mc.Blockiness);
                    var v0 = mc.GetVoxel(GetEdgeFirstIndex(edge));
                    var v1 = mc.GetVoxel(GetEdgeSecondIndex(edge));
                    if (v0.IsInside) {
                        normal = Vector3d.Lerp(normal, v1.RealPosition - v0.RealPosition, mc.Blockiness).Normalized;
                    } else {
                        normal = Vector3d.Lerp(normal, v0.RealPosition - v1.RealPosition, mc.Blockiness).Normalized;
                    }
                }

                vertex.Vertex = (Vector3) minQEFPoint;
                vertex.SetNormalAndVoxelType(chunkUnityWorldPosition, (Vector3) normal, mc.QEFVoxelType);

                edgesVertices[edge] = vertex;
                edgesVerticesIndexes[edge] = mesh.Vertices.Count;
                mesh.Vertices.Add(vertex);
            }
        }


        private void CreateMinQEFAt(Vector3d pos, out VertexData vertex, out int index)
        {
            vertex = vertexPool.Get();
            vertex.Vertex = (Vector3) converter.ToLeveledPosition(pos, level);
            var gradient = voxelGenerator.ComputeGradientInChunk(pos);
            vertex.SetNormalAndVoxelType(chunkUnityWorldPosition, (Vector3) gradient, voxelTypeSet.GetVoxelType(node.CenterVoxel.VoxelTypeIndex));
            index = mesh.Vertices.Count;
            mesh.Vertices.Add(vertex);
        }


        public void CreateMinQEFAtCenter()
        {
            CreateMinQEFAt(node.CenterReal, out minQEF, out minQEFIndex);
        }

        public void CreateMinQEFAtCenterTop()
        {
            CreateMinQEFAt(new Vector3d((node.From.x + node.To.x) * 0.5 * Param.SIZE_OFFSET_INVERSE, node.To.y * Param.SIZE_OFFSET_INVERSE, (node.From.z + node.To.z) * 0.5 * Param.SIZE_OFFSET_INVERSE),
                           out minQEFTop, out minQEFTopIndex);
        }

        public void CreateMinQEFAtCenterBottom()
        {
            CreateMinQEFAt(new Vector3d((node.From.x + node.To.x) * 0.5 * Param.SIZE_OFFSET_INVERSE, node.From.y * Param.SIZE_OFFSET_INVERSE, (node.From.z + node.To.z) * 0.5 * Param.SIZE_OFFSET_INVERSE),
                           out minQEFBottom, out minQEFBottomIndex);
        }

        public void CreateMinQEFAtCenterFront()
        {
            CreateMinQEFAt(new Vector3d((node.From.x + node.To.x) * 0.5 * Param.SIZE_OFFSET_INVERSE, (node.From.y + node.To.y) * 0.5 * Param.SIZE_OFFSET_INVERSE, node.To.z * Param.SIZE_OFFSET_INVERSE),
                           out minQEFFront, out minQEFFrontIndex);
        }

        public void CreateMinQEFAtCenterBack()
        {
            CreateMinQEFAt(new Vector3d((node.From.x + node.To.x) * 0.5 * Param.SIZE_OFFSET_INVERSE, (node.From.y + node.To.y) * 0.5 * Param.SIZE_OFFSET_INVERSE, node.From.z * Param.SIZE_OFFSET_INVERSE),
                           out minQEFBack, out minQEFBackIndex);
        }

        public void CreateMinQEFAtCenterRight()
        {
            CreateMinQEFAt(new Vector3d(node.To.x * Param.SIZE_OFFSET_INVERSE, (node.From.y + node.To.y) * 0.5 * Param.SIZE_OFFSET_INVERSE, (node.From.z + node.To.z) * 0.5 * Param.SIZE_OFFSET_INVERSE),
                           out minQEFRight, out minQEFRightIndex);
        }

        public void CreateMinQEFAtCenterLeft()
        {
            CreateMinQEFAt(new Vector3d(node.From.x * Param.SIZE_OFFSET_INVERSE, (node.From.y + node.To.y) * 0.5 * Param.SIZE_OFFSET_INVERSE, (node.From.z + node.To.z) * 0.5 * Param.SIZE_OFFSET_INVERSE),
                           out minQEFLeft, out minQEFLeftIndex);
        }

        public void CreateMinQEFAtCenterEdge(int edge)
        {
            CreateMinQEFAt(GetRealCenterOfEdge(edge), out edgesVertices[edge], out edgesVerticesIndexes[edge]);
        }

        public void CreateMinQEFAtCorner(int corner)
        {
            CreateMinQEFAt(GetRealCorner(corner), out cornersVertices[corner], out cornersVerticesIndexes[corner]);
        }

        private Vector3d GetRealCorner(int corner)
        {
            switch (corner) {
                case 0:
                    return node.Corner0 * Param.SIZE_OFFSET_INVERSE;
                case 1:
                    return node.Corner1 * Param.SIZE_OFFSET_INVERSE;
                case 2:
                    return node.Corner2 * Param.SIZE_OFFSET_INVERSE;
                case 3:
                    return node.Corner3 * Param.SIZE_OFFSET_INVERSE;
                case 4:
                    return node.Corner4 * Param.SIZE_OFFSET_INVERSE;
                case 5:
                    return node.Corner5 * Param.SIZE_OFFSET_INVERSE;
                case 6:
                    return node.Corner6 * Param.SIZE_OFFSET_INVERSE;
                case 7:
                    return node.Corner7 * Param.SIZE_OFFSET_INVERSE;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("There is no corner n°{0}.", corner));
            }
        }

        private Vector3d GetRealCenterOfEdge(int edge)
        {
            switch (edge) {
                case 0:
                    return (node.Corner0 + node.Corner1) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 1:
                    return (node.Corner1 + node.Corner2) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 2:
                    return (node.Corner2 + node.Corner3) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 3:
                    return (node.Corner3 + node.Corner0) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 4:
                    return (node.Corner4 + node.Corner5) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 5:
                    return (node.Corner5 + node.Corner6) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 6:
                    return (node.Corner6 + node.Corner7) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 7:
                    return (node.Corner7 + node.Corner4) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 8:
                    return (node.Corner0 + node.Corner4) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 9:
                    return (node.Corner1 + node.Corner5) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 10:
                    return (node.Corner2 + node.Corner6) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                case 11:
                    return (node.Corner3 + node.Corner7) * 0.5 * Param.SIZE_OFFSET_INVERSE;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("There is no edge n°{0}.", edge));
            }
        }

        private static int GetEdgeFirstIndex(int edge)
        {
            switch (edge) {
                case 0:
                    return 0;
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 3;
                case 4:
                    return 4;
                case 5:
                    return 5;
                case 6:
                    return 6;
                case 7:
                    return 7;
                case 8:
                    return 0;
                case 9:
                    return 1;
                case 10:
                    return 2;
                case 11:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("There is no edge n°{0}.", edge));
            }
        }

        private static int GetEdgeSecondIndex(int edge)
        {
            switch (edge) {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                    return 3;
                case 3:
                    return 0;
                case 4:
                    return 5;
                case 5:
                    return 6;
                case 6:
                    return 7;
                case 7:
                    return 4;
                case 8:
                    return 4;
                case 9:
                    return 5;
                case 10:
                    return 6;
                case 11:
                    return 7;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("There is no edge n°{0}.", edge));
            }
        }

        private int Duplicate(VertexData vert)
        {
            var vertex = vertexPool.Get();
            vert.CopyTo(vertex);
            var index = mesh.Vertices.Count;
            mesh.Vertices.Add(vertex);
            return index;
        }
    }
}