using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateTerrains
{
    internal sealed class Details
    {
        private readonly DetailObjectsIndexer detailObjectsIndexer;
        private readonly Matrix4x4[][] matrices;
        private readonly int[] matricesCount;


        internal Details(int maxDetailsCountPerChunkPerType, DetailObjectsIndexer detailObjectsIndexer)
        {
            this.detailObjectsIndexer = detailObjectsIndexer;
            matricesCount = new int[detailObjectsIndexer.Count];
            matrices = new Matrix4x4[detailObjectsIndexer.Count][];
            for (var i = 0; i < matrices.Length; i++) {
                matrices[i] = new Matrix4x4[maxDetailsCountPerChunkPerType];
            }
        }

        internal void Reset()
        {
            for (var i = 0; i < matricesCount.Length; i++) {
                matricesCount[i] = 0;
            }
        }

        internal void Add(DetailsParam.DetailObject detailObject, Vector3d position, Vector3 rotation, Vector3 scale)
        {
            var indexOfObject = detailObject.Index;
            if (matricesCount[indexOfObject] < matrices[indexOfObject].Length) {
                matrices[indexOfObject][matricesCount[indexOfObject]] = Matrix4x4.TRS(position.ToUnityOrigin(), Quaternion.Euler(rotation), scale);
                matricesCount[indexOfObject]++;
            } else {
                UDebug.LogWarning("Too many details of index " + detailObject.Index + " to render in a chunk. Please decrease its density.");
            }
        }

        internal void Render()
        {
            for (var i = 0; i < matrices.Length; i++) {
                if (matricesCount[i] > 0) {
                    var obj = detailObjectsIndexer.Get(i);
                    Graphics.DrawMeshInstanced(obj.Mesh, 0, obj.Material, matrices[i], matricesCount[i], null,
                                               ShadowCastingMode.On, true, 0);
                }
            }
        }

        internal void CopyTo(Details other)
        {
            for (var i = 0; i < matricesCount.Length; i++) {
                var mCount = matricesCount[i];
                other.matricesCount[i] = mCount;

                var m = matrices[i];
                var otherM = other.matrices[i];
                for (var j = 0; j < m.Length && j < mCount; j++) {
                    otherM[j] = m[j];
                }
            }
        }
    }
}