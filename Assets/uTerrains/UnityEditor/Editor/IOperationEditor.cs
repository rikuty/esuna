using UnityEditor;

namespace UltimateTerrainsEditor
{
    public interface IOperationEditor
    {
        string Name { get; }

        void OnInspector(TerrainToolEditor editor);

        void OnScene(TerrainToolEditor editor, SceneView sceneview);

        void DestroyReticle();
    }
}