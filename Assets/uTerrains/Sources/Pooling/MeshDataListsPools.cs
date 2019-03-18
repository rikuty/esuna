using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    internal sealed class MeshDataListsPools
    {
        private readonly int minCapacityOfList;

        private readonly UnsafeListPool<Vector2> vector2ListPools;
        private readonly UnsafeListPool<Vector3> vector3ListPools;
        private readonly UnsafeListPool<Vector4> vector4ListPools;
        private readonly UnsafeListPool<Color32> colorListPools;
        private readonly UnsafeListPool<Vector3d> vector3DListPools;

        internal MeshDataListsPools(int initialListCount, int minCapacityOfList)
        {
            this.minCapacityOfList = minCapacityOfList;
            vector2ListPools = new UnsafeListPool<Vector2>(initialListCount, minCapacityOfList);
            vector3ListPools = new UnsafeListPool<Vector3>(initialListCount, minCapacityOfList);
            vector4ListPools = new UnsafeListPool<Vector4>(initialListCount, minCapacityOfList);
            colorListPools = new UnsafeListPool<Color32>(initialListCount, minCapacityOfList);
            vector3DListPools = new UnsafeListPool<Vector3d>(initialListCount, minCapacityOfList);
        }

        internal List<Vector2> GetListVector2(int size)
        {
            return vector2ListPools.Get(Math.Max(size, minCapacityOfList));
        }

        internal List<Vector3> GetListVector3(int size)
        {
            return vector3ListPools.Get(Math.Max(size, minCapacityOfList));
        }

        internal List<Vector4> GetListVector4(int size)
        {
            return vector4ListPools.Get(Math.Max(size, minCapacityOfList));
        }

        internal List<Color32> GetListColor(int size)
        {
            return colorListPools.Get(Math.Max(size, minCapacityOfList));
        }

        internal List<Vector3d> GetListVector3D(int size)
        {
            return vector3DListPools.Get(Math.Max(size, minCapacityOfList));
        }

        internal void FreeListVector2(List<Vector2> a)
        {
            vector2ListPools.Free(a);
        }

        internal void FreeListVector3(List<Vector3> a)
        {
            vector3ListPools.Free(a);
        }

        internal void FreeListVector4(List<Vector4> a)
        {
            vector4ListPools.Free(a);
        }

        internal void FreeListColor(List<Color32> a)
        {
            colorListPools.Free(a);
        }

        internal void FreeListVector3D(List<Vector3d> a)
        {
            vector3DListPools.Free(a);
        }

        public void DebugLog()
        {
            //UDebug.Log("There are currently " + vector2ListPools.Count + "/" + vector2ListPools.TotalInstanciatedCount + " vector2List objects in pool");
            //UDebug.Log("There are currently " + vector3ListPools.Count + "/" + vector3ListPools.TotalInstanciatedCount + " vector3List objects in pool");
            //UDebug.Log("There are currently " + vector4ListPools.Count + "/" + vector4ListPools.TotalInstanciatedCount + " vector4List objects in pool");
            //UDebug.Log("There are currently " + colorListPools.Count + "/" + colorListPools.TotalInstanciatedCount + " colorList objects in pool.");
        }
    }
}