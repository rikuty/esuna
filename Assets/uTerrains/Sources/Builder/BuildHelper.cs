using System;
using UnityEngine;

namespace UltimateTerrains
{
    internal static class BuildHelper
    {
        /// <summary>
        ///     Compute normals of the mesh.
        /// </summary>
        /// <param name="mesh"> The mesh on which normals will be computed.</param>
        /// <param name="chunk"> The chunk of the mesh.</param>
        internal static void SolveNormals(MeshData mesh, MeshDataListsPools pool, int chunkLevel)
        {
            var vertexCount = mesh.VertexCount;

            UProfiler.Begin("Get Vector3 list from pool");
            var normals = pool.GetListVector3(vertexCount);
            normals.Clear();
            for (var i = 0; i < mesh.VertexCount; i++) {
                normals.Add(Vector3.zero);
            }

            UProfiler.End();

            for (var sub = 0; sub < mesh.SubmeshCount; ++sub) {
                var indices = mesh.GetIndices(sub);
                var indexCount = indices.Count;
                for (var i = 0; i < indexCount; i += 3) {
                    var i1 = indices[i + 0];
                    var i2 = indices[i + 1];
                    var i3 = indices[i + 2];

                    // get the three vertices that make the faces
                    var p1 = mesh.Vertices[i1].Vertex;
                    var p2 = mesh.Vertices[i2].Vertex;
                    var p3 = mesh.Vertices[i3].Vertex;

                    var normal = Vector3.Cross(p2 - p1, p3 - p1);
                    normals[i1] += normal;
                    normals[i2] += normal;
                    normals[i3] += normal;
                }
            }

            var lvlCoef = 0.25 * UnitConverter.LevelInverse(chunkLevel);
            for (var i = 0; i < vertexCount; ++i) {
                var v = mesh.Vertices[i];
                v.SetNormal(Vector3.Lerp(v.Normal, normals[i].normalized, v.Blockiness + (float) lvlCoef).normalized);
                normals[i] = Vector3.zero;
            }

            pool.FreeListVector3(normals);
        }

        // Compute tangents
        internal static void SolveTangents(MeshData mesh, MeshDataListsPools pool)
        {
            var vertexCount = mesh.VertexCount;

            UProfiler.Begin("Get Vector3 lists from pool");
            var tan1 = pool.GetListVector3D(mesh.VertexCount);
            var tan2 = pool.GetListVector3D(mesh.VertexCount);
            tan1.Clear();
            tan2.Clear();
            for (var i = 0; i < mesh.VertexCount; i++) {
                tan1.Add(Vector3d.zero);
                tan2.Add(Vector3d.zero);
            }

            UProfiler.End();

            for (var sub = 0; sub < mesh.SubmeshCount; ++sub) {
                var indices = mesh.GetIndices(sub);
                for (var a = 0; a < indices.Count; a += 3) {
                    var i1 = indices[a + 0];
                    var i2 = indices[a + 1];
                    var i3 = indices[a + 2];

                    var vd1 = mesh.Vertices[i1];
                    var vd2 = mesh.Vertices[i2];
                    var vd3 = mesh.Vertices[i3];

                    var v1 = (Vector3d) vd1.Vertex;
                    var v2 = (Vector3d) vd2.Vertex;
                    var v3 = (Vector3d) vd3.Vertex;

                    var w1 = vd1.Uv;
                    var w2 = vd2.Uv;
                    var w3 = vd3.Uv;

                    var x1 = v2.x - v1.x;
                    var x2 = v3.x - v1.x;
                    var y1 = v2.y - v1.y;
                    var y2 = v3.y - v1.y;
                    var z1 = v2.z - v1.z;
                    var z2 = v3.z - v1.z;

                    double s1 = w2.x - w1.x;
                    double s2 = w3.x - w1.x;
                    double t1 = w2.y - w1.y;
                    double t2 = w3.y - w1.y;
                    if (UMath.Approximately(t1, 0.0, 0.001) && UMath.Approximately(t2, 0.0, 0.001)) {
                        t1 = w2.x - w1.x;
                        t2 = w3.x - w1.x;
                        s1 = w2.y - w1.y;
                        s2 = w3.y - w1.y;
                    }

                    var div = s1 * t2 - s2 * t1;

                    var r = -0.001 <= div && div <= 0.001 ? 1000.0 : 1.0 / div;

                    var sdir = new Vector3d((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                    var tdir = new Vector3d((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                    tan1[i1] += sdir;
                    tan1[i2] += sdir;
                    tan1[i3] += sdir;

                    tan2[i1] += tdir;
                    tan2[i2] += tdir;
                    tan2[i3] += tdir;
                }
            }


            Vector3d n, t, tmp;
            double w;
            for (var a = 0; a < vertexCount; ++a) {
                var v = mesh.Vertices[a];
                n = (Vector3d) v.Normal;
                t = tan1[a];

                tmp = (t - n * Vector3d.Dot(n, t)).Normalized;
                if (Vector3d.Dot(Vector3d.Cross(n, t), tan2[a]) < 0) {
                    w = -1.0;
                } else {
                    w = 1.0;
                }

                v.Tangent = new Vector4((float) tmp.x, (float) tmp.y, (float) tmp.z, (float) w);

                tan1[a] = Vector3d.zero;
                tan2[a] = Vector3d.zero;
            }

            // Free arrays for pooling
            pool.FreeListVector3D(tan1);
            pool.FreeListVector3D(tan2);
        }
    }
}