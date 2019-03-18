using System;
using UnityEngine;

namespace UltimateTerrains
{
    public static class Vector3Utils
    {
        public static float DistanceSquared(Vector3 a, Vector3 b)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;
            var dz = b.z - a.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static bool IsPointInsideTriangleTopDown(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var alpha = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) /
                        ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));
            var beta = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) /
                       ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));
            var gamma = 1.0f - alpha - beta;
            return alpha > 0 && beta > 0 && gamma > 0;
        }


        public static bool Intersect3DRayTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, float distance, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            hitPoint = Vector3.zero;
            hitNormal = Vector3.zero;
            Vector3 u, v, n; // triangle vectors
            Vector3 dir, w0, w; // ray vectors
            float r, a, b; // params to calc ray-plane intersect

            // get triangle edge vectors and plane normal 
            u = v1 - v0;
            v = v2 - v0;
            n = Vector3.Cross(u, v); // cross product
            if (n == Vector3.zero) // triangle is degenerate
                return false; // do not deal with this case

            dir = ray.direction; // ray direction vector
            if (distance > 0)
                dir *= distance;

            w0 = ray.origin - v0;
            a = -Vector3.Dot(n, w0);
            b = Vector3.Dot(n, dir);
            if (b < Param.PRECISION && b > -Param.PRECISION) // ray is  parallel to triangle plane
                return false;

            // get intersect point of ray with triangle plane
            r = a / b;
            if (r < 0f) // ray goes away from triangle
                return false; // => no intersect

            // for a segment, also test if (r > 1.0) => no intersect
            if (distance > 0 && r > 1f)
                return false; // => no intersect

            hitPoint = ray.origin + r * dir; // intersect point of ray and plane

            // is I inside T?
            float uu, uv, vv, wu, wv, D;
            uu = Vector3.Dot(u, u);
            uv = Vector3.Dot(u, v);
            vv = Vector3.Dot(v, v);
            w = hitPoint - v0;
            wu = Vector3.Dot(w, u);
            wv = Vector3.Dot(w, v);
            D = uv * uv - uu * vv;

            // get and test parametric coords
            float s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0f || s > 1f) // intersection is outside the triangle
                return false;
            t = (uv * wu - uu * wv) / D;
            if (t < 0f || s + t > 1f) // intersection is outside the triangle
                return false;

            hitNormal = n.normalized;

            return true; // intersection is inside the triangle
        }


        public static float DistanceTo(Bounds b, Vector3 pos)
        {
            float distance;
            var dMin = pos - b.min;
            var dMax = b.max - pos;

            // Check if inside of the box
            if (dMin.x >= 0f && dMin.y >= 0f && dMin.z >= 0f &&
                dMax.x >= 0f && dMax.y >= 0f && dMax.z >= 0f) {
                distance = dMin.x;
                if (dMin.y < distance) {
                    distance = dMin.y;
                }

                if (dMin.z < distance) {
                    distance = dMin.z;
                }

                if (dMax.x < distance) {
                    distance = dMax.x;
                }

                if (dMax.y < distance) {
                    distance = dMax.y;
                }

                if (dMax.z < distance) {
                    distance = dMax.z;
                }

                // we are inside the bounds so we want to return a negative distance
                distance = -distance;
            } else {
                // we are outside the bounds so we return a positive distance
                distance = Mathf.Sqrt(b.SqrDistance(pos));
            }

            return distance;
        }

        
    }
}