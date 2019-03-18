using UltimateTerrains;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Orchestrator))]
public class OrchestratorEditor : Editor
{
    private void OnEnable()
    {
        target.hideFlags = HideFlags.HideInInspector;
    }
}