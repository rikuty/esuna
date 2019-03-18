using LibNoise;
using LibNoise.Primitive;
using UnityEngine;

namespace UltimateTerrains
{
    internal class GrassGenerator
    {
        private const float COS_30 = 0.86603f;
        private const float SIN_30 = 0.5f;
        private const float TAN_ACOS_08 = 0.75f;
        private const int MAX_VERTICES_PER_CHILD = 16384;
        private const float FACTOR = 0.75f;
        private const float GRASS_WHITENOISE_FREQUENCY = 100f;

        private static readonly SimplexPerlin simplex = new SimplexPerlin(PrimitiveModule.DefaultSeed, PrimitiveModule.DefaultQuality);

        private readonly Param param;
        private readonly UnitConverter unitConverter;
        private readonly VoxelTypeSet voxelTypeSet;

        internal GrassGenerator(UltimateTerrain terrain)
        {
            param = terrain.Params;
            unitConverter = terrain.Converter;
            voxelTypeSet = terrain.VoxelTypeSet;
        }

        internal void GenerateGrassMesh(bool genGrass, out MeshData grassMesh, bool genDetails, out Details details, Vector3i chunkWorldPos, int chunkLevel, MeshData srcMesh, ChunkGeneratorPools pools)
        {
            UProfiler.Begin("Get from pool");
            grassMesh = null;
            if (genGrass) {
                grassMesh = pools.GrassMeshDataPool.Get();
            }

            details = null;
            if (genDetails) {
                details = pools.DetailsPool.Get();
            }

            UProfiler.End();
            var chunkUnityWorldPos = unitConverter.VoxelToUnityPosition(chunkWorldPos);

            for (var sub = 0; sub < srcMesh.SubmeshCount; ++sub) {
                var indices = srcMesh.GetIndices(sub);
                var indexCount = indices.Count;
                for (var i = 0; i < indexCount; i += 3) {
                    // get the three vertices that make the faces
                    var i1 = indices[i + 0];
                    var i2 = indices[i + 1];
                    var i3 = indices[i + 2];
                    var vd1 = srcMesh.Vertices[i1];
                    var vd2 = srcMesh.Vertices[i2];
                    var vd3 = srcMesh.Vertices[i3];
                    var vType = GetVoxelType(vd1, vd2, vd3);
                    var minNY = GetMinNormalY(vd1, vd2, vd3);
                    var maxNY = GetMaxNormalY(vd1, vd2, vd3);

                    if (genGrass && vType.IsGrassEnabled) {
                        var grassParam = vType.GrassParam;
                        if (minNY > grassParam.MinNormalY) {
                            var p1 = vd1.Vertex;
                            var p2 = vd2.Vertex;
                            var p3 = vd3.Vertex;

                            // world coordinates
                            var pos1 = p1 + chunkWorldPos * param.SizeXVoxelF;

                            // Height noise
                            var noise = (simplex.GetValue(pos1.x * grassParam.NoiseFrequency, pos1.z * grassParam.NoiseFrequency) + 1.0) * 0.5;

                            // Compute height
                            var height = minNY * grassParam.BaseHeight * noise;
                            if (height < grassParam.MinHeight)
                                continue;

                            // Finally, create grass mesh
                            // One grass per triangle
                            GenerateGrassInPoly(pools.VertexDataPool, grassMesh, grassParam, p1, p2, p3, height, chunkUnityWorldPos);
                        }
                    }

                    if (genDetails) {
                        for (var d = 0; d < vType.DetailsCount; ++d) {
                            if (!vType.EnabledDetails[d])
                                continue;

                            var detailsParam = param.DetailsParams[d];
                            if (chunkLevel > param.MaxLevel)
                                continue;

                            if (minNY >= detailsParam.MinNormalY && maxNY <= detailsParam.MaxNormalY) {
                                DetailsGenerator.AddDetailsToPolygon(details, detailsParam, vd1.Vertex, vd2.Vertex, vd3.Vertex, chunkUnityWorldPos);
                            }
                        }
                    }
                }
            }

            if (genGrass) {
                if (grassMesh.VertexCount > MAX_VERTICES_PER_CHILD) {
                    UDebug.LogWarning("Generated " + grassMesh.VertexCount + " vertices for grass for a single chunk! It should remain lesser than " + MAX_VERTICES_PER_CHILD + " to keep good performance.");
                }

                grassMesh.PrepareToMesh(pools.GrassMeshDataListsPools, pools.VertexDataPool, null, null);
            }
        }

        private float GetMinNormalY(VertexData vd1, VertexData vd2, VertexData vd3)
        {
            var minNY = vd1.Normal.y;
            if (vd2.Normal.y < minNY)
                minNY = vd2.Normal.y;
            if (vd3.Normal.y < minNY)
                minNY = vd3.Normal.y;
            return minNY;
        }

        private float GetMaxNormalY(VertexData vd1, VertexData vd2, VertexData vd3)
        {
            var maxNY = vd1.Normal.y;
            if (vd2.Normal.y > maxNY)
                maxNY = vd2.Normal.y;
            if (vd3.Normal.y > maxNY)
                maxNY = vd3.Normal.y;
            return maxNY;
        }

        private void GenerateGrassInPoly(UnsafePool<VertexData> vertexDataPool, MeshData mesh, GrassParam grassParam, Vector3 p1, Vector3 p2, Vector3 p3, double height, Vector3d chunkUnityWorldPos)
        {
            var minX = p1.x;
            if (p2.x < minX)
                minX = p2.x;
            if (p3.x < minX)
                minX = p3.x;

            var minY = p1.y;
            if (p2.y < minY)
                minY = p2.y;
            if (p3.y < minY)
                minY = p3.y;

            var minZ = p1.z;
            if (p2.z < minZ)
                minZ = p2.z;
            if (p3.z < minZ)
                minZ = p3.z;

            var maxX = p1.x;
            if (p2.x > maxX)
                maxX = p2.x;
            if (p3.x > maxX)
                maxX = p3.x;

            var maxY = p1.y;
            if (p2.y > maxY)
                maxY = p2.y;
            if (p3.y > maxY)
                maxY = p3.y;

            var maxZ = p1.z;
            if (p2.z > maxZ)
                maxZ = p2.z;
            if (p3.z > maxZ)
                maxZ = p3.z;

            var minDx = (minX + chunkUnityWorldPos.x) * grassParam.GrassDensityXInverse;
            var minNx = (int) minDx;
            if (minNx > 0 && minDx - minNx < 0.001f)
                minNx++;
            if (minNx < 0)
                minNx--;

            var minDz = (minZ + chunkUnityWorldPos.z) * grassParam.GrassDensityZInverse;
            var minNz = (int) minDz;
            if (minNz > 0 && minDz - minNz < 0.001f)
                minNz++;
            if (minNz < 0)
                minNz--;

            var maxDx = (maxX + chunkUnityWorldPos.x) * grassParam.GrassDensityXInverse;
            var maxNx = (int) maxDx;
            if (maxNx > 0 && maxDx - maxNx < 0.001f)
                maxNx++;
            if (maxNx < 0)
                maxNx--;

            var maxDz = (maxZ + chunkUnityWorldPos.z) * grassParam.GrassDensityZInverse;
            var maxNz = (int) maxDz;
            if (maxNz > 0 && maxDz - maxNz < 0.001f)
                maxNz++;
            if (maxNz < 0)
                maxNz--;

            Vector3 hitPoint, hitNormal;
            Vector3d m;
            for (var nx = minNx; nx <= maxNx; ++nx) {
                for (var nz = minNz; nz <= maxNz; ++nz) {
                    m.x = nx * grassParam.GrassDensityX - chunkUnityWorldPos.x;
                    m.y = maxY + 1f;
                    m.z = nz * grassParam.GrassDensityZ - chunkUnityWorldPos.z;
                    var whiteNoise = simplex.GetValue(m.x * GRASS_WHITENOISE_FREQUENCY, m.z * GRASS_WHITENOISE_FREQUENCY);
                    m.x += whiteNoise * grassParam.GrassDensityX * grassParam.Dissemination;
                    m.z += whiteNoise * grassParam.GrassDensityZ * grassParam.Dissemination;
                    if (Vector3Utils.Intersect3DRayTriangle(new Ray((Vector3) m, Vector3.down),
                                                            p1, p2, p3, maxY - minY + 2f,
                                                            out hitPoint, out hitNormal)) {
                        Generate3Blades(grassParam, vertexDataPool, mesh, hitPoint, height);
                    }
                }
            }
        }


        private void Generate3Blades(GrassParam param, UnsafePool<VertexData> vertexDataPool, MeshData mesh, Vector3 m, double height)
        {
            var wavingTint = Color.Lerp(param.DirtyColor, param.BaseColor, (float) height - param.DirtyHeight);
            var wavingTintAlpha = wavingTint;
            wavingTintAlpha.a = 1f;

            // Material index perlin noise
            var materialIndex = 0;
            var materialCount = param.Materials.Length;
            if (materialCount > 1) {
                var noise = (simplex.GetValue(m.x * param.MaterialNoiseFrequency, m.z * param.MaterialNoiseFrequency) + 1) * 0.5;
                for (var i = 0; i < materialCount; ++i) {
                    if (param.Materials[i].MaterialProbability > noise) {
                        materialIndex = param.Materials[i].GrassMaterialIndex;
                        break;
                    }
                }
            }

            GenerateBladesAround(param, vertexDataPool, mesh, m, height, wavingTint, wavingTintAlpha, materialIndex, 360.0 / 16.0 * ((int) (m.z + m.x - m.y) % 17));
        }

        private void GenerateBladesAround(GrassParam param, UnsafePool<VertexData> vertexDataPool, MeshData mesh, Vector3 c, double height, Color wavingTint, Color wavingTintAlpha, int submesh, double rot)
        {
            Vector3 v1, v2;
            c.y -= param.GrassSize * TAN_ACOS_08 * 0.5f;

            v1 = c;
            v1.z += 1f * param.GrassSize;
            v2 = c;
            v2.x += COS_30 * param.GrassSize;
            v2.z -= SIN_30 * param.GrassSize;
            GenerateBlade(param, vertexDataPool, mesh, c, v1 + (v1 - v2) * FACTOR, v2 + (v2 - v1) * FACTOR, height, wavingTint, wavingTintAlpha, submesh, rot);

            v1 = c;
            v1.z += 1f * param.GrassSize;
            v2 = c;
            v2.x -= COS_30 * param.GrassSize;
            v2.z -= SIN_30 * param.GrassSize;
            GenerateBlade(param, vertexDataPool, mesh, c, v1 + (v1 - v2) * FACTOR, v2 + (v2 - v1) * FACTOR, height, wavingTint, wavingTintAlpha, submesh, rot);

            v1 = c;
            v1.x += COS_30 * param.GrassSize;
            v1.z -= SIN_30 * param.GrassSize;
            v2 = c;
            v2.x -= COS_30 * param.GrassSize;
            v2.z -= SIN_30 * param.GrassSize;
            GenerateBlade(param, vertexDataPool, mesh, c, v1 + (v1 - v2) * FACTOR, v2 + (v2 - v1) * FACTOR, height, wavingTint, wavingTintAlpha, submesh, rot);
        }

        private void GenerateBlade(GrassParam param, UnsafePool<VertexData> vertexDataPool, MeshData mesh, Vector3 c, Vector3 v1, Vector3 v2, double height, Color wavingTint, Color wavingTintAlpha, int submesh, double rot)
        {
            v1 = RotatePointAroundPivot(v1, c, rot);
            v2 = RotatePointAroundPivot(v2, c, rot);
            VertexData v;
            var indexBase = mesh.Vertices.Count;
            Vector3 vHeight;
            vHeight.x = 0;
            vHeight.y = (float) height;
            vHeight.z = 0;
            var normal = Vector3.Cross(vHeight, v2 - v1).normalized;

            // Add the 4 vertices of the blade

            v = vertexDataPool.Get();
            v.Vertex = v1;
            v.SetNormalAndColor(Vector3.up, wavingTint);
            v.Uv = new Vector2(0, 0);
            v.Tangent = new Vector4(-1f, 0f, 0, 0);
            mesh.Vertices.Add(v);

            v = vertexDataPool.Get();
            v.Vertex = v1 + vHeight + normal * 0.3f;
            v.SetNormalAndColor(Vector3.up, wavingTintAlpha);
            v.Uv = new Vector2(0, 1f);
            v.Tangent = new Vector4(-1f, 1f, 0, 0);
            mesh.Vertices.Add(v);

            v = vertexDataPool.Get();
            v.Vertex = v2 + vHeight + normal * 0.3f;
            v.SetNormalAndColor(Vector3.up, wavingTintAlpha);
            v.Uv = new Vector2(param.TileX, 1f);
            v.Tangent = new Vector4(1f, 1f, 0, 0);
            mesh.Vertices.Add(v);

            v = vertexDataPool.Get();
            v.Vertex = v2;
            v.SetNormalAndColor(Vector3.up, wavingTint);
            v.Uv = new Vector2(param.TileX, 0);
            v.Tangent = new Vector4(1f, 0f, 0, 0);
            mesh.Vertices.Add(v);


            // Add the 2 triangles of the blade

            var indices = mesh.GetIndices(submesh);
            indices.Add(indexBase + 0);
            indices.Add(indexBase + 2);
            indices.Add(indexBase + 1);
            indices.Add(indexBase + 2);
            indices.Add(indexBase + 0);
            indices.Add(indexBase + 3);
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, double angle)
        {
            var dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(0, (float) angle, 0) * dir; // rotate it
            return dir + pivot; // calculate rotated point and returns it
        }

        private VoxelType GetVoxelType(VertexData vd1, VertexData vd2, VertexData vd3)
        {
            var vt = voxelTypeSet.GetVoxelType(vd1.VoxelTypeIndex);
            var vt2 = voxelTypeSet.GetVoxelType(vd2.VoxelTypeIndex);
            var vt3 = voxelTypeSet.GetVoxelType(vd3.VoxelTypeIndex);
            if (vt.Priority < vt2.Priority) {
                vt = vt2;
            }

            if (vt.Priority < vt3.Priority) {
                vt = vt3;
            }

            return vt;
        }
    }
}