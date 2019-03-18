using System;
using System.IO;
using System.Linq;
using UltimateTerrains;
using UltimateTerrainsEditor.NodeBased;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    public sealed class GeneratorModulesEditor
    {
        private GeneratorModulesComponent generatorModulesComponent;
        private UltimateTerrain terrain;
        private TerrainToolEditor terrainToolEditor;
        private readonly ListInspector<Biome> biomesInspector;
        private readonly Type[] dockToWindows;

        public GeneratorModulesEditor()
        {
            var consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
            this.dockToWindows = consoleWindowType != null ? new[] {consoleWindowType} : new Type[0];

            biomesInspector = new ListInspector<Biome>(
                "Add New Biome", null, "Do you really want to delete this biome?", false,
                // Biome inspector
                (biome, i) => {
                    if (!terrain)
                        return biome;
                    
                    EditorGUILayout.LabelField("Biome Id: " + i, EditorStyles.boldLabel);
                    biome = (Biome) EditorGUILayout.ObjectField("Asset", biome, typeof(Biome), false);
                    if (!biome)
                        return biome;

                    Undo.RecordObject(biome, "Biome configuration change");
                    biome.Name = EditorGUILayout.TextField("Name:", biome.Name);

                    if (GUILayout.Button("Edit Biome")) {
                        biome.Init();
                        var editorWindow = EditorWindow.GetWindow<NodeBasedEditor>("Generation Flow", true, dockToWindows);
                        editorWindow.Init(terrain, biome);
                    }

                    return biome;
                },
                // Create biome
                () => {
                    var biome = (Biome) ScriptableObject.CreateInstance(typeof(Biome));
                    biome.Init(true);
                    return biome;
                },
                // Remove biome condition
                i => i > 0,
                true);
        }

        public void OnEnable(UltimateTerrain terrain, TerrainToolEditor terrainToolEditor, GeneratorModulesComponent component)
        {
            this.terrain = terrain;
            this.terrainToolEditor = terrainToolEditor;
            generatorModulesComponent = component;
            generatorModulesComponent.Init();
        }

        public void OnInspectorGUI()
        {
            Undo.RecordObject(generatorModulesComponent, "Biomes configuration change");

            EditorGUILayout.HelpBox("Biomes define how the terrain will be generated. Ultimate Terrains supports multiple biomes, " +
                                    "which means you can generate completely different kind of terrain in different areas.",
                                    MessageType.Info, true);

            EditorGUILayout.HelpBox("It is strongly recommended to get a look at the documentation before editing or adding biomes.",
                                    MessageType.Warning, true);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Enable this to quickly see the result of your configuration changes when you " +
                                    "are editing biomes.",
                                    MessageType.Info, true);
            terrainToolEditor.ToggleDrawTerrainInEditor(true, false, "Enable terrain preview");
            if (TerrainToolEditor.MinLODLevelInEditor < TerrainToolEditor.LODCountForEditor - 3) {
                var bkgCol = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                EditorGUILayout.HelpBox("CAUTION: it is recommended to keep 'Lowest LOD to compute' value close to " +
                                        "'LOD count in editor' value to keep it fast. " +
                                        "Otherwise preview may take too much time to get refreshed.\n\n" +
                                        "Try to increase 'Lowest LOD to compute' value.",
                                        MessageType.Error, true);
                GUI.backgroundColor = bkgCol;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Biome Selector:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The Biome Selector tells which biome goes where. " +
                                    "There is one, and only one, Biome Selector per terrain.",
                                    MessageType.Info, true);

            generatorModulesComponent.BiomeSelector = (BiomeSelector) EditorGUILayout.ObjectField("Asset", generatorModulesComponent.BiomeSelector, typeof(BiomeSelector), false);
            if (generatorModulesComponent.BiomeSelector) {
                var biomeSelector = generatorModulesComponent.BiomeSelector;
                Undo.RecordObject(biomeSelector, "Biome selector change");
                if (GUILayout.Button("Edit Biome Selector")) {
                    biomeSelector.Init();
                    var editorWindow = EditorWindow.GetWindow<NodeBasedEditor>("Generation Flow", true, dockToWindows);
                    editorWindow.Init(terrain, biomeSelector);
                }
            } else if (GUILayout.Button("New Biome Selector")) {
                generatorModulesComponent.Init();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Biomes:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This is where you define biomes. There must be at least one biome." +
                                    "Click on 'Edit biome' to edit the generation workflow of a biome.",
                                    MessageType.Info, true);
            biomesInspector.DisplayListInspector(generatorModulesComponent.Biomes);
            EditorGUILayout.EndVertical();
        }
    }
}