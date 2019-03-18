using UltimateTerrains;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AsyncOperationOrchestrator))]
public class AsyncOperationOrchestratorEditor : Editor
{
    private void OnEnable()
    {
        target.hideFlags = HideFlags.HideInInspector;
    }
}