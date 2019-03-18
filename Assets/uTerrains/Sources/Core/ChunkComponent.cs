using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UltimateTerrains
{
    [ExecuteInEditMode]
    public sealed class ChunkComponent : MonoBehaviour
    {
        public bool gizmoDrawVertices;
        public bool gizmoDrawLines;

        private int id;
        private Chunk chunk;
        private Details detailsCopy;
        private UltimateTerrain terrain;

        // Components caching
        public Transform cTransform;
        private MeshRenderer cRenderer;
        private MeshFilter filter;
        private ChunkColliderComponent chunkCollider;
        private Mesh filterMesh;
        private GrassComponent grassObject;
        private bool built = true;
        private int level;
        private bool isFree;

        // Fast access to parameters
        private int maxLevelCollider;
        private int maxLevelGrass;
        private int maxLevelShadows;
        private int maxLevelDetails;

        public int Id {
            get { return id; }
        }

        public int ChunkId {
            get { return chunk != null ? chunk.Id : -1; }
        }

        public MeshFilter Filter {
            get { return filter; }
        }

#if CHUNK_DEBUG
        public
#else
        internal
#endif
            Chunk Chunk {
            get { return chunk; }
        }

        internal bool IsBelongingToItsChunk {
            get { return !isFree && chunk != null && chunk.HasChunkObject && chunk.IsChunkObjectEqualTo(this); }
        }

        private bool HasMeshCollider {
            get { return !ReferenceEquals(chunkCollider, null); }
        }

        public int Level {
            get { return level; }
        }

        public bool Built {
            get { return built; }
        }

        internal bool IsFree {
            get { return isFree; }
        }

        private static int globalUID;

        internal static string GetName(int level, int id)
        {
            return "Chunk_#" + id + "_[" + level + "]";
        }

        internal static ChunkComponent CreateChunkObject(int lvl, UltimateTerrain terrain)
        {
            // Create game object
            var id = globalUID;
            var go = new GameObject(GetName(lvl, id));
            globalUID++;
            go.layer = terrain.Params.ChunkLayer;
            go.hideFlags = Param.HideFlags;

            // Add Chunk component
            var chunkObject = go.AddComponent<ChunkComponent>();
            chunkObject.id = id;
            chunkObject.terrain = terrain;
            chunkObject.isFree = true;

            // Init transform
            chunkObject.cTransform = go.transform;
            chunkObject.cTransform.parent = terrain.GetChunksParent();

            // Init Chunk component
            chunkObject.level = lvl;
            chunkObject.maxLevelCollider = terrain.Params.MaxLevelCollider;
            chunkObject.maxLevelGrass = terrain.Params.MaxLevelGrass;
            chunkObject.maxLevelShadows = terrain.Params.MaxLevelShadows;
            chunkObject.maxLevelDetails = terrain.Params.MaxLevelDetails;
            chunkObject.chunk = null;
            chunkObject.cRenderer = go.AddComponent<MeshRenderer>();
            chunkObject.cRenderer.enabled = false;
            chunkObject.filter = go.AddComponent<MeshFilter>();

            if (lvl <= chunkObject.maxLevelCollider) {
                chunkObject.chunkCollider = ChunkColliderComponent.Create(chunkObject, terrain);
            }

            if (lvl <= chunkObject.maxLevelGrass) {
                chunkObject.grassObject = GrassComponent.CreateGrassObject(terrain, chunkObject);
            }

            if (lvl <= chunkObject.maxLevelDetails) {
                chunkObject.detailsCopy = new Details(terrain.Params.MaxDetailsCountPerChunkPerType, terrain.DetailObjectsIndexer);
            }

            if (lvl <= chunkObject.maxLevelShadows) {
                chunkObject.cRenderer.shadowCastingMode = ShadowCastingMode.On;
                chunkObject.cRenderer.receiveShadows = true;
            } else {
                chunkObject.cRenderer.shadowCastingMode = ShadowCastingMode.Off;
                chunkObject.cRenderer.receiveShadows = false;
            }

            chunkObject.enabled = false;

            return chunkObject;
        }

        // Delete mesh when chunk is destroyed to prevent leaks
        public void DestroyMeshAndReset()
        {
            if (filterMesh != null) {
                DestroyImmediate(filterMesh);
                filterMesh = null;
            }

            if (grassObject != null) {
                grassObject.DestroyMeshAndReset();
            }

            chunk = null;
            terrain = null;
        }


        //Update the position of the chunk's tranform so it matches with its UltimateTerrain position,
        //and reset some values to be ready for new build.
        internal void AffectNewPosition()
        {
            cTransform.position = ComputeUnityPosition();
        }

        internal void Enable()
        {
            if (chunk == null) {
                UDebug.Fatal("[ChunkComponent#" + Id + "] Chunk is null in enable");
                return;
            }

            if (!chunk.IsChunkObjectEqualTo(this)) {
                UDebug.Fatal("[ChunkComponent#" + Id + "] Chunk's chunkObject is different than this one in enable");
                return;
            }

            enabled = level <= maxLevelDetails;
            cRenderer.enabled = true;
            if (HasMeshCollider) {
                chunkCollider.FakeEnable();
            }

            if (level <= maxLevelGrass && grassObject.ReadyToBeRendered) {
                grassObject.Enable();
            }
        }

        internal void Disable()
        {
            enabled = false;
            cRenderer.enabled = false;
            if (HasMeshCollider) {
                chunkCollider.FakeDisable();
            }

            if (level <= maxLevelGrass) {
                grassObject.Disable();
            }
        }

        internal void SetFree()
        {
            isFree = true;
            chunk = null;
            built = true;
            Disable();
            filter.sharedMesh = null;
            if (HasMeshCollider) {
                chunkCollider.ClearMesh();
                chunkCollider.FakeDisable();
            }

            filterMesh.Clear();
            ChunkObjectPool.MeshPool.Free(filterMesh);
            filterMesh = null;

            if (grassObject != null) {
                grassObject.SetFree();
            }

            if (detailsCopy != null) {
                detailsCopy.Reset();
            }
        }

        internal void SetUsedBy(Chunk chunkOfObject)
        {
            isFree = false;
            chunk = chunkOfObject;
        }


        private Vector3 ComputeUnityPosition()
        {
            return new Vector3d(
                chunk.Position.x * terrain.Params.SizeXTotal,
                chunk.Position.y * terrain.Params.SizeYTotal,
                chunk.Position.z * terrain.Params.SizeZTotal).ToUnityOrigin();
        }

        internal void PostBuild(bool renderImmediately)
        {
            if (chunk == null) {
                UDebug.Fatal("[ChunkComponent#" + Id + "] Chunk is null in post-building");
                return;
            }

            if (!chunk.IsChunkObjectEqualTo(this)) {
                UDebug.Fatal("[ChunkComponent#" + Id + "] Chunk's chunkObject is different than this one in post-building");
                return;
            }

            Profiler.BeginSample("PostBuild inside");

            var meshData = chunk.MeshData;

            if (meshData != null && meshData.HasTriangles) {
                //UP.FindDuplicateVertices (meshData);
                //UP.FindDuplicateTriangle (meshData);

                // Apply new mesh
                var bounds = new Bounds(
                    new Vector3(level * terrain.Params.SizeXTotalF / 2, level * terrain.Params.SizeYTotalF / 2, level * terrain.Params.SizeZTotalF / 2),
                    new Vector3(level * terrain.Params.SizeXTotalF, level * terrain.Params.SizeYTotalF, level * terrain.Params.SizeZTotalF)
                );
                int materialsMask;

                Profiler.BeginSample("meshData.ToMesh");
                meshData.ToMesh(ref filterMesh, out materialsMask, bounds);
                Profiler.EndSample();

                cRenderer.sharedMaterials = terrain.VoxelTypeSet.GetMaterials(materialsMask);

                filter.sharedMesh = filterMesh;

                if (HasMeshCollider) {
                    Profiler.BeginSample("Update meshCollider");
                    // Update mesh collider collision data
                    chunkCollider.SetMesh(filterMesh);
                    Profiler.EndSample();
                }

                // Immediately send the modified mesh to the graphics API, to avoid a possible hiccup later.
                Profiler.BeginSample("UploadMeshData");
                filterMesh.UploadMeshData(false);
                Profiler.EndSample();

                if (level <= maxLevelGrass) {
                    Profiler.BeginSample("PostBuild grass");
                    grassObject.PostBuild(renderImmediately);
                    Profiler.EndSample();
                }

                if (level <= maxLevelDetails) {
                    Profiler.BeginSample("Spawn details objects");
                    chunk.Details.CopyTo(detailsCopy);
                    Profiler.EndSample();
                }

                if (renderImmediately) {
                    Enable();
                }
            } else {
                UDebug.LogWarning("[ChunkComponent#" + Id + "] Mesh data should always contain vertices when entering Chunk-object post-building process");
                Disable();
            }

            built = true;
            Profiler.EndSample();
        }

        private void LateUpdate()
        {
            if (detailsCopy != null) {
                detailsCopy.Render();
            } else {
                UDebug.LogWarning("ChunkComponent is enabled but there is no details to render");
            }
        }


        // PUBLIC

        /**
	 * Build and post-build this chunk immediately. Possible impact on frame rate.
	 * @param createVegetation If true, vegetation will be generated.
	 */
        public void BuildImmediately(bool clearCache, int count = 1)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < count; i++) {
                //TODO
                //chunk.BuildImmediately(clearCache);
            }

            stopwatch.Stop();
            UDebug.Log("[ChunkComponent#" + Id + "] ElapsedMilliseconds for " + count + " build(s) = " + stopwatch.ElapsedMilliseconds);
        }

        /**
	 * @return mesh of this Chunk.
	 */
        public Mesh GetMesh()
        {
            if (filter != null)
                return filter.sharedMesh;
            return null;
        }

        public bool Raycast(Ray ray, float distance, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            var mesh = GetMesh();
            ray.origin -= cTransform.position;
            if (mesh != null && MeshUtils.Raycast(mesh, ray, distance, out hitPoint, out hitNormal)) {
                hitPoint += cTransform.position;
                return true;
            }

            hitPoint = Vector3.zero;
            hitNormal = Vector3.zero;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (gizmoDrawVertices) {
                Gizmos.color = Color.yellow;
                foreach (var vert in filter.sharedMesh.vertices) {
                    Gizmos.DrawWireSphere(cTransform.TransformPoint(vert), 0.05f);
                }
            }

            if (gizmoDrawLines) {
                Gizmos.color = Color.green;
                for (var x = 0; x < Param.SIZE_X; ++x) {
                    for (var z = 0; z < Param.SIZE_Z; ++z) {
                        Gizmos.DrawLine(cTransform.position + new Vector3(x * terrain.Params.SizeXVoxelF, 0, z * terrain.Params.SizeZVoxelF) * level,
                                        cTransform.position + new Vector3(x * terrain.Params.SizeXVoxelF, terrain.Params.SizeYTotalF, z * terrain.Params.SizeZVoxelF) * level);
                    }
                }

                Gizmos.color = Color.blue;
                for (var x = 0; x < Param.SIZE_X; ++x) {
                    for (var y = 0; y < Param.SIZE_Y; ++y) {
                        Gizmos.DrawLine(cTransform.position + new Vector3(x * terrain.Params.SizeXVoxelF, y * terrain.Params.SizeYVoxelF, 0) * level,
                                        cTransform.position + new Vector3(x * terrain.Params.SizeXVoxelF, y * terrain.Params.SizeYVoxelF, terrain.Params.SizeZTotalF) * level);
                    }
                }

                Gizmos.color = Color.red;
                for (var z = 0; z < Param.SIZE_Z; ++z) {
                    for (var y = 0; y < Param.SIZE_Y; ++y) {
                        Gizmos.DrawLine(cTransform.position + new Vector3(0, y * terrain.Params.SizeYVoxelF, z * terrain.Params.SizeZVoxelF) * level,
                                        cTransform.position + new Vector3(terrain.Params.SizeXTotalF, y * terrain.Params.SizeYVoxelF, z * terrain.Params.SizeZVoxelF) * level);
                    }
                }
            }
        }
    }
}