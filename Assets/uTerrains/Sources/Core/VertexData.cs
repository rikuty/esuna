using UnityEngine;

namespace UltimateTerrains
{
    internal sealed class VertexData
    {
        public Vector3 Vertex;
        public Vector3 Normal { get; private set; }
        public Vector4 Uv;
        public Vector4 Uv2;
        public Vector4 Uv3;
        public Vector4 Uv4;
        public Vector4 Tangent;
        public Color32 Color { get; private set; }
        public float Blockiness { get; private set; }
        public ushort VoxelTypeIndex { get; private set; }

        public void SetNormal(Vector3 normal)
        {
            Normal = normal;
        }

        public void SetNormalAndVoxelType(Vector3d meshWorldPosition, Vector3 normal, VoxelType voxelType)
        {
            VoxelTypeIndex = voxelType.Index;
            Normal = normal;
            Blockiness = voxelType.Blockiness;
            Color = voxelType.Functions.GetVertexColor(meshWorldPosition, Vertex, normal);
            Uv = voxelType.Functions.GetVertexUVW(meshWorldPosition, Vertex, normal);
            Uv2 = voxelType.Functions.GetVertexUVW2(meshWorldPosition, Vertex, normal);
            Uv3 = voxelType.Functions.GetVertexUVW3(meshWorldPosition, Vertex, normal);
            Uv4 = voxelType.Functions.GetVertexUVW4(meshWorldPosition, Vertex, normal);
        }

        public void SetNormalAndColor(Vector3 normal, Color32 color)
        {
            VoxelTypeIndex = ushort.MaxValue;
            Normal = normal;
            Blockiness = 0f;
            Color = color;
        }

        public void CopyTo(VertexData other)
        {
            other.Vertex = Vertex;
            other.Normal = Normal;
            other.Uv = Uv;
            other.Uv2 = Uv2;
            other.Uv3 = Uv3;
            other.Uv4 = Uv4;
            other.Tangent = Tangent;
            other.Color = Color;
            other.Blockiness = Blockiness;
            other.VoxelTypeIndex = VoxelTypeIndex;
        }
    }
}