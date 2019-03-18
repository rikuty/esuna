using System;
using UnityEngine;

namespace UltimateTerrains
{
    internal sealed class MarchingCubesQEF
    {
        private readonly Voxel[] gridVoxel = new Voxel[8];
        private readonly Vector3d[] vertices = new Vector3d[12];
        private readonly Vector3d[] normals = new Vector3d[12];
        private readonly bool[] edgeHasSignChange = new bool[12];
        private readonly EdgeGradientCache edgeXGradientCache = new EdgeGradientCache();
        private readonly EdgeGradientCache edgeYGradientCache = new EdgeGradientCache();
        private readonly EdgeGradientCache edgeZGradientCache = new EdgeGradientCache();
        private readonly VoxelGenerator voxelGenerator;
        private readonly VoxelTypeSet voxelTypeSet;
        private int nbIntersect;
        private Vector3d massPoint;
        private Vector3d minQEFNormal;
        private VoxelType vType;
        private float blockiness;

        public MarchingCubesQEF(VoxelGenerator voxelGenerator, VoxelTypeSet voxelTypeSet)
        {
            this.voxelGenerator = voxelGenerator;
            this.voxelTypeSet = voxelTypeSet;
        }

        public Vector3d From {
            get { return gridVoxel[0].RealPosition; }
        }

        public Vector3d To {
            get { return gridVoxel[6].RealPosition; }
        }

        public Vector3d Center {
            get { return (From + To) * 0.5; }
        }

        public VoxelType QEFVoxelType {
            get { return vType; }
        }

        public Vector3d MassPoint {
            get { return massPoint; }
        }

        public float Blockiness {
            get { return blockiness; }
        }

        public Vector3d QEFVertexNormal {
            get { return minQEFNormal; }
        }

        public Vector3d[] EdgeVertices {
            get { return vertices; }
        }

        public Vector3d[] EdgeNormals {
            get { return normals; }
        }

        public bool[] EdgeSignChanges {
            get { return edgeHasSignChange; }
        }

        public void SetVoxel(int i, Voxel voxel)
        {
            gridVoxel[i] = voxel;
        }

        public Voxel GetVoxel(int i)
        {
            return gridVoxel[i];
        }

        public void ResetCache()
        {
            edgeXGradientCache.ResetCache();
            edgeYGradientCache.ResetCache();
            edgeZGradientCache.ResetCache();
        }

        /*
			       Given a grid cell and an calculate the triangular
			       facets required to represent the isosurface through the cell.
			       Return the number of triangular facets, the array "triangles"
			       will be loaded up with the vertices at most 5 triangular facets.
			        0 will be returned if the grid cell is either totally above
			       of totally below the isolevel.
			         4---------5
			        /|        /|
			       / |       / |
			      7--0------6--1
			      | /       | /
			      |/        |/
			      3---------2

			    */
        public int ComputeIntersections()
        {
            // Determine the index into the edge table which
            // tells us which vertices are inside of the surface
            var cubeindex = 0;

            if (gridVoxel[0].IsInside)
                cubeindex |= 1;
            if (gridVoxel[1].IsInside)
                cubeindex |= 2;
            if (gridVoxel[2].IsInside)
                cubeindex |= 4;
            if (gridVoxel[3].IsInside)
                cubeindex |= 8;
            if (gridVoxel[4].IsInside)
                cubeindex |= 16;
            if (gridVoxel[5].IsInside)
                cubeindex |= 32;
            if (gridVoxel[6].IsInside)
                cubeindex |= 64;
            if (gridVoxel[7].IsInside)
                cubeindex |= 128;

            // Cube is entirely in/out of the surface
            if (cubeindex == 0 || cubeindex == 255)
                return 0;

            // Determines submesh and vertex color, harmonizing it with neighbour
            blockiness = 0f;
            ColorAndBlockinessInterp();

            /* Find the vertices where the surface intersects the cube */
            var edge = MarchingCubesTables.McEdges[cubeindex];
            Vector3d vertex;
            Vector3d normal;
            for (var i = 0; i < 12; ++i) {
                edgeHasSignChange[i] = false;
            }

            massPoint = Vector3d.zero;
            minQEFNormal = Vector3d.zero;
            nbIntersect = 0;

            if ((edge & 1) != 0) {
                ++nbIntersect;
                if (!edgeXGradientCache.TryGet(gridVoxel[0].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[0], gridVoxel[1], out normal);
                    edgeXGradientCache.Cache(gridVoxel[0].Position, vertex, normal);
                }

                edgeHasSignChange[0] = true;
                vertices[0] = vertex;
                normals[0] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 2) != 0) {
                ++nbIntersect;
                if (!edgeZGradientCache.TryGet(gridVoxel[1].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[1], gridVoxel[2], out normal);
                    edgeZGradientCache.Cache(gridVoxel[1].Position, vertex, normal);
                }

                edgeHasSignChange[1] = true;
                vertices[1] = vertex;
                normals[1] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 4) != 0) {
                ++nbIntersect;
                if (!edgeXGradientCache.TryGet(gridVoxel[3].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[2], gridVoxel[3], out normal);
                    edgeXGradientCache.Cache(gridVoxel[3].Position, vertex, normal);
                }

                edgeHasSignChange[2] = true;
                vertices[2] = vertex;
                normals[2] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 8) != 0) {
                ++nbIntersect;
                if (!edgeZGradientCache.TryGet(gridVoxel[0].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[3], gridVoxel[0], out normal);
                    edgeZGradientCache.Cache(gridVoxel[0].Position, vertex, normal);
                }

                edgeHasSignChange[3] = true;
                vertices[3] = vertex;
                normals[3] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 16) != 0) {
                ++nbIntersect;
                if (!edgeXGradientCache.TryGet(gridVoxel[4].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[4], gridVoxel[5], out normal);
                    edgeXGradientCache.Cache(gridVoxel[4].Position, vertex, normal);
                }

                edgeHasSignChange[4] = true;
                vertices[4] = vertex;
                normals[4] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 32) != 0) {
                ++nbIntersect;
                if (!edgeZGradientCache.TryGet(gridVoxel[5].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[5], gridVoxel[6], out normal);
                    edgeZGradientCache.Cache(gridVoxel[5].Position, vertex, normal);
                }

                edgeHasSignChange[5] = true;
                vertices[5] = vertex;
                normals[5] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 64) != 0) {
                ++nbIntersect;
                if (!edgeXGradientCache.TryGet(gridVoxel[7].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[6], gridVoxel[7], out normal);
                    edgeXGradientCache.Cache(gridVoxel[7].Position, vertex, normal);
                }

                edgeHasSignChange[6] = true;
                vertices[6] = vertex;
                normals[6] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 128) != 0) {
                ++nbIntersect;
                if (!edgeZGradientCache.TryGet(gridVoxel[4].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[7], gridVoxel[4], out normal);
                    edgeZGradientCache.Cache(gridVoxel[4].Position, vertex, normal);
                }

                edgeHasSignChange[7] = true;
                vertices[7] = vertex;
                normals[7] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 256) != 0) {
                ++nbIntersect;
                if (!edgeYGradientCache.TryGet(gridVoxel[0].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[0], gridVoxel[4], out normal);
                    edgeYGradientCache.Cache(gridVoxel[0].Position, vertex, normal);
                }

                edgeHasSignChange[8] = true;
                vertices[8] = vertex;
                normals[8] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 512) != 0) {
                ++nbIntersect;
                if (!edgeYGradientCache.TryGet(gridVoxel[1].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[1], gridVoxel[5], out normal);
                    edgeYGradientCache.Cache(gridVoxel[1].Position, vertex, normal);
                }

                edgeHasSignChange[9] = true;
                vertices[9] = vertex;
                normals[9] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 1024) != 0) {
                ++nbIntersect;
                if (!edgeYGradientCache.TryGet(gridVoxel[2].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[2], gridVoxel[6], out normal);
                    edgeYGradientCache.Cache(gridVoxel[2].Position, vertex, normal);
                }

                edgeHasSignChange[10] = true;
                vertices[10] = vertex;
                normals[10] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            if ((edge & 2048) != 0) {
                ++nbIntersect;
                if (!edgeYGradientCache.TryGet(gridVoxel[3].Position, out vertex, out normal)) {
                    vertex = VertexInterp(gridVoxel[3], gridVoxel[7], out normal);
                    edgeYGradientCache.Cache(gridVoxel[3].Position, vertex, normal);
                }

                edgeHasSignChange[11] = true;
                vertices[11] = vertex;
                normals[11] = normal;
                minQEFNormal += normal;
                massPoint += vertex;
            }

            minQEFNormal = minQEFNormal.Normalized;
            massPoint /= nbIntersect;

            return nbIntersect;
        }


        //-----------------------------------------------------------------------

        // Compute vertex position and normal between voxels for MC & MS.
        // Linearly interpolate the position where an isosurface cuts
        // an edge between two vertices, each with their own scalar value
        private Vector3d VertexInterp(Voxel b1, Voxel b2, out Vector3d normal)
        {
            const double nextToVoxel = 1e-5;

            if (b1.Value < nextToVoxel && b1.Value > -nextToVoxel) {
                normal = voxelGenerator.ComputeGradientInChunk(b1.RealPosition);
                return b1.RealPosition;
            }

            if (b2.Value < nextToVoxel && b2.Value > -nextToVoxel) {
                normal = voxelGenerator.ComputeGradientInChunk(b2.RealPosition);
                return b2.RealPosition;
            }

            // mu = (Const.ISOLEVEL - valp1) / (valp2 - valp1);
            var mu = b1.Value / (b1.Value - b2.Value);
            var p1 = b1.RealPosition;
            var p2 = b2.RealPosition;
            var position = new Vector3d(p1.x + mu * (p2.x - p1.x), p1.y + mu * (p2.y - p1.y), p1.z + mu * (p2.z - p1.z));

            normal = voxelGenerator.ComputeGradientInChunk(position);

            return position;
        }

        private void ColorAndBlockinessInterp()
        {
            var priority = int.MinValue;
            var overridenPriority = 0;
            for (var i = 0; i < gridVoxel.Length; ++i) {
                if (gridVoxel[i].OverrideVoxelTypePriority > overridenPriority) {
                    overridenPriority = gridVoxel[i].OverrideVoxelTypePriority;
                    var voxelType = voxelTypeSet.GetVoxelType(gridVoxel[i].VoxelTypeIndex);
                    vType = voxelType;
                    blockiness = voxelType.Blockiness;
                } else if (overridenPriority == 0) {
                    var voxelType = voxelTypeSet.GetVoxelType(gridVoxel[i].VoxelTypeIndex);
                    if (voxelType.Priority > priority) {
                        priority = voxelType.Priority;
                        vType = voxelType;
                        blockiness = voxelType.Blockiness;
                    }
                }
            }
        }
    }
}