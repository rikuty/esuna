using System;
using UltimateTerrains;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    [CustomEditor(typeof(ChunkComponent))]
    public sealed class ChunkComponentEditor : Editor
    {
        private ChunkComponent chunkObject;

        private int buildCount = 1;

        public void OnEnable()
        {
            chunkObject = (ChunkComponent) target;
        }

        public void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            var style = new GUIStyle(GUI.skin.label);
            EditorGUILayout.LabelField("ID: " + chunkObject.Id, style);
            EditorGUILayout.LabelField("Internal Chunk ID: " + chunkObject.ChunkId, style);
            EditorGUILayout.LabelField("Is built?: " + chunkObject.Built, style);
            if (chunkObject.Filter.sharedMesh != null) {
                EditorGUILayout.LabelField("Vertex Count: " + chunkObject.Filter.sharedMesh.vertexCount, style);
                EditorGUILayout.LabelField("Triangle Count: " + chunkObject.Filter.sharedMesh.triangles.Length, style);
            }

#if CHUNK_DEBUG
            if (chunkObject.Chunk != null) {
                EditorGUILayout.LabelField("Chunk Has Chunk Object: " + chunkObject.Chunk.HasChunkObject, style);
                EditorGUILayout.LabelField("Chunk's Chunk Object ID: " + chunkObject.Chunk.ChunkObject.Id, style);
                EditorGUILayout.LabelField("Level: " + chunkObject.Chunk.Level, style);
                EditorGUILayout.LabelField("Position: " + chunkObject.Chunk.Position, style);
                EditorGUILayout.LabelField("World Position: " + chunkObject.Chunk.WorldPosition, style);
                EditorGUILayout.LabelField("Center Position: " + chunkObject.Chunk.CenterPosition, style);
            }
#endif

            buildCount = EditorGUILayout.IntField("Build N times", buildCount);

            if (GUILayout.Button("Build Immediately")) {
                //add everthing the button would do.
                chunkObject.BuildImmediately(false, buildCount);
            }

            if (GUILayout.Button("Build Immediately (clear cache)")) {
                //add everthing the button would do.
                chunkObject.BuildImmediately(true, buildCount);
            }

            if (GUILayout.Button("Collect GC")) {
                GC.Collect();
            }

            DrawDefaultInspector();

            if (GUI.changed) {
                EditorUtility.SetDirty(target);
            }
        }
    }
}