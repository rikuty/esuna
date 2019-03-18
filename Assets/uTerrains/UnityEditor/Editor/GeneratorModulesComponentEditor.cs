using UltimateTerrains;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GeneratorModulesComponent))]
public class GeneratorModulesComponentEditor : Editor
{
    private void OnEnable()
    {
        target.hideFlags = HideFlags.HideInInspector;
    }
}