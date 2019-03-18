using LibNoise.Primitive;
using UnityEngine;

namespace UltimateTerrains
{
    internal static class DetailsGenerator
    {
        private static readonly SimplexPerlin simplex = new SimplexPerlin();
        private static readonly SimplexPerlin simplexScale = new SimplexPerlin();

        public static void AddDetailsToPolygon(Details details, DetailsParam param, Vector3 p1, Vector3 p2, Vector3 p3,
                                               Vector3d chunkWorldPos)
        {
            double minX = p1.x;
            if (p2.x < minX)
                minX = p2.x;
            if (p3.x < minX)
                minX = p3.x;

            double minY = p1.y;
            if (p2.y < minY)
                minY = p2.y;
            if (p3.y < minY)
                minY = p3.y;

            double minZ = p1.z;
            if (p2.z < minZ)
                minZ = p2.z;
            if (p3.z < minZ)
                minZ = p3.z;

            double maxX = p1.x;
            if (p2.x > maxX)
                maxX = p2.x;
            if (p3.x > maxX)
                maxX = p3.x;

            double maxY = p1.y;
            if (p2.y > maxY)
                maxY = p2.y;
            if (p3.y > maxY)
                maxY = p3.y;

            double maxZ = p1.z;
            if (p2.z > maxZ)
                maxZ = p2.z;
            if (p3.z > maxZ)
                maxZ = p3.z;

            var minNx = (int) ((minX + chunkWorldPos.x) * param.DensityXInverse);
            if (minNx < 0)
                minNx -= 1;
            var minNz = (int) ((minZ + chunkWorldPos.z) * param.DensityZInverse);
            if (minNz < 0)
                minNz -= 1;
            var maxNx = (int) ((maxX + chunkWorldPos.x) * param.DensityXInverse);
            if (maxNx < 0)
                maxNx -= 1;
            var maxNz = (int) ((maxZ + chunkWorldPos.z) * param.DensityZInverse);
            if (maxNz < 0)
                maxNz -= 1;

            for (var nx = minNx; nx <= maxNx; ++nx) {
                for (var nz = minNz; nz <= maxNz; ++nz) {
                    var x = (nx + (BevinsValue.ValueNoise1D(nx + (nx + nz * 9) % 5) + 1.0) * 0.4) *
                            param.DensityX - chunkWorldPos.x;
                    var z = (nz + (BevinsValue.ValueNoise1D(nz + (nx * 7 - nz) % 6) + 1.0) * 0.4) *
                            param.DensityZ - chunkWorldPos.z;
                    if (minX <= x && x <= maxX && minZ <= z && z <= maxZ) {
                        Vector3 hitPoint, hitNormal, m;
                        m.x = (float) x;
                        m.y = (float) (maxY + 1);
                        m.z = (float) z;
                        if (Vector3Utils.Intersect3DRayTriangle(new Ray(m, Vector3.down),
                                                                p1, p2, p3, (float) (maxY - minY + 2),
                                                                out hitPoint, out hitNormal)) {
                            hitPoint.y = hitPoint.y + param.VerticalOffset;
                            AddDetailAt(details, param, chunkWorldPos + (Vector3d) hitPoint);
                        }
                    }
                }
            }
        }

        private static void AddDetailAt(Details details, DetailsParam param, Vector3d wpos)
        {
            var objectCount = param.ObjectCount;
            if (objectCount > 0) {
                var noise = (simplex.GetValue(wpos.x * param.ObjectsNoiseFrequency,
                                              wpos.z * param.ObjectsNoiseFrequency) + 1.0) * 0.5;
                for (var i = 0; i < objectCount; ++i) {
                    if (param.Objects[i].ObjectProbability > noise) {
                        var rot = param.Rotate
                            ? new Vector3(0, 360f / 16f * ((int) (wpos.x + wpos.z) % 17), 0)
                            : Vector3.zero;
                        var scale = Vector3.one * Mathf.Clamp(
                                        (float) (simplexScale.GetValue(wpos.x * param.ScaleNoiseFrequency,
                                                                       wpos.z * param.ScaleNoiseFrequency) + 1.0) *
                                        0.5f * (param.MaxScale - param.MinScale) + param.MinScale,
                                        param.MinScale, param.MaxScale);
                        details.Add(param.Objects[i], wpos, rot, scale);
                        break;
                    }
                }
            }
        }
    }
}