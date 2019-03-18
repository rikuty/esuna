using UnityEngine;

namespace UltimateTerrains
{
    public static class MeshUtils
    {
        public static bool Raycast(Mesh mesh, Ray ray, float distance, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            hitPoint = Vector3.zero;
            hitNormal = Vector3.zero;
            var vertices = mesh.vertices;
            for (var sub = 0; sub < mesh.subMeshCount; ++sub) {
                var indices = mesh.GetIndices(sub);
                var indexCount = indices.Length;
                for (var i = 0; i < indexCount; i += 3) {
                    // raycast on that triangle
                    if (Vector3Utils.Intersect3DRayTriangle(ray, vertices[indices[i + 0]], vertices[indices[i + 1]], vertices[indices[i + 2]], distance, out hitPoint, out hitNormal))
                        return true;
                }
            }

            return false;
        }
    }
}