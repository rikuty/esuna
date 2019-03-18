using UnityEngine;

namespace UltimateTerrains
{
    [ExecuteInEditMode]
    public sealed class ChunkColliderComponent : MonoBehaviour
    {
        private static readonly Vector3 PositionOffset = new Vector3(0, -100000, 0);
        private Transform cTransform;
        private MeshCollider meshCollider;

        internal static ChunkColliderComponent Create(ChunkComponent chunk, UltimateTerrain terrain)
        {
            // Create game object
            var go = new GameObject("ChunkCollider")
            {
                layer = terrain.Params.ChunkLayer,
                hideFlags = Param.HideFlags
            };

            // Add ChunkColliderComponent
            var chunkCollider = go.AddComponent<ChunkColliderComponent>();
            chunkCollider.enabled = false;

            // Init
            chunkCollider.cTransform = go.transform;
            chunkCollider.cTransform.parent = chunk.cTransform;
            chunkCollider.cTransform.localPosition = Vector3.zero;
            chunkCollider.meshCollider = go.AddComponent<MeshCollider>();

            return chunkCollider;
        }

        public void FakeEnable()
        {
            cTransform.localPosition = Vector3.zero;
        }

        public void FakeDisable()
        {
            cTransform.localPosition = PositionOffset;
        }

        public void SetMesh(Mesh mesh)
        {
            meshCollider.sharedMesh = mesh;
        }

        public void ClearMesh()
        {
            meshCollider.sharedMesh = null;
        }
    }
}