//#define CONTOURING_DEBUG

using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UltimateTerrains
{
    internal sealed class DualContouring
    {
        private MeshData mesh;
        private List<int>[] indices;
        private OctreeNode root;
        private readonly bool addSkirts;

        internal DualContouring(Param param)
        {
            addSkirts = param.HasSkirts;
        }

        /** Generates the dualgrid of the given octree root node.
        @param root
            The octree root node.
        @param totalFrom
            The global from.
        @param totalTo
            The global to.
        @param saveDualCells
            Whether to save the generated dualcells of the generated dual cells.
        */
        internal void GenerateDualCountour(OctreeNode rootNode)
        {
            root = rootNode;
            mesh = rootNode.Mesh;
            indices = mesh.Indices;

            if (root.IsSubdivided) {
                ProcessNode(root);
            } else {
                // Build up a minimal dualgrid for octrees without children.
                CreateCornersContour(root);
            }
        }


        //-----------------------------------------------------------------------

        private void ProcessNode(OctreeNode n)
        {
            if (n.IsSubdivided) {
                var c0 = n.Child0;
                var c1 = n.Child1;
                var c2 = n.Child2;
                var c3 = n.Child3;
                var c4 = n.Child4;
                var c5 = n.Child5;
                var c6 = n.Child6;
                var c7 = n.Child7;

                ProcessNode(c0);
                ProcessNode(c1);
                ProcessNode(c2);
                ProcessNode(c3);
                ProcessNode(c4);
                ProcessNode(c5);
                ProcessNode(c6);
                ProcessNode(c7);

                ProcessFaceXY(c0, c3);
                ProcessFaceXY(c1, c2);
                ProcessFaceXY(c4, c7);
                ProcessFaceXY(c5, c6);

                ProcessFaceZY(c0, c1);
                ProcessFaceZY(c3, c2);
                ProcessFaceZY(c4, c5);
                ProcessFaceZY(c7, c6);

                ProcessFaceXZ(c4, c0);
                ProcessFaceXZ(c5, c1);
                ProcessFaceXZ(c7, c3);
                ProcessFaceXZ(c6, c2);

                ProcessEdgeX(c0, c3, c7, c4);
                ProcessEdgeX(c1, c2, c6, c5);

                ProcessEdgeY(c0, c1, c2, c3);
                ProcessEdgeY(c4, c5, c6, c7);

                ProcessEdgeZ(c7, c6, c2, c3);
                ProcessEdgeZ(c4, c5, c1, c0);
            }
        }

        //-----------------------------------------------------------------------

        private void ProcessFaceXY(OctreeNode n0, OctreeNode n1)
        {
            var n0Subdivided = n0.IsSubdivided;
            var n1Subdivided = n1.IsSubdivided;

            if (n0Subdivided || n1Subdivided) {
                var c0 = n0Subdivided ? n0.Child3 : n0;
                var c1 = n0Subdivided ? n0.Child2 : n0;
                var c2 = n1Subdivided ? n1.Child1 : n1;
                var c3 = n1Subdivided ? n1.Child0 : n1;

                var c4 = n0Subdivided ? n0.Child7 : n0;
                var c5 = n0Subdivided ? n0.Child6 : n0;
                var c6 = n1Subdivided ? n1.Child5 : n1;
                var c7 = n1Subdivided ? n1.Child4 : n1;

                ProcessFaceXY(c0, c3);
                ProcessFaceXY(c1, c2);
                ProcessFaceXY(c4, c7);
                ProcessFaceXY(c5, c6);

                ProcessEdgeX(c0, c3, c7, c4);
                ProcessEdgeX(c1, c2, c6, c5);
                ProcessEdgeY(c0, c1, c2, c3);
                ProcessEdgeY(c4, c5, c6, c7);
            }
        }

        //-----------------------------------------------------------------------

        private void ProcessFaceZY(OctreeNode n0, OctreeNode n1)
        {
            var n0Subdivided = n0.IsSubdivided;
            var n1Subdivided = n1.IsSubdivided;

            if (n0Subdivided || n1Subdivided) {
                var c0 = n0Subdivided ? n0.Child1 : n0;
                var c1 = n1Subdivided ? n1.Child0 : n1;
                var c2 = n1Subdivided ? n1.Child3 : n1;
                var c3 = n0Subdivided ? n0.Child2 : n0;

                var c4 = n0Subdivided ? n0.Child5 : n0;
                var c5 = n1Subdivided ? n1.Child4 : n1;
                var c6 = n1Subdivided ? n1.Child7 : n1;
                var c7 = n0Subdivided ? n0.Child6 : n0;

                ProcessFaceZY(c0, c1);
                ProcessFaceZY(c3, c2);
                ProcessFaceZY(c4, c5);
                ProcessFaceZY(c7, c6);

                ProcessEdgeY(c0, c1, c2, c3);
                ProcessEdgeY(c4, c5, c6, c7);
                ProcessEdgeZ(c7, c6, c2, c3);
                ProcessEdgeZ(c4, c5, c1, c0);
            }
        }

        //-----------------------------------------------------------------------

        private void ProcessFaceXZ(OctreeNode n0, OctreeNode n1)
        {
            var n0Subdivided = n0.IsSubdivided;
            var n1Subdivided = n1.IsSubdivided;

            if (n0Subdivided || n1Subdivided) {
                var c0 = n1Subdivided ? n1.Child4 : n1;
                var c1 = n1Subdivided ? n1.Child5 : n1;
                var c2 = n1Subdivided ? n1.Child6 : n1;
                var c3 = n1Subdivided ? n1.Child7 : n1;

                var c4 = n0Subdivided ? n0.Child0 : n0;
                var c5 = n0Subdivided ? n0.Child1 : n0;
                var c6 = n0Subdivided ? n0.Child2 : n0;
                var c7 = n0Subdivided ? n0.Child3 : n0;

                ProcessFaceXZ(c4, c0);
                ProcessFaceXZ(c5, c1);
                ProcessFaceXZ(c7, c3);
                ProcessFaceXZ(c6, c2);

                ProcessEdgeX(c0, c3, c7, c4);
                ProcessEdgeX(c1, c2, c6, c5);
                ProcessEdgeZ(c7, c6, c2, c3);
                ProcessEdgeZ(c4, c5, c1, c0);
            }
        }

        //-----------------------------------------------------------------------

        private void ProcessEdgeX(OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            var n0Subdivided = n0.IsSubdivided;
            var n1Subdivided = n1.IsSubdivided;
            var n2Subdivided = n2.IsSubdivided;
            var n3Subdivided = n3.IsSubdivided;

            if (n0Subdivided || n1Subdivided || n2Subdivided || n3Subdivided) {
                var c0 = n0Subdivided ? n0.Child7 : n0;
                var c1 = n0Subdivided ? n0.Child6 : n0;
                var c2 = n1Subdivided ? n1.Child5 : n1;
                var c3 = n1Subdivided ? n1.Child4 : n1;
                var c4 = n3Subdivided ? n3.Child3 : n3;
                var c5 = n3Subdivided ? n3.Child2 : n3;
                var c6 = n2Subdivided ? n2.Child1 : n2;
                var c7 = n2Subdivided ? n2.Child0 : n2;

                ProcessEdgeX(c0, c3, c7, c4);
                ProcessEdgeX(c1, c2, c6, c5);
            } else {
                var minLevel = n0.Level;
                if (n1.Level < minLevel)
                    minLevel = n1.Level;
                if (n2.Level < minLevel)
                    minLevel = n2.Level;
                if (n3.Level < minLevel)
                    minLevel = n3.Level;

                if (n0.Level == minLevel) {
                    AddQuadSafe(n0.Corner7Voxel, n0.Corner6Voxel, n0, n1, n2, n3);
                } else if (n1.Level == minLevel) {
                    AddQuadSafe(n1.Corner4Voxel, n1.Corner5Voxel, n0, n1, n2, n3);
                } else if (n2.Level == minLevel) {
                    AddQuadSafe(n2.Corner0Voxel, n2.Corner1Voxel, n0, n1, n2, n3);
                } else {
                    AddQuadSafe(n3.Corner3Voxel, n3.Corner2Voxel, n0, n1, n2, n3);
                }

                CreateBorderYContourForEdgeX(n0, n3);
                CreateBorderYContourForEdgeX(n1, n2);
                CreateBorderZContourForEdgeX(n0, n1);
                CreateBorderZContourForEdgeX(n3, n2);
                CreateCornersContour(n0);
                CreateCornersContour(n1);
                CreateCornersContour(n2);
                CreateCornersContour(n3);

                if (addSkirts) {
                    CreateSkirtsLeft(minLevel, n0, n1, n2, n3);
                    CreateSkirtsRight(minLevel, n0, n1, n2, n3);
                }
            }
        }

        //-----------------------------------------------------------------------

        private void ProcessEdgeY(OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            var n0Subdivided = n0.IsSubdivided;
            var n1Subdivided = n1.IsSubdivided;
            var n2Subdivided = n2.IsSubdivided;
            var n3Subdivided = n3.IsSubdivided;

            if (n0Subdivided || n1Subdivided || n2Subdivided || n3Subdivided) {
                var c0 = n0Subdivided ? n0.Child2 : n0;
                var c1 = n1Subdivided ? n1.Child3 : n1;
                var c2 = n2Subdivided ? n2.Child0 : n2;
                var c3 = n3Subdivided ? n3.Child1 : n3;
                var c4 = n0Subdivided ? n0.Child6 : n0;
                var c5 = n1Subdivided ? n1.Child7 : n1;
                var c6 = n2Subdivided ? n2.Child4 : n2;
                var c7 = n3Subdivided ? n3.Child5 : n3;

                ProcessEdgeY(c0, c1, c2, c3);
                ProcessEdgeY(c4, c5, c6, c7);
            } else {
                var minLevel = n0.Level;
                if (n1.Level < minLevel)
                    minLevel = n1.Level;
                if (n2.Level < minLevel)
                    minLevel = n2.Level;
                if (n3.Level < minLevel)
                    minLevel = n3.Level;

                if (n0.Level == minLevel) {
                    AddQuadSafe(n0.Corner2Voxel, n0.Corner6Voxel, n0, n1, n2, n3);
                } else if (n1.Level == minLevel) {
                    AddQuadSafe(n1.Corner3Voxel, n1.Corner7Voxel, n0, n1, n2, n3);
                } else if (n2.Level == minLevel) {
                    AddQuadSafe(n2.Corner0Voxel, n2.Corner4Voxel, n0, n1, n2, n3);
                } else {
                    AddQuadSafe(n3.Corner1Voxel, n3.Corner5Voxel, n0, n1, n2, n3);
                }

                CreateBorderXContourForEdgeY(n0, n1);
                CreateBorderXContourForEdgeY(n3, n2);
                CreateBorderZContourForEdgeY(n0, n3);
                CreateBorderZContourForEdgeY(n1, n2);
                CreateCornersContour(n0);
                CreateCornersContour(n1);
                CreateCornersContour(n2);
                CreateCornersContour(n3);

                if (addSkirts) {
                    CreateSkirtsBottom(minLevel, n0, n1, n2, n3);
                    CreateSkirtsTop(minLevel, n0, n1, n2, n3);
                }
            }
        }

        //-----------------------------------------------------------------------

        private void ProcessEdgeZ(OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            var n0Subdivided = n0.IsSubdivided;
            var n1Subdivided = n1.IsSubdivided;
            var n2Subdivided = n2.IsSubdivided;
            var n3Subdivided = n3.IsSubdivided;

            if (n0Subdivided || n1Subdivided || n2Subdivided || n3Subdivided) {
                var c0 = n3Subdivided ? n3.Child5 : n3;
                var c1 = n2Subdivided ? n2.Child4 : n2;
                var c2 = n2Subdivided ? n2.Child7 : n2;
                var c3 = n3Subdivided ? n3.Child6 : n3;
                var c4 = n0Subdivided ? n0.Child1 : n0;
                var c5 = n1Subdivided ? n1.Child0 : n1;
                var c6 = n1Subdivided ? n1.Child3 : n1;
                var c7 = n0Subdivided ? n0.Child2 : n0;

                ProcessEdgeZ(c7, c6, c2, c3);
                ProcessEdgeZ(c4, c5, c1, c0);
            } else {
                var minLevel = n0.Level;
                if (n1.Level < minLevel)
                    minLevel = n1.Level;
                if (n2.Level < minLevel)
                    minLevel = n2.Level;
                if (n3.Level < minLevel)
                    minLevel = n3.Level;

                if (n0.Level == minLevel) {
                    AddQuadSafe(n0.Corner1Voxel, n0.Corner2Voxel, n0, n1, n2, n3);
                } else if (n1.Level == minLevel) {
                    AddQuadSafe(n1.Corner0Voxel, n1.Corner3Voxel, n0, n1, n2, n3);
                } else if (n2.Level == minLevel) {
                    AddQuadSafe(n2.Corner4Voxel, n2.Corner7Voxel, n0, n1, n2, n3);
                } else {
                    AddQuadSafe(n3.Corner5Voxel, n3.Corner6Voxel, n0, n1, n2, n3);
                }

                CreateBorderXContourForEdgeZ(n0, n1);
                CreateBorderXContourForEdgeZ(n3, n2);
                CreateBorderYContourForEdgeZ(n3, n0);
                CreateBorderYContourForEdgeZ(n2, n1);
                CreateCornersContour(n0);
                CreateCornersContour(n1);
                CreateCornersContour(n2);
                CreateCornersContour(n3);

                if (addSkirts) {
                    CreateSkirtsFront(minLevel, n0, n1, n2, n3);
                    CreateSkirtsBack(minLevel, n0, n1, n2, n3);
                }
            }
        }

        //-----------------------------------------------------------------------

        private void CreateBorderXContourForEdgeY(OctreeNode n0, OctreeNode n1)
        {
            if (n0 == n1)
                return;

            // Front: n0, n1, n1, n0
            if (n0.IsBorderFront && n1.IsBorderFront && (!n0.HasBorderXContourForEdgeYRight || !n1.HasBorderXContourForEdgeYLeft)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderFrontXContourForEdgeY(n0, n1, n0.Corner2Voxel, n0.Corner6Voxel);
                } else {
                    CreateBorderFrontXContourForEdgeY(n0, n1, n1.Corner3Voxel, n1.Corner7Voxel);
                }
            }

            // Back: n0, n1, n1, n0
            if (n0.IsBorderBack && n1.IsBorderBack && (!n0.HasBorderXContourForEdgeYRight || !n1.HasBorderXContourForEdgeYLeft)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderBackXContourForEdgeY(n0, n1, n0.Corner1Voxel, n0.Corner5Voxel);
                } else {
                    CreateBorderBackXContourForEdgeY(n0, n1, n1.Corner0Voxel, n1.Corner4Voxel);
                }
            }
        }

        private void CreateBorderFrontXContourForEdgeY(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiFront(n0);
                CheckQEFiFront(n1);

                if (n0.HasMiddleEdgeVertexFrontX) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexFrontXIndex);
                    AddQuad(vA, vB, middleEdgeVertexIndex, n0.DupQEFi, n1.DupQEFi, n1.DupQEFiFront);
                    AddQuad(vA, vB, n0.DupQEFi, middleEdgeVertexIndex, middleEdgeVertexIndex, n0.DupQEFiFront);
                } else {
                    AddQuad(vA, vB, n0.DupQEFi, n1.DupQEFi, n1.DupQEFiFront, n0.DupQEFiFront);
                }

                n0.HasBorderXContourForEdgeYRight = true;
                n1.HasBorderXContourForEdgeYLeft = true;
            }
        }

        private void CreateBorderBackXContourForEdgeY(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiBack(n0);
                CheckQEFiBack(n1);

                if (n0.HasMiddleEdgeVertexBackX) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexBackXIndex);
                    AddQuad(vA, vB, n0.DupQEFi, n0.DupQEFiBack, middleEdgeVertexIndex, n1.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n1.DupQEFiBack, n1.DupQEFi);
                } else {
                    AddQuad(vA, vB, n0.DupQEFiBack, n1.DupQEFiBack, n1.DupQEFi, n0.DupQEFi);
                }

                n0.HasBorderXContourForEdgeYRight = true;
                n1.HasBorderXContourForEdgeYLeft = true;
            }
        }


        private void CreateBorderXContourForEdgeZ(OctreeNode n0, OctreeNode n1)
        {
            if (n0 == n1)
                return;

            // Top: n0, n1, n1, n0
            if (n0.IsBorderTop && n1.IsBorderTop && (!n0.HasBorderXContourForEdgeZRight || !n1.HasBorderXContourForEdgeZLeft)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderTopXContourForEdgeZ(n0, n1, n0.Corner5Voxel, n0.Corner6Voxel);
                } else {
                    CreateBorderTopXContourForEdgeZ(n0, n1, n1.Corner4Voxel, n1.Corner7Voxel);
                }
            }

            // Bottom: n0, n1, n1, n0
            if (n0.IsBorderBottom && n1.IsBorderBottom && (!n0.HasBorderXContourForEdgeZRight || !n1.HasBorderXContourForEdgeZLeft)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderBottomXContourForEdgeZ(n0, n1, n0.Corner1Voxel, n0.Corner2Voxel);
                } else {
                    CreateBorderBottomXContourForEdgeZ(n0, n1, n1.Corner0Voxel, n1.Corner3Voxel);
                }
            }
        }

        private void CreateBorderTopXContourForEdgeZ(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiTop(n0);
                CheckQEFiTop(n1);

                if (n0.HasMiddleEdgeVertexTopX) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexTopXIndex);
                    AddQuad(vA, vB, n0.DupQEFi, n0.DupQEFiTop, middleEdgeVertexIndex, n1.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n1.DupQEFiTop, n1.DupQEFi);
                } else {
                    AddQuad(vA, vB, n0.DupQEFiTop, n1.DupQEFiTop, n1.DupQEFi, n0.DupQEFi);
                }

                n0.HasBorderXContourForEdgeZRight = true;
                n1.HasBorderXContourForEdgeZLeft = true;
            }
        }

        private void CreateBorderBottomXContourForEdgeZ(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiBottom(n0);
                CheckQEFiBottom(n1);

                if (n0.HasMiddleEdgeVertexBottomX) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexBottomXIndex);
                    AddQuad(vA, vB, n0.DupQEFi, n1.DupQEFi, middleEdgeVertexIndex, n0.DupQEFiBottom);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n1.DupQEFi, n1.DupQEFiBottom);
                } else {
                    AddQuad(vA, vB, n0.DupQEFi, n1.DupQEFi, n1.DupQEFiBottom, n0.DupQEFiBottom);
                }

                n0.HasBorderXContourForEdgeZRight = true;
                n1.HasBorderXContourForEdgeZLeft = true;
            }
        }


        private void CreateBorderYContourForEdgeX(OctreeNode n0, OctreeNode n1)
        {
            if (n0 == n1)
                return;

            // Front: n0, n0, n1, n1
            if (n0.IsBorderFront && n1.IsBorderFront && (!n0.HasBorderYContourForEdgeXTop || !n1.HasBorderYContourForEdgeXBottom)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderFrontYContourForEdgeX(n0, n1, n0.Corner7Voxel, n0.Corner6Voxel);
                } else {
                    CreateBorderFrontYContourForEdgeX(n0, n1, n1.Corner3Voxel, n1.Corner2Voxel);
                }
            }

            // Back: n0, n0, n1, n1
            if (n0.IsBorderBack && n1.IsBorderBack && (!n0.HasBorderYContourForEdgeXTop || !n1.HasBorderYContourForEdgeXBottom)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderBackYContourForEdgeX(n0, n1, n0.Corner4Voxel, n0.Corner5Voxel);
                } else if (n1.Corner0Voxel.IsInside != n1.Corner1Voxel.IsInside) {
                    CreateBorderBackYContourForEdgeX(n0, n1, n1.Corner0Voxel, n1.Corner1Voxel);
                }
            }
        }

        private void CreateBorderFrontYContourForEdgeX(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiFront(n0);
                CheckQEFiFront(n1);

                if (n0.HasMiddleEdgeVertexFrontY) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexFrontYIndex);
                    AddQuad(vA, vB, n0.DupQEFi, n0.DupQEFiFront, middleEdgeVertexIndex, n1.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n1.DupQEFiFront, n1.DupQEFi);
                } else {
                    AddQuad(vA, vB, n0.DupQEFi, n0.DupQEFiFront, n1.DupQEFiFront, n1.DupQEFi);
                }

                n0.HasBorderYContourForEdgeXTop = true;
                n1.HasBorderYContourForEdgeXBottom = true;
            }
        }

        private void CreateBorderBackYContourForEdgeX(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiBack(n0);
                CheckQEFiBack(n1);

                if (n0.HasMiddleEdgeVertexBackY) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexBackYIndex);
                    AddQuad(vA, vB, middleEdgeVertexIndex, n0.DupQEFiBack, n0.DupQEFi, n1.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n1.DupQEFi, n1.DupQEFiBack);
                } else {
                    AddQuad(vA, vB, n0.DupQEFiBack, n0.DupQEFi, n1.DupQEFi, n1.DupQEFiBack);
                }

                n0.HasBorderYContourForEdgeXTop = true;
                n1.HasBorderYContourForEdgeXBottom = true;
            }
        }


        private void CreateBorderYContourForEdgeZ(OctreeNode n0, OctreeNode n1)
        {
            if (n0 == n1)
                return;

            // Left: n1, n1, n0, n0
            if (n0.IsBorderLeft && n1.IsBorderLeft && (!n0.HasBorderYContourForEdgeZTop || !n1.HasBorderYContourForEdgeZBottom)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderLeftYContourForEdgeZ(n0, n1, n0.Corner4Voxel, n0.Corner7Voxel);
                } else {
                    CreateBorderLeftYContourForEdgeZ(n0, n1, n1.Corner0Voxel, n1.Corner3Voxel);
                }
            }

            // Right n1, n1, n0, n0
            if (n0.IsBorderRight && n1.IsBorderRight && (!n0.HasBorderYContourForEdgeZTop || !n1.HasBorderYContourForEdgeZBottom)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderRightYContourForEdgeZ(n0, n1, n0.Corner5Voxel, n0.Corner6Voxel);
                } else {
                    CreateBorderRightYContourForEdgeZ(n0, n1, n1.Corner1Voxel, n1.Corner2Voxel);
                }
            }
        }

        private void CreateBorderLeftYContourForEdgeZ(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiLeft(n0);
                CheckQEFiLeft(n1);

                if (n0.HasMiddleEdgeVertexLeftY) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexLeftYIndex);
                    AddQuad(vA, vB, middleEdgeVertexIndex, n1.DupQEFiLeft, n1.DupQEFi, n0.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n0.DupQEFi, n0.DupQEFiLeft);
                } else {
                    AddQuad(vA, vB, n1.DupQEFiLeft, n1.DupQEFi, n0.DupQEFi, n0.DupQEFiLeft);
                }

                n0.HasBorderYContourForEdgeZTop = true;
                n1.HasBorderYContourForEdgeZBottom = true;
            }
        }

        private void CreateBorderRightYContourForEdgeZ(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiRight(n0);
                CheckQEFiRight(n1);

                if (n0.HasMiddleEdgeVertexRightY) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexRightYIndex);
                    AddQuad(vA, vB, n1.DupQEFi, n1.DupQEFiRight, middleEdgeVertexIndex, n0.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n0.DupQEFiRight, n0.DupQEFi);
                } else {
                    AddQuad(vA, vB, n1.DupQEFi, n1.DupQEFiRight, n0.DupQEFiRight, n0.DupQEFi);
                }

                n0.HasBorderYContourForEdgeZTop = true;
                n1.HasBorderYContourForEdgeZBottom = true;
            }
        }


        private void CreateBorderZContourForEdgeY(OctreeNode n0, OctreeNode n1)
        {
            if (n0 == n1)
                return;

            // Left: n0, n0, n1, n1
            if (n0.IsBorderLeft && n1.IsBorderLeft && (!n0.HasBorderZContourForEdgeYFront || !n1.HasBorderZContourForEdgeYBack)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderLeftZContourForEdgeY(n0, n1, n0.Corner3Voxel, n0.Corner7Voxel);
                } else {
                    CreateBorderLeftZContourForEdgeY(n0, n1, n1.Corner0Voxel, n1.Corner4Voxel);
                }
            }

            // Right: n0, n0, n1, n1
            if (n0.IsBorderRight && n1.IsBorderRight && (!n0.HasBorderZContourForEdgeYFront || !n1.HasBorderZContourForEdgeYBack)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderRightZContourForEdgeY(n0, n1, n0.Corner2Voxel, n0.Corner6Voxel);
                } else {
                    CreateBorderRightZContourForEdgeY(n0, n1, n1.Corner1Voxel, n1.Corner5Voxel);
                }
            }
        }

        private void CreateBorderLeftZContourForEdgeY(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiLeft(n0);
                CheckQEFiLeft(n1);

                if (n0.HasMiddleEdgeVertexLeftZ) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexLeftZIndex);
                    AddQuad(vA, vB, middleEdgeVertexIndex, n0.DupQEFiLeft, n0.DupQEFi, n1.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n1.DupQEFi, n1.DupQEFiLeft);
                } else {
                    AddQuad(vA, vB, n0.DupQEFiLeft, n0.DupQEFi, n1.DupQEFi, n1.DupQEFiLeft);
                }

                n0.HasBorderZContourForEdgeYFront = true;
                n1.HasBorderZContourForEdgeYBack = true;
            }
        }

        private void CreateBorderRightZContourForEdgeY(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiRight(n0);
                CheckQEFiRight(n1);

                if (n0.HasMiddleEdgeVertexRightZ) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexRightZIndex);
                    AddQuad(vA, vB, n0.DupQEFi, n0.DupQEFiRight, middleEdgeVertexIndex, n1.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n1.DupQEFiRight, n1.DupQEFi);
                } else {
                    AddQuad(vA, vB, n0.DupQEFi, n0.DupQEFiRight, n1.DupQEFiRight, n1.DupQEFi);
                }

                n0.HasBorderZContourForEdgeYFront = true;
                n1.HasBorderZContourForEdgeYBack = true;
            }
        }


        private void CreateBorderZContourForEdgeX(OctreeNode n0, OctreeNode n1)
        {
            if (n0 == n1)
                return;

            // Top: n0, n1, n1, n0
            if (n0.IsBorderTop && n1.IsBorderTop && (!n0.HasBorderZContourForEdgeXFront || !n1.HasBorderZContourForEdgeXBack)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderTopZContourForEdgeX(n0, n1, n0.Corner7Voxel, n0.Corner6Voxel);
                } else {
                    CreateBorderTopZContourForEdgeX(n0, n1, n1.Corner4Voxel, n1.Corner5Voxel);
                }
            }

            // Bottom: n0, n1, n1, n0
            if (n0.IsBorderBottom && n1.IsBorderBottom && (!n0.HasBorderZContourForEdgeXFront || !n1.HasBorderZContourForEdgeXBack)) {
                if (n0.Level <= n1.Level) {
                    CreateBorderBottomZContourForEdgeX(n0, n1, n0.Corner3Voxel, n0.Corner2Voxel);
                } else {
                    CreateBorderBottomZContourForEdgeX(n0, n1, n1.Corner0Voxel, n1.Corner1Voxel);
                }
            }
        }

        private void CreateBorderTopZContourForEdgeX(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiTop(n0);
                CheckQEFiTop(n1);

                if (n0.HasMiddleEdgeVertexTopZ) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexTopZIndex);
                    AddQuad(vA, vB, middleEdgeVertexIndex, n0.DupQEFi, n1.DupQEFi, n1.DupQEFiTop);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n0.DupQEFiTop, n0.DupQEFi);
                } else {
                    AddQuad(vA, vB, n0.DupQEFi, n1.DupQEFi, n1.DupQEFiTop, n0.DupQEFiTop);
                }

                n0.HasBorderZContourForEdgeXFront = true;
                n1.HasBorderZContourForEdgeXBack = true;
            }
        }

        private void CreateBorderBottomZContourForEdgeX(OctreeNode n0, OctreeNode n1, Voxel vA, Voxel vB)
        {
            if (vA.IsInside != vB.IsInside) {
                CheckQEFi(n0);
                CheckQEFi(n1);
                CheckQEFiBottom(n0);
                CheckQEFiBottom(n1);

                if (n0.HasMiddleEdgeVertexBottomZ) {
                    var middleEdgeVertexIndex = n0.DupEdgesVerticesIndexes(OctreeNodeVertexData.MiddleEdgeVertexBottomZIndex);
                    AddQuad(vA, vB, n0.DupQEFi, n0.DupQEFiBottom, middleEdgeVertexIndex, n1.DupQEFi);
                    AddQuad(vA, vB, middleEdgeVertexIndex, middleEdgeVertexIndex, n1.DupQEFiBottom, n1.DupQEFi);
                } else {
                    AddQuad(vA, vB, n0.DupQEFiBottom, n1.DupQEFiBottom, n1.DupQEFi, n0.DupQEFi);
                }

                n0.HasBorderZContourForEdgeXFront = true;
                n1.HasBorderZContourForEdgeXBack = true;
            }
        }


        private void CreateCornersContour(OctreeNode n)
        {
            if (n.IsBorderTop) {
                if (n.IsBorderBack && n.Corner4Voxel.IsInside != n.Corner5Voxel.IsInside && !n.HasCornerOfEdge[4]) {
                    CheckQEFiTop(n);
                    CheckQEFiBack(n);
                    CheckQEFiEdge(n, 4);
                    AddQuad(n.Corner4Voxel, n.Corner5Voxel, n.DupQEFiBack, n.DupQEFi, n.DupQEFiTop, n.DupEdgesVerticesIndexes(4));
                    n.HasCornerOfEdge[4] = true;
                }

                if (n.IsBorderFront && n.Corner7Voxel.IsInside != n.Corner6Voxel.IsInside && !n.HasCornerOfEdge[6]) {
                    CheckQEFiTop(n);
                    CheckQEFiFront(n);
                    CheckQEFiEdge(n, 6);
                    AddQuad(n.Corner7Voxel, n.Corner6Voxel, n.DupQEFi, n.DupQEFiFront, n.DupEdgesVerticesIndexes(6), n.DupQEFiTop);
                    n.HasCornerOfEdge[6] = true;
                }

                if (n.IsBorderLeft && n.Corner4Voxel.IsInside != n.Corner7Voxel.IsInside && !n.HasCornerOfEdge[7]) {
                    CheckQEFiTop(n);
                    CheckQEFiLeft(n);
                    CheckQEFiEdge(n, 7);
                    AddQuad(n.Corner4Voxel, n.Corner7Voxel, n.DupEdgesVerticesIndexes(7), n.DupQEFiTop, n.DupQEFi, n.DupQEFiLeft);
                    n.HasCornerOfEdge[7] = true;
                }

                if (n.IsBorderRight && n.Corner5Voxel.IsInside != n.Corner6Voxel.IsInside && !n.HasCornerOfEdge[5]) {
                    CheckQEFiTop(n);
                    CheckQEFiRight(n);
                    CheckQEFiEdge(n, 5);
                    AddQuad(n.Corner5Voxel, n.Corner6Voxel, n.DupQEFiTop, n.DupEdgesVerticesIndexes(5), n.DupQEFiRight, n.DupQEFi);
                    n.HasCornerOfEdge[5] = true;
                }
            }

            if (n.IsBorderBottom) {
                if (n.IsBorderBack && n.Corner0Voxel.IsInside != n.Corner1Voxel.IsInside && !n.HasCornerOfEdge[0]) {
                    CheckQEFiBottom(n);
                    CheckQEFiBack(n);
                    CheckQEFiEdge(n, 0);
                    AddQuad(n.Corner0Voxel, n.Corner1Voxel, n.DupEdgesVerticesIndexes(0), n.DupQEFiBottom, n.DupQEFi, n.DupQEFiBack);
                    n.HasCornerOfEdge[0] = true;
                }

                if (n.IsBorderFront && n.Corner3Voxel.IsInside != n.Corner2Voxel.IsInside && !n.HasCornerOfEdge[2]) {
                    CheckQEFiBottom(n);
                    CheckQEFiFront(n);
                    CheckQEFiEdge(n, 2);
                    AddQuad(n.Corner3Voxel, n.Corner2Voxel, n.DupQEFiBottom, n.DupEdgesVerticesIndexes(2), n.DupQEFiFront, n.DupQEFi);
                    n.HasCornerOfEdge[2] = true;
                }

                if (n.IsBorderLeft && n.Corner0Voxel.IsInside != n.Corner3Voxel.IsInside && !n.HasCornerOfEdge[3]) {
                    CheckQEFiBottom(n);
                    CheckQEFiLeft(n);
                    CheckQEFiEdge(n, 3);
                    AddQuad(n.Corner0Voxel, n.Corner3Voxel, n.DupQEFiLeft, n.DupQEFi, n.DupQEFiBottom, n.DupEdgesVerticesIndexes(3));
                    n.HasCornerOfEdge[3] = true;
                }

                if (n.IsBorderRight && n.Corner1Voxel.IsInside != n.Corner2Voxel.IsInside && !n.HasCornerOfEdge[1]) {
                    CheckQEFiBottom(n);
                    CheckQEFiRight(n);
                    CheckQEFiEdge(n, 1);
                    AddQuad(n.Corner1Voxel, n.Corner2Voxel, n.DupQEFi, n.DupQEFiRight, n.DupEdgesVerticesIndexes(1), n.DupQEFiBottom);
                    n.HasCornerOfEdge[1] = true;
                }
            }

            if (n.IsBorderFront) {
                if (n.IsBorderLeft && n.Corner3Voxel.IsInside != n.Corner7Voxel.IsInside && !n.HasCornerOfEdge[11]) {
                    CheckQEFiFront(n);
                    CheckQEFiLeft(n);
                    CheckQEFiEdge(n, 11);
                    AddQuad(n.Corner3Voxel, n.Corner7Voxel, n.DupQEFiLeft, n.DupQEFi, n.DupQEFiFront, n.DupEdgesVerticesIndexes(11));
                    n.HasCornerOfEdge[11] = true;
                }

                if (n.IsBorderRight && n.Corner2Voxel.IsInside != n.Corner6Voxel.IsInside && !n.HasCornerOfEdge[10]) {
                    CheckQEFiFront(n);
                    CheckQEFiRight(n);
                    CheckQEFiEdge(n, 10);
                    AddQuad(n.Corner2Voxel, n.Corner6Voxel, n.DupQEFi, n.DupQEFiRight, n.DupEdgesVerticesIndexes(10), n.DupQEFiFront);
                    n.HasCornerOfEdge[10] = true;
                }
            }

            if (n.IsBorderBack) {
                if (n.IsBorderLeft && n.Corner0Voxel.IsInside != n.Corner4Voxel.IsInside && !n.HasCornerOfEdge[8]) {
                    CheckQEFiBack(n);
                    CheckQEFiLeft(n);
                    CheckQEFiEdge(n, 8);
                    AddQuad(n.Corner0Voxel, n.Corner4Voxel, n.DupEdgesVerticesIndexes(8), n.DupQEFiBack, n.DupQEFi, n.DupQEFiLeft);
                    n.HasCornerOfEdge[8] = true;
                }

                if (n.IsBorderRight && n.Corner1Voxel.IsInside != n.Corner5Voxel.IsInside && !n.HasCornerOfEdge[9]) {
                    CheckQEFiBack(n);
                    CheckQEFiRight(n);
                    CheckQEFiEdge(n, 9);
                    AddQuad(n.Corner1Voxel, n.Corner5Voxel, n.DupQEFiBack, n.DupEdgesVerticesIndexes(9), n.DupQEFiRight, n.DupQEFi);
                    n.HasCornerOfEdge[9] = true;
                }
            }
        }

        private void CreateSkirtsFront(int minLevel, OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            if (n0.IsBorderFront && n1.IsBorderFront && n2.IsBorderFront && n3.IsBorderFront) {
                // Skirt edge
                bool isCenterInside;
                if (n0.Level == minLevel) {
                    isCenterInside = n0.Corner2Voxel.IsInside;
                } else if (n1.Level == minLevel) {
                    isCenterInside = n1.Corner3Voxel.IsInside;
                } else if (n2.Level == minLevel) {
                    isCenterInside = n2.Corner7Voxel.IsInside;
                } else {
                    isCenterInside = n3.Corner6Voxel.IsInside;
                }

                if (isCenterInside && (!n0.Corner7Voxel.IsInside || !n1.Corner6Voxel.IsInside || !n2.Corner2Voxel.IsInside || !n3.Corner3Voxel.IsInside)) {
                    CheckQEFiFront(n0);
                    CheckQEFiFront(n1);
                    CheckQEFiFront(n2);
                    CheckQEFiFront(n3);
                    AddQuad(n0.DupQEFiFront, n3.DupQEFiFront, n2.DupQEFiFront, n1.DupQEFiFront, n0.Corner2Voxel.MaterialIndex);
                }

                // Skirt borders
                if (n0 != n1 && n0.IsBorderTop && n1.IsBorderTop && n0.Corner6Voxel.IsInside && (!n0.Corner3Voxel.IsInside || !n0.Corner7Voxel.IsInside || !n1.Corner6Voxel.IsInside || !n1.Corner2Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 6);
                    CheckQEFiEdge(n1, 6);
                    CheckQEFiFront(n0);
                    CheckQEFiFront(n1);
                    AddQuad(n0.DupEdgesVerticesIndexes(6), n0.DupQEFiFront, n1.DupQEFiFront, n1.DupEdgesVerticesIndexes(6), n0.Corner6Voxel.MaterialIndex);
                }

                if (n1 != n2 && n1.IsBorderRight && n2.IsBorderRight && n1.Corner2Voxel.IsInside && (!n1.Corner7Voxel.IsInside || !n1.Corner6Voxel.IsInside || !n2.Corner2Voxel.IsInside || !n2.Corner3Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 10);
                    CheckQEFiEdge(n2, 10);
                    CheckQEFiFront(n1);
                    CheckQEFiFront(n2);
                    AddQuad(n1.DupEdgesVerticesIndexes(10), n1.DupQEFiFront, n2.DupQEFiFront, n2.DupEdgesVerticesIndexes(10), n1.Corner2Voxel.MaterialIndex);
                }

                if (n2 != n3 && n2.IsBorderBottom && n3.IsBorderBottom && n2.Corner3Voxel.IsInside && (!n2.Corner6Voxel.IsInside || !n2.Corner2Voxel.IsInside || !n3.Corner3Voxel.IsInside || !n3.Corner7Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 2);
                    CheckQEFiEdge(n3, 2);
                    CheckQEFiFront(n2);
                    CheckQEFiFront(n3);
                    AddQuad(n2.DupEdgesVerticesIndexes(2), n2.DupQEFiFront, n3.DupQEFiFront, n3.DupEdgesVerticesIndexes(2), n2.Corner3Voxel.MaterialIndex);
                }

                if (n3 != n0 && n3.IsBorderLeft && n0.IsBorderLeft && n3.Corner7Voxel.IsInside && (!n3.Corner2Voxel.IsInside || !n3.Corner3Voxel.IsInside || !n0.Corner7Voxel.IsInside || !n0.Corner6Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 11);
                    CheckQEFiEdge(n0, 11);
                    CheckQEFiFront(n3);
                    CheckQEFiFront(n0);
                    AddQuad(n3.DupEdgesVerticesIndexes(11), n3.DupQEFiFront, n0.DupQEFiFront, n0.DupEdgesVerticesIndexes(11), n3.Corner7Voxel.MaterialIndex);
                }

                //Skirt corners
                if (n0.IsBorderLeft && n0.IsBorderTop && n0.Corner7Voxel.IsInside && (!n0.Corner3Voxel.IsInside || !n0.Corner6Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 6);
                    CheckQEFiEdge(n0, 11);
                    CheckQEFiFront(n0);
                    CheckQEFiCorner(n0, 7);
                    AddQuad(n0.DupQEFiFront, n0.DupEdgesVerticesIndexes(6), n0.DupCornersVerticesIndexes(7), n0.DupEdgesVerticesIndexes(11), n0.Corner7Voxel.MaterialIndex);
                }

                if (n1.IsBorderRight && n1.IsBorderTop && n1.Corner6Voxel.IsInside && (!n1.Corner7Voxel.IsInside || !n1.Corner2Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 6);
                    CheckQEFiEdge(n1, 10);
                    CheckQEFiFront(n1);
                    CheckQEFiCorner(n1, 6);
                    AddQuad(n1.DupQEFiFront, n1.DupEdgesVerticesIndexes(10), n1.DupCornersVerticesIndexes(6), n1.DupEdgesVerticesIndexes(6), n1.Corner6Voxel.MaterialIndex);
                }

                if (n2.IsBorderRight && n2.IsBorderBottom && n2.Corner2Voxel.IsInside && (!n2.Corner3Voxel.IsInside || !n2.Corner6Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 10);
                    CheckQEFiEdge(n2, 2);
                    CheckQEFiFront(n2);
                    CheckQEFiCorner(n2, 2);
                    AddQuad(n2.DupQEFiFront, n2.DupEdgesVerticesIndexes(2), n2.DupCornersVerticesIndexes(2), n2.DupEdgesVerticesIndexes(10), n2.Corner2Voxel.MaterialIndex);
                }

                if (n3.IsBorderLeft && n3.IsBorderBottom && n3.Corner3Voxel.IsInside && (!n3.Corner7Voxel.IsInside || !n3.Corner2Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 11);
                    CheckQEFiEdge(n3, 2);
                    CheckQEFiFront(n3);
                    CheckQEFiCorner(n3, 3);
                    AddQuad(n3.DupQEFiFront, n3.DupEdgesVerticesIndexes(11), n3.DupCornersVerticesIndexes(3), n3.DupEdgesVerticesIndexes(2), n3.Corner3Voxel.MaterialIndex);
                }
            }
        }

        private void CreateSkirtsBack(int minLevel, OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            if (n0.IsBorderBack && n1.IsBorderBack && n2.IsBorderBack && n3.IsBorderBack) {
                // Skirt edge
                bool isCenterInside;
                if (n0.Level == minLevel) {
                    isCenterInside = n0.Corner1Voxel.IsInside;
                } else if (n1.Level == minLevel) {
                    isCenterInside = n1.Corner0Voxel.IsInside;
                } else if (n2.Level == minLevel) {
                    isCenterInside = n2.Corner4Voxel.IsInside;
                } else {
                    isCenterInside = n3.Corner5Voxel.IsInside;
                }

                if (isCenterInside && (!n0.Corner4Voxel.IsInside || !n1.Corner5Voxel.IsInside || !n2.Corner1Voxel.IsInside || !n3.Corner0Voxel.IsInside)) {
                    CheckQEFiBack(n0);
                    CheckQEFiBack(n1);
                    CheckQEFiBack(n2);
                    CheckQEFiBack(n3);
                    AddQuad(n0.DupQEFiBack, n1.DupQEFiBack, n2.DupQEFiBack, n3.DupQEFiBack, n0.Corner1Voxel.MaterialIndex);
                }

                // Skirt borders
                if (n0 != n1 && n0.IsBorderTop && n1.IsBorderTop && n0.Corner5Voxel.IsInside && (!n0.Corner0Voxel.IsInside || !n0.Corner4Voxel.IsInside || !n1.Corner5Voxel.IsInside || !n1.Corner1Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 4);
                    CheckQEFiEdge(n1, 4);
                    CheckQEFiBack(n0);
                    CheckQEFiBack(n1);
                    AddQuad(n0.DupEdgesVerticesIndexes(4), n1.DupEdgesVerticesIndexes(4), n1.DupQEFiBack, n0.DupQEFiBack, n0.Corner5Voxel.MaterialIndex);
                }

                if (n1 != n2 && n1.IsBorderRight && n2.IsBorderRight && n1.Corner1Voxel.IsInside && (!n1.Corner4Voxel.IsInside || !n1.Corner5Voxel.IsInside || !n2.Corner1Voxel.IsInside || !n2.Corner0Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 9);
                    CheckQEFiEdge(n2, 9);
                    CheckQEFiBack(n1);
                    CheckQEFiBack(n2);
                    AddQuad(n1.DupEdgesVerticesIndexes(9), n2.DupEdgesVerticesIndexes(9), n2.DupQEFiBack, n1.DupQEFiBack, n1.Corner1Voxel.MaterialIndex);
                }

                if (n2 != n3 && n2.IsBorderBottom && n3.IsBorderBottom && n2.Corner0Voxel.IsInside && (!n2.Corner5Voxel.IsInside || !n2.Corner1Voxel.IsInside || !n3.Corner0Voxel.IsInside || !n3.Corner4Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 0);
                    CheckQEFiEdge(n3, 0);
                    CheckQEFiBack(n2);
                    CheckQEFiBack(n3);
                    AddQuad(n2.DupEdgesVerticesIndexes(0), n3.DupEdgesVerticesIndexes(0), n3.DupQEFiBack, n2.DupQEFiBack, n2.Corner0Voxel.MaterialIndex);
                }

                if (n3 != n0 && n3.IsBorderLeft && n0.IsBorderLeft && n3.Corner4Voxel.IsInside && (!n3.Corner1Voxel.IsInside || !n3.Corner0Voxel.IsInside || !n0.Corner4Voxel.IsInside || !n0.Corner5Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 8);
                    CheckQEFiEdge(n0, 8);
                    CheckQEFiBack(n3);
                    CheckQEFiBack(n0);
                    AddQuad(n3.DupEdgesVerticesIndexes(8), n0.DupEdgesVerticesIndexes(8), n0.DupQEFiBack, n3.DupQEFiBack, n3.Corner4Voxel.MaterialIndex);
                }

                //Skirt corners
                if (n0.IsBorderLeft && n0.IsBorderTop && n0.Corner4Voxel.IsInside && (!n0.Corner0Voxel.IsInside || !n0.Corner5Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 8);
                    CheckQEFiEdge(n0, 4);
                    CheckQEFiBack(n0);
                    CheckQEFiCorner(n0, 4);
                    AddQuad(n0.DupQEFiBack, n0.DupEdgesVerticesIndexes(8), n0.DupCornersVerticesIndexes(4), n0.DupEdgesVerticesIndexes(4), n0.Corner4Voxel.MaterialIndex);
                }

                if (n1.IsBorderRight && n1.IsBorderTop && n1.Corner5Voxel.IsInside && (!n1.Corner4Voxel.IsInside || !n1.Corner1Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 9);
                    CheckQEFiEdge(n1, 4);
                    CheckQEFiBack(n1);
                    CheckQEFiCorner(n1, 5);
                    AddQuad(n1.DupQEFiBack, n1.DupEdgesVerticesIndexes(4), n1.DupCornersVerticesIndexes(5), n1.DupEdgesVerticesIndexes(9), n1.Corner5Voxel.MaterialIndex);
                }

                if (n2.IsBorderRight && n2.IsBorderBottom && n2.Corner1Voxel.IsInside && (!n2.Corner0Voxel.IsInside || !n2.Corner5Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 0);
                    CheckQEFiEdge(n2, 9);
                    CheckQEFiBack(n2);
                    CheckQEFiCorner(n2, 1);
                    AddQuad(n2.DupQEFiBack, n2.DupEdgesVerticesIndexes(9), n2.DupCornersVerticesIndexes(1), n2.DupEdgesVerticesIndexes(0), n2.Corner1Voxel.MaterialIndex);
                }

                if (n3.IsBorderLeft && n3.IsBorderBottom && n3.Corner0Voxel.IsInside && (!n3.Corner4Voxel.IsInside || !n3.Corner1Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 0);
                    CheckQEFiEdge(n3, 8);
                    CheckQEFiBack(n3);
                    CheckQEFiCorner(n3, 0);
                    AddQuad(n3.DupQEFiBack, n3.DupEdgesVerticesIndexes(0), n3.DupCornersVerticesIndexes(0), n3.DupEdgesVerticesIndexes(8), n3.Corner0Voxel.MaterialIndex);
                }
            }
        }

        private void CreateSkirtsLeft(int minLevel, OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            if (n0.IsBorderLeft && n1.IsBorderLeft && n2.IsBorderLeft && n3.IsBorderLeft) {
                // Skirt edge
                bool isCenterInside;
                if (n0.Level == minLevel) {
                    isCenterInside = n0.Corner7Voxel.IsInside;
                } else if (n1.Level == minLevel) {
                    isCenterInside = n1.Corner4Voxel.IsInside;
                } else if (n2.Level == minLevel) {
                    isCenterInside = n2.Corner0Voxel.IsInside;
                } else {
                    isCenterInside = n3.Corner3Voxel.IsInside;
                }

                if (isCenterInside && (!n0.Corner0Voxel.IsInside || !n1.Corner3Voxel.IsInside || !n2.Corner7Voxel.IsInside || !n3.Corner4Voxel.IsInside)) {
                    CheckQEFiLeft(n0);
                    CheckQEFiLeft(n1);
                    CheckQEFiLeft(n2);
                    CheckQEFiLeft(n3);
                    AddQuad(n0.DupQEFiLeft, n1.DupQEFiLeft, n2.DupQEFiLeft, n3.DupQEFiLeft, n0.Corner7Voxel.MaterialIndex);
                }

                // Skirt borders
                if (n0 != n1 && n0.IsBorderBottom && n1.IsBorderBottom && n0.Corner3Voxel.IsInside && (!n0.Corner0Voxel.IsInside || !n0.Corner4Voxel.IsInside || !n1.Corner3Voxel.IsInside || !n1.Corner7Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 3);
                    CheckQEFiEdge(n1, 3);
                    CheckQEFiLeft(n0);
                    CheckQEFiLeft(n1);
                    AddQuad(n0.DupEdgesVerticesIndexes(3), n1.DupEdgesVerticesIndexes(3), n1.DupQEFiLeft, n0.DupQEFiLeft, n0.Corner3Voxel.MaterialIndex);
                }

                if (n1 != n2 && n1.IsBorderFront && n2.IsBorderFront && n1.Corner7Voxel.IsInside && (!n1.Corner0Voxel.IsInside || !n1.Corner3Voxel.IsInside || !n2.Corner7Voxel.IsInside || !n2.Corner4Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 11);
                    CheckQEFiEdge(n2, 11);
                    CheckQEFiLeft(n1);
                    CheckQEFiLeft(n2);
                    AddQuad(n1.DupEdgesVerticesIndexes(11), n2.DupEdgesVerticesIndexes(11), n2.DupQEFiLeft, n1.DupQEFiLeft, n1.Corner7Voxel.MaterialIndex);
                }

                if (n2 != n3 && n2.IsBorderTop && n3.IsBorderTop && n2.Corner4Voxel.IsInside && (!n2.Corner7Voxel.IsInside || !n2.Corner3Voxel.IsInside || !n3.Corner4Voxel.IsInside || !n3.Corner0Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 7);
                    CheckQEFiEdge(n3, 7);
                    CheckQEFiLeft(n2);
                    CheckQEFiLeft(n3);
                    AddQuad(n2.DupEdgesVerticesIndexes(7), n3.DupEdgesVerticesIndexes(7), n3.DupQEFiLeft, n2.DupQEFiLeft, n2.Corner4Voxel.MaterialIndex);
                }

                if (n3 != n0 && n3.IsBorderBack && n0.IsBorderBack && n3.Corner0Voxel.IsInside && (!n3.Corner4Voxel.IsInside || !n3.Corner7Voxel.IsInside || !n0.Corner0Voxel.IsInside || !n0.Corner3Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 8);
                    CheckQEFiEdge(n0, 8);
                    CheckQEFiLeft(n3);
                    CheckQEFiLeft(n0);
                    AddQuad(n3.DupEdgesVerticesIndexes(8), n0.DupEdgesVerticesIndexes(8), n0.DupQEFiLeft, n3.DupQEFiLeft, n3.Corner0Voxel.MaterialIndex);
                }

                //Skirt corners
                if (n0.IsBorderBack && n0.IsBorderBottom && n0.Corner0Voxel.IsInside && (!n0.Corner3Voxel.IsInside || !n0.Corner4Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 3);
                    CheckQEFiEdge(n0, 8);
                    CheckQEFiLeft(n0);
                    CheckQEFiCorner(n0, 0);
                    AddQuad(n0.DupQEFiLeft, n0.DupEdgesVerticesIndexes(8), n0.DupCornersVerticesIndexes(0), n0.DupEdgesVerticesIndexes(3), n0.Corner0Voxel.MaterialIndex);
                }

                if (n1.IsBorderFront && n1.IsBorderBottom && n1.Corner3Voxel.IsInside && (!n1.Corner7Voxel.IsInside || !n1.Corner0Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 3);
                    CheckQEFiEdge(n1, 11);
                    CheckQEFiLeft(n1);
                    CheckQEFiCorner(n1, 3);
                    AddQuad(n1.DupQEFiLeft, n1.DupEdgesVerticesIndexes(3), n1.DupCornersVerticesIndexes(3), n1.DupEdgesVerticesIndexes(11), n1.Corner3Voxel.MaterialIndex);
                }

                if (n2.IsBorderFront && n2.IsBorderTop && n2.Corner7Voxel.IsInside && (!n2.Corner3Voxel.IsInside || !n2.Corner4Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 11);
                    CheckQEFiEdge(n2, 7);
                    CheckQEFiLeft(n2);
                    CheckQEFiCorner(n2, 7);
                    AddQuad(n2.DupQEFiLeft, n2.DupEdgesVerticesIndexes(11), n2.DupCornersVerticesIndexes(7), n2.DupEdgesVerticesIndexes(7), n2.Corner7Voxel.MaterialIndex);
                }

                if (n3.IsBorderBack && n3.IsBorderTop && n3.Corner4Voxel.IsInside && (!n3.Corner7Voxel.IsInside || !n3.Corner0Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 7);
                    CheckQEFiEdge(n3, 8);
                    CheckQEFiLeft(n3);
                    CheckQEFiCorner(n3, 4);
                    AddQuad(n3.DupQEFiLeft, n3.DupEdgesVerticesIndexes(7), n3.DupCornersVerticesIndexes(4), n3.DupEdgesVerticesIndexes(8), n3.Corner4Voxel.MaterialIndex);
                }
            }
        }

        private void CreateSkirtsRight(int minLevel, OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            if (n0.IsBorderRight && n1.IsBorderRight && n2.IsBorderRight && n3.IsBorderRight) {
                // Skirt edge
                bool isCenterInside;
                if (n0.Level == minLevel) {
                    isCenterInside = n0.Corner6Voxel.IsInside;
                } else if (n1.Level == minLevel) {
                    isCenterInside = n1.Corner5Voxel.IsInside;
                } else if (n2.Level == minLevel) {
                    isCenterInside = n2.Corner1Voxel.IsInside;
                } else {
                    isCenterInside = n3.Corner2Voxel.IsInside;
                }

                if (isCenterInside && (!n0.Corner1Voxel.IsInside || !n1.Corner2Voxel.IsInside || !n2.Corner6Voxel.IsInside || !n3.Corner5Voxel.IsInside)) {
                    CheckQEFiRight(n0);
                    CheckQEFiRight(n1);
                    CheckQEFiRight(n2);
                    CheckQEFiRight(n3);
                    AddQuad(n0.DupQEFiRight, n3.DupQEFiRight, n2.DupQEFiRight, n1.DupQEFiRight, n0.Corner6Voxel.MaterialIndex);
                }

                // Skirt borders
                if (n0 != n1 && n0.IsBorderBottom && n1.IsBorderBottom && n0.Corner2Voxel.IsInside && (!n0.Corner1Voxel.IsInside || !n0.Corner5Voxel.IsInside || !n1.Corner2Voxel.IsInside || !n1.Corner6Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 1);
                    CheckQEFiEdge(n1, 1);
                    CheckQEFiRight(n0);
                    CheckQEFiRight(n1);
                    AddQuad(n0.DupEdgesVerticesIndexes(1), n0.DupQEFiRight, n1.DupQEFiRight, n1.DupEdgesVerticesIndexes(1), n0.Corner2Voxel.MaterialIndex);
                }

                if (n1 != n2 && n1.IsBorderFront && n2.IsBorderFront && n1.Corner6Voxel.IsInside && (!n1.Corner1Voxel.IsInside || !n1.Corner2Voxel.IsInside || !n2.Corner6Voxel.IsInside || !n2.Corner5Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 10);
                    CheckQEFiEdge(n2, 10);
                    CheckQEFiRight(n1);
                    CheckQEFiRight(n2);
                    AddQuad(n1.DupEdgesVerticesIndexes(10), n1.DupQEFiRight, n2.DupQEFiRight, n2.DupEdgesVerticesIndexes(10), n1.Corner6Voxel.MaterialIndex);
                }

                if (n2 != n3 && n2.IsBorderTop && n3.IsBorderTop && n2.Corner5Voxel.IsInside && (!n2.Corner6Voxel.IsInside || !n2.Corner2Voxel.IsInside || !n3.Corner5Voxel.IsInside || !n3.Corner1Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 5);
                    CheckQEFiEdge(n3, 5);
                    CheckQEFiRight(n2);
                    CheckQEFiRight(n3);
                    AddQuad(n2.DupEdgesVerticesIndexes(5), n2.DupQEFiRight, n3.DupQEFiRight, n3.DupEdgesVerticesIndexes(5), n2.Corner5Voxel.MaterialIndex);
                }

                if (n3 != n0 && n3.IsBorderBack && n0.IsBorderBack && n3.Corner1Voxel.IsInside && (!n3.Corner5Voxel.IsInside || !n3.Corner6Voxel.IsInside || !n0.Corner1Voxel.IsInside || !n0.Corner2Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 9);
                    CheckQEFiEdge(n0, 9);
                    CheckQEFiRight(n3);
                    CheckQEFiRight(n0);
                    AddQuad(n3.DupEdgesVerticesIndexes(9), n3.DupQEFiRight, n0.DupQEFiRight, n0.DupEdgesVerticesIndexes(9), n3.Corner1Voxel.MaterialIndex);
                }

                //Skirt corners
                if (n0.IsBorderBack && n0.IsBorderBottom && n0.Corner1Voxel.IsInside && (!n0.Corner2Voxel.IsInside || !n0.Corner5Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 1);
                    CheckQEFiEdge(n0, 9);
                    CheckQEFiRight(n0);
                    CheckQEFiCorner(n0, 1);
                    AddQuad(n0.DupQEFiRight, n0.DupEdgesVerticesIndexes(1), n0.DupCornersVerticesIndexes(1), n0.DupEdgesVerticesIndexes(9), n0.Corner1Voxel.MaterialIndex);
                }

                if (n1.IsBorderFront && n1.IsBorderBottom && n1.Corner2Voxel.IsInside && (!n1.Corner6Voxel.IsInside || !n1.Corner1Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 1);
                    CheckQEFiEdge(n1, 10);
                    CheckQEFiRight(n1);
                    CheckQEFiCorner(n1, 2);
                    AddQuad(n1.DupQEFiRight, n1.DupEdgesVerticesIndexes(10), n1.DupCornersVerticesIndexes(2), n1.DupEdgesVerticesIndexes(1), n1.Corner2Voxel.MaterialIndex);
                }

                if (n2.IsBorderFront && n2.IsBorderTop && n2.Corner6Voxel.IsInside && (!n2.Corner2Voxel.IsInside || !n2.Corner5Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 10);
                    CheckQEFiEdge(n2, 5);
                    CheckQEFiRight(n2);
                    CheckQEFiCorner(n2, 6);
                    AddQuad(n2.DupQEFiRight, n2.DupEdgesVerticesIndexes(5), n2.DupCornersVerticesIndexes(6), n2.DupEdgesVerticesIndexes(10), n2.Corner6Voxel.MaterialIndex);
                }

                if (n3.IsBorderBack && n3.IsBorderTop && n3.Corner5Voxel.IsInside && (!n3.Corner6Voxel.IsInside || !n3.Corner1Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 5);
                    CheckQEFiEdge(n3, 9);
                    CheckQEFiRight(n3);
                    CheckQEFiCorner(n3, 5);
                    AddQuad(n3.DupQEFiRight, n3.DupEdgesVerticesIndexes(9), n3.DupCornersVerticesIndexes(5), n3.DupEdgesVerticesIndexes(5), n3.Corner5Voxel.MaterialIndex);
                }
            }
        }

        private void CreateSkirtsBottom(int minLevel, OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            if (n0.IsBorderBottom && n1.IsBorderBottom && n2.IsBorderBottom && n3.IsBorderBottom) {
                // Skirt edge
                bool isCenterInside;
                if (n0.Level == minLevel) {
                    isCenterInside = n0.Corner2Voxel.IsInside;
                } else if (n1.Level == minLevel) {
                    isCenterInside = n1.Corner3Voxel.IsInside;
                } else if (n2.Level == minLevel) {
                    isCenterInside = n2.Corner0Voxel.IsInside;
                } else {
                    isCenterInside = n3.Corner1Voxel.IsInside;
                }

                if (isCenterInside && (!n0.Corner0Voxel.IsInside || !n1.Corner1Voxel.IsInside || !n2.Corner2Voxel.IsInside || !n3.Corner3Voxel.IsInside)) {
                    CheckQEFiBottom(n0);
                    CheckQEFiBottom(n1);
                    CheckQEFiBottom(n2);
                    CheckQEFiBottom(n3);
                    AddQuad(n0.DupQEFiBottom, n1.DupQEFiBottom, n2.DupQEFiBottom, n3.DupQEFiBottom, n0.Corner2Voxel.MaterialIndex);
                }

                // Skirt borders
                if (n0 != n1 && n0.IsBorderBack && n1.IsBorderBack && n0.Corner1Voxel.IsInside && (!n0.Corner0Voxel.IsInside || !n0.Corner3Voxel.IsInside || !n1.Corner1Voxel.IsInside || !n1.Corner2Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 0);
                    CheckQEFiEdge(n1, 0);
                    CheckQEFiBottom(n0);
                    CheckQEFiBottom(n1);
                    AddQuad(n0.DupEdgesVerticesIndexes(0), n1.DupEdgesVerticesIndexes(0), n1.DupQEFiBottom, n0.DupQEFiBottom, n0.Corner1Voxel.MaterialIndex);
                }

                if (n1 != n2 && n1.IsBorderRight && n2.IsBorderRight && n1.Corner2Voxel.IsInside && (!n1.Corner1Voxel.IsInside || !n1.Corner0Voxel.IsInside || !n2.Corner2Voxel.IsInside || !n2.Corner3Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 1);
                    CheckQEFiEdge(n2, 1);
                    CheckQEFiBottom(n1);
                    CheckQEFiBottom(n2);
                    AddQuad(n1.DupEdgesVerticesIndexes(1), n2.DupEdgesVerticesIndexes(1), n2.DupQEFiBottom, n1.DupQEFiBottom, n1.Corner2Voxel.MaterialIndex);
                }

                if (n2 != n3 && n2.IsBorderFront && n3.IsBorderFront && n2.Corner3Voxel.IsInside && (!n2.Corner1Voxel.IsInside || !n2.Corner2Voxel.IsInside || !n3.Corner3Voxel.IsInside || !n3.Corner0Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 2);
                    CheckQEFiEdge(n3, 2);
                    CheckQEFiBottom(n2);
                    CheckQEFiBottom(n3);
                    AddQuad(n2.DupEdgesVerticesIndexes(2), n3.DupEdgesVerticesIndexes(2), n3.DupQEFiBottom, n2.DupQEFiBottom, n2.Corner4Voxel.MaterialIndex);
                }

                if (n3 != n0 && n3.IsBorderLeft && n0.IsBorderLeft && n3.Corner0Voxel.IsInside && (!n3.Corner2Voxel.IsInside || !n3.Corner3Voxel.IsInside || !n0.Corner0Voxel.IsInside || !n0.Corner1Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 8);
                    CheckQEFiEdge(n0, 8);
                    CheckQEFiBottom(n3);
                    CheckQEFiBottom(n0);
                    AddQuad(n3.DupEdgesVerticesIndexes(3), n0.DupEdgesVerticesIndexes(3), n0.DupQEFiBottom, n3.DupQEFiBottom, n3.Corner0Voxel.MaterialIndex);
                }

                //Skirt corners
                if (n0.IsBorderBack && n0.IsBorderLeft && n0.Corner0Voxel.IsInside && (!n0.Corner1Voxel.IsInside || !n0.Corner3Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 3);
                    CheckQEFiEdge(n0, 0);
                    CheckQEFiBottom(n0);
                    CheckQEFiCorner(n0, 0);
                    AddQuad(n0.DupQEFiBottom, n0.DupEdgesVerticesIndexes(3), n0.DupCornersVerticesIndexes(0), n0.DupEdgesVerticesIndexes(0), n0.Corner0Voxel.MaterialIndex);
                }

                if (n1.IsBorderBack && n1.IsBorderRight && n1.Corner1Voxel.IsInside && (!n1.Corner0Voxel.IsInside || !n1.Corner2Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 0);
                    CheckQEFiEdge(n1, 1);
                    CheckQEFiBottom(n1);
                    CheckQEFiCorner(n1, 1);
                    AddQuad(n1.DupQEFiBottom, n1.DupEdgesVerticesIndexes(0), n1.DupCornersVerticesIndexes(1), n1.DupEdgesVerticesIndexes(1), n1.Corner1Voxel.MaterialIndex);
                }

                if (n2.IsBorderFront && n2.IsBorderRight && n2.Corner2Voxel.IsInside && (!n2.Corner1Voxel.IsInside || !n2.Corner3Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 1);
                    CheckQEFiEdge(n2, 2);
                    CheckQEFiBottom(n2);
                    CheckQEFiCorner(n2, 2);
                    AddQuad(n2.DupQEFiBottom, n2.DupEdgesVerticesIndexes(1), n2.DupCornersVerticesIndexes(2), n2.DupEdgesVerticesIndexes(2), n2.Corner2Voxel.MaterialIndex);
                }

                if (n3.IsBorderFront && n3.IsBorderLeft && n3.Corner3Voxel.IsInside && (!n3.Corner0Voxel.IsInside || !n3.Corner2Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 2);
                    CheckQEFiEdge(n3, 3);
                    CheckQEFiBottom(n3);
                    CheckQEFiCorner(n3, 3);
                    AddQuad(n3.DupQEFiBottom, n3.DupEdgesVerticesIndexes(2), n3.DupCornersVerticesIndexes(3), n3.DupEdgesVerticesIndexes(3), n3.Corner3Voxel.MaterialIndex);
                }
            }
        }

        private void CreateSkirtsTop(int minLevel, OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            if (n0.IsBorderTop && n1.IsBorderTop && n2.IsBorderTop && n3.IsBorderTop) {
                // Skirt edge
                bool isCenterInside;
                if (n0.Level == minLevel) {
                    isCenterInside = n0.Corner6Voxel.IsInside;
                } else if (n1.Level == minLevel) {
                    isCenterInside = n1.Corner7Voxel.IsInside;
                } else if (n2.Level == minLevel) {
                    isCenterInside = n2.Corner4Voxel.IsInside;
                } else {
                    isCenterInside = n3.Corner5Voxel.IsInside;
                }

                if (isCenterInside && (!n0.Corner4Voxel.IsInside || !n1.Corner5Voxel.IsInside || !n2.Corner6Voxel.IsInside || !n3.Corner7Voxel.IsInside)) {
                    CheckQEFiTop(n0);
                    CheckQEFiTop(n1);
                    CheckQEFiTop(n2);
                    CheckQEFiTop(n3);
                    AddQuad(n0.DupQEFiTop, n3.DupQEFiTop, n2.DupQEFiTop, n1.DupQEFiTop, n0.Corner6Voxel.MaterialIndex);
                }

                // Skirt borders
                if (n0 != n1 && n0.IsBorderBack && n1.IsBorderBack && n0.Corner5Voxel.IsInside && (!n0.Corner4Voxel.IsInside || !n0.Corner7Voxel.IsInside || !n1.Corner5Voxel.IsInside || !n1.Corner6Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 4);
                    CheckQEFiEdge(n1, 4);
                    CheckQEFiTop(n0);
                    CheckQEFiTop(n1);
                    AddQuad(n0.DupEdgesVerticesIndexes(4), n0.DupQEFiTop, n1.DupQEFiTop, n1.DupEdgesVerticesIndexes(4), n0.Corner5Voxel.MaterialIndex);
                }

                if (n1 != n2 && n1.IsBorderRight && n2.IsBorderRight && n1.Corner6Voxel.IsInside && (!n1.Corner5Voxel.IsInside || !n1.Corner4Voxel.IsInside || !n2.Corner6Voxel.IsInside || !n2.Corner7Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 5);
                    CheckQEFiEdge(n2, 5);
                    CheckQEFiTop(n1);
                    CheckQEFiTop(n2);
                    AddQuad(n1.DupEdgesVerticesIndexes(5), n1.DupQEFiTop, n2.DupQEFiTop, n2.DupEdgesVerticesIndexes(5), n1.Corner6Voxel.MaterialIndex);
                }

                if (n2 != n3 && n2.IsBorderFront && n3.IsBorderFront && n2.Corner7Voxel.IsInside && (!n2.Corner5Voxel.IsInside || !n2.Corner6Voxel.IsInside || !n3.Corner7Voxel.IsInside || !n3.Corner4Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 6);
                    CheckQEFiEdge(n3, 6);
                    CheckQEFiTop(n2);
                    CheckQEFiTop(n3);
                    AddQuad(n2.DupEdgesVerticesIndexes(6), n2.DupQEFiTop, n3.DupQEFiTop, n3.DupEdgesVerticesIndexes(6), n2.Corner4Voxel.MaterialIndex);
                }

                if (n3 != n0 && n3.IsBorderLeft && n0.IsBorderLeft && n3.Corner4Voxel.IsInside && (!n3.Corner6Voxel.IsInside || !n3.Corner7Voxel.IsInside || !n0.Corner4Voxel.IsInside || !n0.Corner5Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 8);
                    CheckQEFiEdge(n0, 8);
                    CheckQEFiTop(n3);
                    CheckQEFiTop(n0);
                    AddQuad(n3.DupEdgesVerticesIndexes(7), n3.DupQEFiTop, n0.DupQEFiTop, n0.DupEdgesVerticesIndexes(7), n3.Corner4Voxel.MaterialIndex);
                }

                //Skirt corners
                if (n0.IsBorderBack && n0.IsBorderLeft && n0.Corner4Voxel.IsInside && (!n0.Corner5Voxel.IsInside || !n0.Corner7Voxel.IsInside)) {
                    CheckQEFiEdge(n0, 7);
                    CheckQEFiEdge(n0, 4);
                    CheckQEFiTop(n0);
                    CheckQEFiCorner(n0, 4);
                    AddQuad(n0.DupQEFiTop, n0.DupEdgesVerticesIndexes(4), n0.DupCornersVerticesIndexes(4), n0.DupEdgesVerticesIndexes(7), n0.Corner4Voxel.MaterialIndex);
                }

                if (n1.IsBorderBack && n1.IsBorderRight && n1.Corner5Voxel.IsInside && (!n1.Corner4Voxel.IsInside || !n1.Corner6Voxel.IsInside)) {
                    CheckQEFiEdge(n1, 4);
                    CheckQEFiEdge(n1, 5);
                    CheckQEFiTop(n1);
                    CheckQEFiCorner(n1, 5);
                    AddQuad(n1.DupQEFiTop, n1.DupEdgesVerticesIndexes(5), n1.DupCornersVerticesIndexes(5), n1.DupEdgesVerticesIndexes(4), n1.Corner5Voxel.MaterialIndex);
                }

                if (n2.IsBorderFront && n2.IsBorderRight && n2.Corner6Voxel.IsInside && (!n2.Corner5Voxel.IsInside || !n2.Corner7Voxel.IsInside)) {
                    CheckQEFiEdge(n2, 5);
                    CheckQEFiEdge(n2, 6);
                    CheckQEFiTop(n2);
                    CheckQEFiCorner(n2, 6);
                    AddQuad(n2.DupQEFiTop, n2.DupEdgesVerticesIndexes(6), n2.DupCornersVerticesIndexes(6), n2.DupEdgesVerticesIndexes(5), n2.Corner6Voxel.MaterialIndex);
                }

                if (n3.IsBorderFront && n3.IsBorderLeft && n3.Corner7Voxel.IsInside && (!n3.Corner4Voxel.IsInside || !n3.Corner6Voxel.IsInside)) {
                    CheckQEFiEdge(n3, 6);
                    CheckQEFiEdge(n3, 7);
                    CheckQEFiTop(n3);
                    CheckQEFiCorner(n3, 7);
                    AddQuad(n3.DupQEFiTop, n3.DupEdgesVerticesIndexes(7), n3.DupCornersVerticesIndexes(7), n3.DupEdgesVerticesIndexes(6), n3.Corner7Voxel.MaterialIndex);
                }
            }
        }

        /// <summary>
        ///     Effectively adds the quad by adding triangles to the mesh.
        /// </summary>
        /// <param name="i0">Index of first vertex</param>
        /// <param name="i1">Index of second vertex</param>
        /// <param name="i2">Index of third vertex</param>
        /// <param name="i3">Index of fourth vertex</param>
        /// <param name="submesh">Index of the submesh (ie. material)</param>
        private void AddQuad(int i0, int i1, int i2, int i3, int submesh)
        {
            if (i0 != -1 && i1 != -1 && i2 != -1 && i3 != -1) {
                var indicesSubmesh = indices[submesh];

                if (i0 != i1 && i0 != i2 && i1 != i2) {
                    var vertex0 = mesh.Vertices[i0].Vertex;
                    var vertex1 = mesh.Vertices[i1].Vertex;
                    var vertex2 = mesh.Vertices[i2].Vertex;
                    if (Vector3Utils.DistanceSquared(vertex0, vertex1) > 0.00001f &&
                        Vector3Utils.DistanceSquared(vertex0, vertex2) > 0.00001f &&
                        Vector3Utils.DistanceSquared(vertex1, vertex2) > 0.00001f) {
                        DeepDebug.CheckTriangle(mesh, i0, i1, i2);
                        indicesSubmesh.Add(i0);
                        indicesSubmesh.Add(i1);
                        indicesSubmesh.Add(i2);
                    }
                }

                if (i0 != i3 && i0 != i2 && i3 != i2) {
                    var vertex0 = mesh.Vertices[i0].Vertex;
                    var vertex2 = mesh.Vertices[i2].Vertex;
                    var vertex3 = mesh.Vertices[i3].Vertex;
                    if (Vector3Utils.DistanceSquared(vertex0, vertex3) > 0.00001f &&
                        Vector3Utils.DistanceSquared(vertex0, vertex2) > 0.00001f &&
                        Vector3Utils.DistanceSquared(vertex3, vertex2) > 0.00001f) {
                        DeepDebug.CheckTriangle(mesh, i0, i2, i3);
                        indicesSubmesh.Add(i0);
                        indicesSubmesh.Add(i2);
                        indicesSubmesh.Add(i3);
                    }
                }

                /*if (UP.FindDuplicateTriangle (indicesSubmesh)) {
					UDebug.LogError ("DUPLICATE INDICES!!!!!!!!!!");
				}*/
            }
        }

        /// <summary>
        ///     Chooses the right order of vertices depending on which voxel is inside and adds the quad to the mesh.
        /// </summary>
        /// <param name="v0">V0.</param>
        /// <param name="v1">V1.</param>
        /// <param name="i0">I0.</param>
        /// <param name="i1">I1.</param>
        /// <param name="i2">I2.</param>
        /// <param name="i3">I3.</param>
        private void AddQuad(Voxel v0, Voxel v1, int i0, int i1, int i2, int i3)
        {
            if (v0.IsInside != v1.IsInside) {
                if (v1.IsInside) {
                    AddQuad(i0, i1, i2, i3, v1.MaterialIndex);
                } else {
                    AddQuad(i0, i3, i2, i1, v0.MaterialIndex);
                }
            }
        }


        private void AddQuadSafe(Voxel v0, Voxel v1,
                                 OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
        {
            if (v0.IsInside != v1.IsInside) {
                DeepDebug.CheckVerticesExistence(n0, n1, n2, n3);
                if (n0.QEFi == -1) {
                    // This must never happen. If it is the case, we have to force splitting to compute missed vertex
                    n0.VertexData.CreateMinQEFAtCenter();
                }

                if (n1.QEFi == -1) {
                    // This must never happen. If it is the case, we have to force splitting to compute missed vertex
                    n1.VertexData.CreateMinQEFAtCenter();
                }

                if (n2.QEFi == -1) {
                    // This must never happen. If it is the case, we have to force splitting to compute missed vertex
                    n2.VertexData.CreateMinQEFAtCenter();
                }

                if (n3.QEFi == -1) {
                    // This must never happen. If it is the case, we have to force splitting to compute missed vertex
                    n3.VertexData.CreateMinQEFAtCenter();
                }

                AddQuad(v0, v1, n0.DupQEFi, n1.DupQEFi, n2.DupQEFi, n3.DupQEFi);
            }
        }

        private void CheckQEFi(OctreeNode n)
        {
            if (n.QEFi == -1) {
                n.VertexData.CreateMinQEFAtCenter();
            }
        }

        private void CheckQEFiTop(OctreeNode n)
        {
            if (n.QEFiTop == -1) {
                n.VertexData.CreateMinQEFAtCenterTop();
            }
        }

        private void CheckQEFiBottom(OctreeNode n)
        {
            if (n.QEFiBottom == -1) {
                n.VertexData.CreateMinQEFAtCenterBottom();
            }
        }

        private void CheckQEFiFront(OctreeNode n)
        {
            if (n.QEFiFront == -1) {
                n.VertexData.CreateMinQEFAtCenterFront();
            }
        }

        private void CheckQEFiBack(OctreeNode n)
        {
            if (n.QEFiBack == -1) {
                n.VertexData.CreateMinQEFAtCenterBack();
            }
        }

        private void CheckQEFiRight(OctreeNode n)
        {
            if (n.QEFiRight == -1) {
                n.VertexData.CreateMinQEFAtCenterRight();
            }
        }

        private void CheckQEFiLeft(OctreeNode n)
        {
            if (n.QEFiLeft == -1) {
                n.VertexData.CreateMinQEFAtCenterLeft();
            }
        }

        private void CheckQEFiEdge(OctreeNode n, int edge)
        {
            if (n.EdgesVerticesIndexes[edge] == -1) {
                n.VertexData.CreateMinQEFAtCenterEdge(edge);
            }
        }

        private void CheckQEFiCorner(OctreeNode n, int corner)
        {
            if (n.CornersVerticesIndexes[corner] == -1) {
                n.VertexData.CreateMinQEFAtCorner(corner);
            }
        }

        private static class DeepDebug
        {
            [Conditional("CONTOURING_DEBUG")]
            internal static void CheckTriangle(MeshData mesh, int i1, int i2, int i3)
            {
                var v1 = mesh.Vertices[i1];
                var v2 = mesh.Vertices[i2];
                var v3 = mesh.Vertices[i3];
                if (i1 == i2 || i1 == i3 || i2 == i3) {
                    UDebug.LogError("Triangle with at least two times the same vertex index");
                }

                if (v1 == v2 || v1 == v3 || v2 == v3) {
                    UDebug.LogError("Triangle with at least two times the same vertex");
                }

                if (Vector3.Distance(v1.Vertex, v2.Vertex) < 0.0001f &&
                    Vector3.Distance(v1.Vertex, v3.Vertex) < 0.0001f) {
                    UDebug.LogError("Triangle with all 3 vertices at the same position");
                } else if (Vector3.Distance(v1.Vertex, v2.Vertex) < 0.0001f ||
                           Vector3.Distance(v1.Vertex, v3.Vertex) < 0.0001f ||
                           Vector3.Distance(v2.Vertex, v3.Vertex) < 0.0001f) {
                    UDebug.LogError("Triangle with 2 vertices at the same position");
                }
            }

            [Conditional("CONTOURING_DEBUG")]
            internal static void CheckVerticesExistence(OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3)
            {
                if (n0.QEFi == -1 || n1.QEFi == -1 || n2.QEFi == -1 || n3.QEFi == -1) {
                    // This must never happen. If it is the case, we have to force splitting to compute missed vertex
                    UDebug.LogError("Missing vertex");
                }
            }
        }
    }
}