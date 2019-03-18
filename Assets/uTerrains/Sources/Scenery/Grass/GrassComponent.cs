using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateTerrains
{
    public sealed class GrassComponent : MonoBehaviour
    {
        [SerializeField] private ChunkComponent chunkObject;
        [SerializeField] private UltimateTerrain terrain;

        // Components caching
        [SerializeField] private Transform cTransform;
        [SerializeField] private MeshRenderer cRenderer;
        [SerializeField] private MeshFilter filter;
        [SerializeField] private Mesh filterMesh;
        [SerializeField] private int level;
        [SerializeField] public bool ReadyToBeRendered;

        internal static GrassComponent CreateGrassObject(UltimateTerrain terrain, ChunkComponent chunkObject)
        {
            // Create game object
            var go = new GameObject("Grass");
            go.layer = terrain.Params.GrassLayer;
            go.hideFlags = Param.HideFlags;

            // Add Grass component
            var grassObject = go.AddComponent<GrassComponent>();
            grassObject.terrain = terrain;
            grassObject.chunkObject = chunkObject;
            grassObject.level = chunkObject.Level;

            // Init transform
            grassObject.cTransform = go.transform;
            grassObject.cTransform.parent = chunkObject.cTransform;

            // Init Chunk component
            grassObject.cRenderer = go.AddComponent<MeshRenderer>();
            grassObject.cRenderer.sharedMaterials = terrain.VoxelTypeSet.GrassMaterials;
            grassObject.filter = go.AddComponent<MeshFilter>();
            if (terrain.Params.GrassCastShadows) {
                grassObject.cRenderer.shadowCastingMode = ShadowCastingMode.On;
            } else {
                grassObject.cRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }

            grassObject.cRenderer.receiveShadows = terrain.Params.GrassReceiveShadows;
            grassObject.ReadyToBeRendered = false;

            grassObject.enabled = false;
            grassObject.Disable();

            return grassObject;
        }

        internal void Enable()
        {
            cRenderer.enabled = true;
        }

        internal void Disable()
        {
            cRenderer.enabled = false;
        }

        // Delete mesh when grass is destroyed to prevent leaks
        public void DestroyMeshAndReset()
        {
            if (filterMesh != null) {
                DestroyImmediate(filterMesh);
            }

            chunkObject = null;
            terrain = null;
        }

        internal void SetFree()
        {
            Disable();
            filter.sharedMesh = null;
            if (!Application.isEditor) {
                Destroy(filterMesh);
            } else {
                DestroyImmediate(filterMesh);
            }

            filterMesh = null;
        }

        /**
	 * Harmonize new mesh with neighbours, apply it to the chunk, enable renderer if needed, 
	 * compute mesh collider, and add vegetation.
	 * @param updateCollider If true, mesh-collider will be update.
	 */
        internal void PostBuild(bool renderImmediately)
        {
            ReadyToBeRendered = false;

            if (chunkObject.Chunk == null) {
                UDebug.LogError("ChunkObject.Chunk is null in grass post-building");
                return;
            }

            var meshData = chunkObject.Chunk.GrassMeshData;

            if (meshData != null && meshData.HasTriangles) {
                var bounds = new Bounds(
                    new Vector3(level * terrain.Params.SizeXTotalF / 2, level * terrain.Params.SizeYTotalF / 2, level * terrain.Params.SizeZTotalF / 2),
                    new Vector3(level * terrain.Params.SizeXTotalF, level * terrain.Params.SizeYTotalF, level * terrain.Params.SizeZTotalF)
                );

                // Apply new mesh
                int mMask;
                meshData.ToMesh(ref filterMesh, out mMask, bounds);

                filter.sharedMesh = filterMesh;

                ReadyToBeRendered = true;
                if (renderImmediately) {
                    Enable();
                }
            } else {
                Disable();
            }
        }
    }
}