using System;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public abstract class AbstractVoxelTypeFunctions : ScriptableObject
    {
        public abstract Color32 GetVertexColor(Vector3d meshWorldPosition, Vector3 vertexPosition, Vector3 vertexNormal);

        public virtual Vector4 GetVertexUVW(Vector3d meshWorldPosition, Vector3 vertexPosition, Vector3 vertexNormal)
        {
            return new Vector4
            {
                x = (float)(meshWorldPosition.x % 1.0) + vertexPosition.x,
                y = (float)(meshWorldPosition.z % 1.0) + vertexPosition.z
            };
        }

        public virtual Vector4 GetVertexUVW2(Vector3d meshWorldPosition, Vector3 vertexPosition, Vector3 vertexNormal)
        {
            return Vector4.zero;
        }

        public virtual Vector4 GetVertexUVW3(Vector3d meshWorldPosition, Vector3 vertexPosition, Vector3 vertexNormal)
        {
            return Vector4.zero;
        }

        public virtual Vector4 GetVertexUVW4(Vector3d meshWorldPosition, Vector3 vertexPosition, Vector3 vertexNormal)
        {
            return Vector4.zero;
        }

        public abstract void OnEditorGUI(UltimateTerrain uTerrain, VoxelType voxelType);
    }
}