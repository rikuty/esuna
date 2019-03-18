using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    internal sealed class MeshData
    {
        private readonly List<VertexData> vertices;

        public List<VertexData> Vertices {
            get {
                if (isReadyForToMesh) {
                    UDebug.Fatal("Vertices list cannot be accessed after PrepareToMesh has been called");
                    return null;
                }

                return vertices;
            }
        }

        public readonly List<int>[] Indices;
        public readonly int SubmeshCount;

        private List<Vector3> newVertices;
        private List<Color32> newColors;
        private List<Vector3> newNormals;
        private List<Vector4> newUVs;
        private List<Vector4> newUVs2;
        private List<Vector4> newUVs3;
        private List<Vector4> newUVs4;
        private List<Vector4> newTangents;
        private int actualSubmeshCount;
        private bool isReadyForToMesh;

        internal int VertexCount {
            get { return isReadyForToMesh ? newVertices.Count : Vertices.Count; }
        }

        internal bool HasTriangles {
            get {
                if (VertexCount == 0) {
                    // quick win: no vertex, no triangle!
                    return false;
                }

                for (var i = 0; i < SubmeshCount; ++i) {
                    if (Indices[i].Count > 0) {
                        return true;
                    }
                }

                return false;
            }
        }

        internal MeshData(int subMeshCount, int initialListsSize)
        {
            vertices = new List<VertexData>(initialListsSize);
            Indices = new List<int>[subMeshCount];
            for (var i = 0; i < subMeshCount; ++i) {
                Indices[i] = new List<int>(initialListsSize);
            }

            SubmeshCount = subMeshCount;
            isReadyForToMesh = false;
        }

        internal List<int> GetIndices(int index)
        {
            return Indices[index];
        }

        private void ClearLists(UnsafePool<VertexData> vertexDataPool)
        {
            vertexDataPool.BulkFree(vertices, vertices.Count);
            vertices.Clear();

            for (var i = 0; i < SubmeshCount; ++i) {
                Indices[i].Clear();
            }
        }

        internal void Free(ChunkGeneratorPools chunkGeneratorPools, MeshDataListsPools meshDataListsPools, MeshDataPool meshDataPool)
        {
            isReadyForToMesh = false;
            ClearLists(chunkGeneratorPools.VertexDataPool);
            FreeToMeshLists(meshDataListsPools);
            meshDataPool.Free(this);
        }

        private void FreeToMeshLists(MeshDataListsPools meshDataListsPools)
        {
            if (newVertices != null) {
                meshDataListsPools.FreeListVector3(newVertices);
                newVertices = null;
            }

            if (newColors != null) {
                meshDataListsPools.FreeListColor(newColors);
                newColors = null;
            }

            if (newNormals != null) {
                meshDataListsPools.FreeListVector3(newNormals);
                newNormals = null;
            }

            if (newUVs != null) {
                meshDataListsPools.FreeListVector4(newUVs);
                newUVs = null;
            }

            if (newUVs2 != null) {
                meshDataListsPools.FreeListVector4(newUVs2);
                newUVs2 = null;
            }

            if (newUVs3 != null) {
                meshDataListsPools.FreeListVector4(newUVs3);
                newUVs3 = null;
            }

            if (newUVs4 != null) {
                meshDataListsPools.FreeListVector4(newUVs4);
                newUVs4 = null;
            }

            if (newTangents != null) {
                meshDataListsPools.FreeListVector4(newTangents);
                newTangents = null;
            }
        }

        internal void PrepareToMesh(MeshDataListsPools meshDataListsPools, UnsafePool<VertexData> vertexDataPool, VoxelTypeSet voxelTypeSet, MegaSplatMeshProcessor megaSplatMeshProcessor)
        {
            UProfiler.Begin("PrepareToMesh");

            if (isReadyForToMesh) {
                UDebug.LogError("PrepareToMesh cannot be called twice on the same MeshData (unless it is reset)");
            }

            FreeToMeshLists(meshDataListsPools);

            var count = Vertices.Count;
            if (count == 0) {
                return;
            }

            UProfiler.Begin("Get lists from pool");
            newVertices = meshDataListsPools.GetListVector3(count);
            newColors = meshDataListsPools.GetListColor(count);
            newNormals = meshDataListsPools.GetListVector3(count);
            newUVs = meshDataListsPools.GetListVector4(count);
            newUVs2 = meshDataListsPools.GetListVector4(count);
            newUVs3 = meshDataListsPools.GetListVector4(count);
            newUVs4 = meshDataListsPools.GetListVector4(count);
            newTangents = meshDataListsPools.GetListVector4(count);
            UProfiler.End();

            UProfiler.Begin("Fill lists");
            for (var i = 0; i < count; ++i) {
                var v = Vertices[i];
                newVertices.Add(v.Vertex);
                newColors.Add(v.Color);
                newNormals.Add(v.Normal);
                newUVs.Add(v.Uv);
                newUVs2.Add(v.Uv2);
                newUVs3.Add(v.Uv3);
                newUVs4.Add(v.Uv4);
                newTangents.Add(v.Tangent);
            }

            UProfiler.End();

            UProfiler.Begin("Compute actual submesh count");
            actualSubmeshCount = 0;
            for (var i = 0; i < SubmeshCount; ++i) {
                if (Indices[i].Count > 0) {
                    ++actualSubmeshCount;
                    if (voxelTypeSet != null && voxelTypeSet.MaterialInfos[i].IsMegaSplat && megaSplatMeshProcessor != null) {
                        UProfiler.Begin("Processing MegaSplat mesh");
                        megaSplatMeshProcessor.Process(newVertices, Indices[i], newColors, newNormals, newUVs, newUVs2, newUVs3, newUVs4, newTangents);
                        UProfiler.End();
                    }
                }
            }

            UProfiler.End();

            isReadyForToMesh = true;

            vertexDataPool.BulkFree(vertices, vertices.Count);
            vertices.Clear();

            UProfiler.End();
        }

        internal void ToMesh(ref Mesh mesh, out int materialsMask, Bounds bounds)
        {
            if (!isReadyForToMesh) {
                UDebug.LogError("MeshData is no ready for ToMesh conversion");
            }

            materialsMask = 0;
            if (mesh != null) {
                mesh.Clear();
            }

            if (newVertices.Count == 0) {
                return;
            }

            if (mesh == null) {
                mesh = ChunkObjectPool.MeshPool.Get();
                mesh.MarkDynamic();
            }

            mesh.SetVertices(newVertices);
            mesh.SetColors(newColors);
            mesh.SetNormals(newNormals);
            mesh.SetUVs(0, newUVs);
            mesh.SetUVs(1, newUVs2);
            mesh.SetUVs(2, newUVs3);
            mesh.SetUVs(3, newUVs4);
            mesh.SetTangents(newTangents);

            mesh.subMeshCount = actualSubmeshCount;
            var mIndex = 0;
            for (var i = 0; i < SubmeshCount; ++i) {
                var ind = Indices[i];
                if (ind.Count > 0) {
                    materialsMask |= 1 << i;
                    mesh.SetTriangles(ind, mIndex++);
                }
            }

            mesh.bounds = bounds;
        }

        public bool Raycast(Ray ray, float distance, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            hitPoint = Vector3.zero;
            hitNormal = Vector3.zero;
            for (var sub = 0; sub < SubmeshCount; ++sub) {
                var indices = GetIndices(sub);
                var indexCount = indices.Count;
                for (var i = 0; i < indexCount; i += 3) {
                    // get the three vertices that make the triangle
                    var v0 = isReadyForToMesh ? newVertices[indices[i + 0]] : Vertices[indices[i + 0]].Vertex;
                    var v1 = isReadyForToMesh ? newVertices[indices[i + 1]] : Vertices[indices[i + 1]].Vertex;
                    var v2 = isReadyForToMesh ? newVertices[indices[i + 2]] : Vertices[indices[i + 2]].Vertex;

                    // raycast on that triangle
                    if (Vector3Utils.Intersect3DRayTriangle(ray, v0, v1, v2, distance, out hitPoint, out hitNormal))
                        return true;
                }
            }

            return false;
        }
    }
}