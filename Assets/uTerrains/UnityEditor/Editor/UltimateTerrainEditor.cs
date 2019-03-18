using System.Globalization;
using System.IO;
using UltimateTerrains;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    [CustomEditor(typeof(UltimateTerrain))]
    public sealed class UltimateTerrainEditor : Editor
    {
        private const int VoxelTypesTab = 0;
        private const int BiomesTab = 1;
        private const int SceneryTab = 2;
        private const int EditorTab = 3;
        private const int GlobalSettingsTab = 4;
        private const int DocumentationTab = 5;

        private const int ForceRepaintEachNIteration = 10;

        private int repaintCounter;

        // Sub-editors
        private GeneratorModulesEditor generatorModulesEditor;
        private VoxelTypeSetEditor voxelTypeSetEditor;
        private TerrainToolEditor terrainToolEditor;
        private SceneryEditor sceneryEditor;

        private UltimateTerrain terrain;
        private GeneratorModulesComponent generatorModulesComponent;

        private Texture2D settingsIcon;
        private Texture2D voxelIcon;
        private Texture2D biomesIcon;
        private Texture2D sceneryIcon;
        private Texture2D terrainToolIcon;
        private Texture2D documentationIcon;
        private GUIStyle bigTabsStyle;

        public void OnEnable()
        {
            // get terrain instance
            terrain = (UltimateTerrain) target;
            EditorApplication.update += OnUpdate;

            generatorModulesComponent = terrain.GetComponent<GeneratorModulesComponent>();
            if (generatorModulesComponent == null) {
                generatorModulesComponent = terrain.gameObject.AddComponent<GeneratorModulesComponent>();
            }

            if (voxelTypeSetEditor == null)
                voxelTypeSetEditor = new VoxelTypeSetEditor();
            voxelTypeSetEditor.OnEnable(terrain.VoxelTypeSet, terrain);

            if (terrainToolEditor == null)
                terrainToolEditor = new TerrainToolEditor();
            terrainToolEditor.OnEnable(terrain, generatorModulesComponent);

            if (sceneryEditor == null)
                sceneryEditor = new SceneryEditor();
            sceneryEditor.OnEnable(terrain);

            if (generatorModulesEditor == null)
                generatorModulesEditor = new GeneratorModulesEditor();
            generatorModulesEditor.OnEnable(terrain, terrainToolEditor, generatorModulesComponent);
        }

        public void OnDisable()
        {
            voxelTypeSetEditor.OnDisable();
            terrainToolEditor.OnDisable();
            sceneryEditor.OnDisable();
        }

        private void InitInspectorGUI()
        {
            settingsIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/settings");
            voxelIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/voxel");
            biomesIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/biomes");
            sceneryIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/scenery");
            terrainToolIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/terrain-tool");
            documentationIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/documentation");

            bigTabsStyle = new GUIStyle(EditorStyles.toolbarButton);
            bigTabsStyle.fixedHeight = 0;
            bigTabsStyle.padding = new RectOffset(4, 4, 4, 4);
        }

        public override void OnInspectorGUI()
        {
            InitInspectorGUI();

            repaintCounter = 0;
            serializedObject.Update();
            GUI.skin.button.wordWrap = true;
            GUI.skin.label.wordWrap = true;

            EditorGUIUtility.SetIconSize(new Vector2(22, 22));
            EditorGUILayout.Space();
            var currentTab = EditorPrefs.GetInt("uTerrainsTab", 0);
            var mainTab = GUILayout.SelectionGrid(currentTab, new[]
            {
                new GUIContent(" Voxel types", voxelIcon),
                new GUIContent(" Biomes", biomesIcon),
                new GUIContent(" Scenery", sceneryIcon),
                new GUIContent(" Editor tool", terrainToolIcon),
                new GUIContent(" Global settings", settingsIcon),
                new GUIContent(" Documentation", documentationIcon)
            }, 3, bigTabsStyle);
            if (currentTab != mainTab) {
                EditorPrefs.SetInt("uTerrainsTab", mainTab);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUIUtility.SetIconSize(Vector2.zero);

            Undo.RecordObject(target, "uTerrain configuration");

            switch (mainTab) {
                case EditorTab:
                    terrainToolEditor.OnInspectorGUI();
                    break;

                case GlobalSettingsTab:
                    DrawParamsInspector();
                    break;

                case BiomesTab:
                    generatorModulesEditor.OnInspectorGUI();
                    break;

                case VoxelTypesTab:
                    voxelTypeSetEditor.OnInspectorGUI();
                    break;

                case SceneryTab:
                    sceneryEditor.OnInspectorGUI();
                    break;

                case DocumentationTab:
                    DrawDocumentationInspector();
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }


        private void OnUpdate()
        {
            if (terrainToolEditor != null)
                terrainToolEditor.OnUpdate();

            repaintCounter++;
            if (repaintCounter >= ForceRepaintEachNIteration) {
                repaintCounter = 0;
                Repaint();
            }
        }


        private void DrawParamsInspector()
        {
            if (terrainToolEditor.Active) {
                EditorGUILayout.HelpBox("Global Settings cannot be edited while the terrain is generated " +
                                        "or displayed in the scene view.\n\n" +
                                        "Please disable 'Generate & display terrain in scene view' if you want to edit them.",
                                        MessageType.Warning, true);
                terrainToolEditor.ToggleDrawTerrainInEditor(false, false);
                EditorGUILayout.Space();
                GUI.enabled = false;
            }

            var param = terrain.ParamsForEditor;
            MiscSettingsInspector(param);
            EditorGUILayout.Space();
            TerrainSizeInspector(param);
            EditorGUILayout.Space();
            DataLoadingInspector(param);
            EditorGUILayout.Space();
            GeometricErrorInspector(param);
            EditorGUILayout.Space();
            InitialChunkCountInspector(param);
            GUI.enabled = true;
        }

        private void InitialChunkCountInspector(Param param)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            terrain.InitialChunkCountFoldoutForEditor = EditorGUILayout.Foldout(terrain.InitialChunkCountFoldoutForEditor,
                                                                                "Initial Chunk Count Settings",
                                                                                EditorUtils.FoldoutGUIStyle(terrain.InitialChunkCountFoldoutForEditor));
            if (!terrain.InitialChunkCountFoldoutForEditor) {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Initial chunk count per LOD.");
            for (var i = 0; i < Param.MAX_LEVEL_COUNT; ++i) {
                var lvl = i + 1; //(int)Math.Pow (2, i);
                param.InitialChunkCountLevel[i] = EditorGUILayout.IntField(new GUIContent("For LOD " + lvl,
                                                                                          "Adjust the number of initial chunks count of level " + lvl +
                                                                                          ". There should be enough chunks to avoid creation of new ones during the game.\n\n" +
                                                                                          "A warning will be written in the console (while playing) if there is not enough chunks of a particular level."), param.InitialChunkCountLevel[i]);
            }

            // ---------------------------
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
        }

        private void DataLoadingInspector(Param param)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            terrain.DataLoadingFoldoutForEditor = EditorGUILayout.Foldout(terrain.DataLoadingFoldoutForEditor,
                                                                          "Data Path Settings",
                                                                          EditorUtils.FoldoutGUIStyle(terrain.DataLoadingFoldoutForEditor));
            if (!terrain.DataLoadingFoldoutForEditor) {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.HelpBox("Terrain data (ie. operations) is in the StreamingAssets/uTerrainsData folder so it " +
                                    "is automatically included in the final build of your project. See https://docs.unity3d.com/Manual/StreamingAssets.html " +
                                    "for more information.\n\n" +
                                    "Current actual base path is:\n" + param.BasePath,
                                    MessageType.Info, true);

            var dataPathLongLabel = "Data path determines where terrain data will be saved and where it will be loaded from.\n\n" +
                                    "This path must be relative to the base path. For example, if the base path is 'StreamingAssets/uTerrainsData' and the data path is 'foobar', then " +
                                    "data will be loaded from 'StreamingAssets/uTerrainsData/foobar/'.";
            EditorGUILayout.LabelField(new GUIContent("Data will be loaded from <base-path>/<data-path>", dataPathLongLabel));
            param.DataPath = EditorGUILayout.TextField(new GUIContent("Data path:", dataPathLongLabel), param.DataPath);

            EditorGUILayout.HelpBox("Terrain data will be loaded from:\n" + Path.Combine("StreamingAssets/uTerrainsData", param.DataPath),
                                    MessageType.Info, true);

            if (!param.IsTerrainInfinite) {
                param.LoadDynamically = false;
                GUI.enabled = false;
            }

            var loadDynamicallyLongLabel = "When this is enabled, data will be loaded region by region, dynamically. Finite terrains can't use this.";
            EditorGUILayout.LabelField(new GUIContent("Enables or disables dynamic loading.", loadDynamicallyLongLabel));
            param.LoadDynamically = EditorGUILayout.Toggle(new GUIContent("Enable loading:", loadDynamicallyLongLabel), param.LoadDynamically);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void TerrainSizeInspector(Param param)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            terrain.TerrainSizeFoldoutForEditor = EditorGUILayout.Foldout(terrain.TerrainSizeFoldoutForEditor,
                                                                          "Terrain Size Settings",
                                                                          EditorUtils.FoldoutGUIStyle(terrain.TerrainSizeFoldoutForEditor));
            if (!terrain.TerrainSizeFoldoutForEditor) {
                EditorGUILayout.EndVertical();
                return;
            }

            var voxelSizeLongLabel = "Size of a voxel in Unity's world. The smaller it is, the smaller the terrain will be, but you will be able to dig smaller holes or add smaller cubes.";
            EditorGUILayout.LabelField(new GUIContent("Size of a voxel in Unity's world", voxelSizeLongLabel));
            param.SizeXVoxel = EditorGUILayout.FloatField(new GUIContent("Size X:", voxelSizeLongLabel), (float) param.SizeXVoxel);
            param.SizeYVoxel = EditorGUILayout.FloatField(new GUIContent("Size Y:", voxelSizeLongLabel), (float) param.SizeYVoxel);
            param.SizeZVoxel = EditorGUILayout.FloatField(new GUIContent("Size Z:", voxelSizeLongLabel), (float) param.SizeZVoxel);

            EditorGUILayout.Space();

            var buildDistancetLongLabel = "Controls the draw size of the terrain by setting the number of chunks of lowest LOD that must be generated between the player and the borders. You should let it to 4 unless you set a very low LOD count (like 1 or 2).";
            EditorGUILayout.LabelField(new GUIContent("Number of root chunks between player and visible part borders", buildDistancetLongLabel));
            param.BuildDistance = EditorGUILayout.IntSlider(new GUIContent("Draw size (build distance):", buildDistancetLongLabel), param.BuildDistance, 4, 16);
            EditorGUILayout.Space();
            var verticalBuildDistancetLongLabel = "Controls the vertical draw size of the terrain by setting the number of chunks of lowest LOD that must be generated between the player and the borders. You should let it to 4 unless you set a very low LOD count (like 1 or 2).";
            EditorGUILayout.LabelField(new GUIContent("Number of root chunks between player and visible part borders (vertically)", verticalBuildDistancetLongLabel));
            param.VerticalBuildDistance = EditorGUILayout.IntSlider(new GUIContent("Vertical draw size (build distance):", verticalBuildDistancetLongLabel), param.VerticalBuildDistance, 2, 16);
            EditorGUILayout.Space();
            var levelCountLongLabel = "Increasing it by one will add one more level of detail, which will double the size of the terrain.";
            EditorGUILayout.LabelField(new GUIContent("Levels Of Detail count (LOD)", levelCountLongLabel));
            param.LevelCount = EditorGUILayout.IntSlider(new GUIContent("LOD count:", levelCountLongLabel), param.LevelCount, 1, Param.MAX_LEVEL_COUNT);
            // ---------------------------
            EditorGUILayout.HelpBox("Terrain size\n" +
                                    "        = 2x[Draw size] x 16x2^[LOD Count] x [Voxel size]\n" +
                                    "        = " + (2 * param.BuildDistance * Param.SIZE_X * (1 << (param.LevelCount - 1)) * param.SizeXVoxel).ToString(CultureInfo.InvariantCulture),
                                    MessageType.Info, true);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ---------------------------
            var lodDistanceLongLabel = "This represents the number of chunks of a level which will be drawn between two Levels Of Detail.";
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Distance from LOD 2 to LOD 1.", lodDistanceLongLabel));
            param.Lod2Distance = EditorGUILayout.IntSlider(new GUIContent("LOD 2 distance:", lodDistanceLongLabel), param.Lod2Distance, 1, 8);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Distance from LOD 4 to LOD 2.", lodDistanceLongLabel));
            param.Lod4Distance = EditorGUILayout.IntSlider(new GUIContent("LOD 4 distance:", lodDistanceLongLabel), param.Lod4Distance, 2, 8);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Distance from LOD 8 to LOD 4.", lodDistanceLongLabel));
            param.Lod8Distance = EditorGUILayout.IntSlider(new GUIContent("LOD 8 distance:", lodDistanceLongLabel), param.Lod8Distance, 2, 8);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Distance from LOD 16 to LOD 8.", lodDistanceLongLabel));
            param.Lod16Distance = EditorGUILayout.IntSlider(new GUIContent("LOD 16 distance:", lodDistanceLongLabel), param.Lod16Distance, 2, 8);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Distance from LOD 32 to LOD 16.", lodDistanceLongLabel));
            param.Lod32Distance = EditorGUILayout.IntSlider(new GUIContent("LOD 32 distance:", lodDistanceLongLabel), param.Lod32Distance, 2, 8);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Distance from LOD 64 to LOD 32.", lodDistanceLongLabel));
            param.Lod64Distance = EditorGUILayout.IntSlider(new GUIContent("LOD 64 distance:", lodDistanceLongLabel), param.Lod64Distance, 2, 8);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawMinMaxGUI();
            EditorGUILayout.EndVertical();
        }

        private void DrawMinMaxGUI()
        {
            EditorGUILayout.HelpBox("It is sometimes possible to limit the size of the terrain, which can improve performance as Ultimate Terrains won't " +
                                    "compute anything outside these limits. For example, " +
                                    "if your terrain is a land with mountains and valleys, maybe you can set the " +
                                    "Max Y to the height of the highest mountain and Min Y to the height of the deepest valley.", MessageType.Info, true);

            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190;

            EditorGUILayout.BeginVertical("Box");
            generatorModulesComponent.HasMinX = GUILayout.Toggle(generatorModulesComponent.HasMinX, "Set minimum X");
            GUI.enabled = generatorModulesComponent.HasMinX;
            generatorModulesComponent.MinX = EditorGUILayout.IntField("      Minimum X (in voxel units):", generatorModulesComponent.MinX);
            GUI.enabled = true;

            generatorModulesComponent.HasMinY = GUILayout.Toggle(generatorModulesComponent.HasMinY, "Set minimum Y");
            GUI.enabled = generatorModulesComponent.HasMinY;
            generatorModulesComponent.MinY = EditorGUILayout.IntField("      Minimum Y (in voxel units):", generatorModulesComponent.MinY);
            GUI.enabled = true;

            generatorModulesComponent.HasMinZ = GUILayout.Toggle(generatorModulesComponent.HasMinZ, "Set minimum Z");
            GUI.enabled = generatorModulesComponent.HasMinZ;
            generatorModulesComponent.MinZ = EditorGUILayout.IntField("      Minimum Z (in voxel units):", generatorModulesComponent.MinZ);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            generatorModulesComponent.HasMaxX = GUILayout.Toggle(generatorModulesComponent.HasMaxX, "Set maximum X");
            GUI.enabled = generatorModulesComponent.HasMaxX;
            generatorModulesComponent.MaxX = EditorGUILayout.IntField("      Maximum X (in voxel units):", generatorModulesComponent.MaxX);
            GUI.enabled = true;

            generatorModulesComponent.HasMaxY = GUILayout.Toggle(generatorModulesComponent.HasMaxY, "Set maximum Y");
            GUI.enabled = generatorModulesComponent.HasMaxY;
            generatorModulesComponent.MaxY = EditorGUILayout.IntField("      Maximum Y (in voxel units):", generatorModulesComponent.MaxY);
            GUI.enabled = true;

            generatorModulesComponent.HasMaxZ = GUILayout.Toggle(generatorModulesComponent.HasMaxZ, "Set maximum Z");
            GUI.enabled = generatorModulesComponent.HasMaxZ;
            generatorModulesComponent.MaxZ = EditorGUILayout.IntField("      Maximum Z (in voxel units):", generatorModulesComponent.MaxZ);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        private void GeometricErrorInspector(Param param)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            terrain.GeometricErrorFoldoutForEditor = EditorGUILayout.Foldout(terrain.GeometricErrorFoldoutForEditor,
                                                                             "Acceptable Geometric Error Settings",
                                                                             EditorUtils.FoldoutGUIStyle(terrain.GeometricErrorFoldoutForEditor));
            if (!terrain.GeometricErrorFoldoutForEditor) {
                EditorGUILayout.EndVertical();
                return;
            }

            var baseErrorLongLabel = "A smaller geometric error will produce a more accurate terrain, with more polygons, but it will be longer to compute and may drop performance.";
            EditorGUILayout.LabelField(new GUIContent("Acceptable geometric error for different LODs", baseErrorLongLabel));
            param.Lod1BaseError = EditorGUILayout.Slider(new GUIContent("Max error LOD 1:", baseErrorLongLabel), (float) param.Lod1BaseError, 0f, 1f);
            param.Lod2BaseError = EditorGUILayout.Slider(new GUIContent("Max error LOD 2:", baseErrorLongLabel), (float) param.Lod2BaseError, 0f, 1f);
            param.Lod4BaseError = EditorGUILayout.Slider(new GUIContent("Max error LOD 3:", baseErrorLongLabel), (float) param.Lod4BaseError, 0f, 1f);
            param.Lod8BaseError = EditorGUILayout.Slider(new GUIContent("Max error LOD 4:", baseErrorLongLabel), (float) param.Lod8BaseError, 0f, 1f);
            param.Lod16BaseError = EditorGUILayout.Slider(new GUIContent("Max error LOD 5:", baseErrorLongLabel), (float) param.Lod16BaseError, 0f, 1f);
            param.Lod32BaseError = EditorGUILayout.Slider(new GUIContent("Max error LOD 6:", baseErrorLongLabel), (float) param.Lod32BaseError, 0f, 1f);
            param.Lod64BaseError = EditorGUILayout.Slider(new GUIContent("Max error LOD 7:", baseErrorLongLabel), (float) param.Lod64BaseError, 0f, 1f);

            EditorGUILayout.Space();
            var signChangesErrorMultiplicatorLongLabel = "This coefficient will affect maximum geometric error on surface of the terrain. " +
                                                         "The final acceptable error will be equal to <max error> * <multiplier>. " +
                                                         "This is useful if you want to tell Ultimate Terrains to be more or less accurate near terrain surface in order to improve quality or reduce the number of polygons.";
            EditorGUILayout.LabelField(new GUIContent("Acceptable error multiplier on surface", signChangesErrorMultiplicatorLongLabel));
            param.Lod1SignChangesErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 1:", signChangesErrorMultiplicatorLongLabel), (float) param.Lod1SignChangesErrorMultiplicator, 0f, 10f);
            param.Lod2SignChangesErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 2:", signChangesErrorMultiplicatorLongLabel), (float) param.Lod2SignChangesErrorMultiplicator, 0f, 10f);
            param.Lod4SignChangesErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 3:", signChangesErrorMultiplicatorLongLabel), (float) param.Lod4SignChangesErrorMultiplicator, 0f, 10f);
            param.Lod8SignChangesErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 4:", signChangesErrorMultiplicatorLongLabel), (float) param.Lod8SignChangesErrorMultiplicator, 0f, 10f);
            param.Lod16SignChangesErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 5:", signChangesErrorMultiplicatorLongLabel), (float) param.Lod16SignChangesErrorMultiplicator, 0f, 10f);
            param.Lod32SignChangesErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 6:", signChangesErrorMultiplicatorLongLabel), (float) param.Lod32SignChangesErrorMultiplicator, 0f, 10f);
            param.Lod64SignChangesErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 7:", signChangesErrorMultiplicatorLongLabel), (float) param.Lod64SignChangesErrorMultiplicator, 0f, 10f);

            EditorGUILayout.Space();
            var borderErrorMultiplicatorLongLabel = "This coefficient will affect maximum geometric error on chunk borders. " +
                                                    "The final acceptable error will be equal to <max error> * <multiplier>. " +
                                                    "This can be useful to force Ultimate Terrain to be more accurate on chunk borders and avoid potential holes in the terrain.";
            EditorGUILayout.LabelField(new GUIContent("Acceptable error multiplier on chunk borders", borderErrorMultiplicatorLongLabel));
            param.Lod1BorderErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 1:", borderErrorMultiplicatorLongLabel), (float) param.Lod1BorderErrorMultiplicator, 0f, 1f);
            param.Lod2BorderErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 2:", borderErrorMultiplicatorLongLabel), (float) param.Lod2BorderErrorMultiplicator, 0f, 1f);
            param.Lod4BorderErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 3:", borderErrorMultiplicatorLongLabel), (float) param.Lod4BorderErrorMultiplicator, 0f, 1f);
            param.Lod8BorderErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 4:", borderErrorMultiplicatorLongLabel), (float) param.Lod8BorderErrorMultiplicator, 0f, 1f);
            param.Lod16BorderErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 5:", borderErrorMultiplicatorLongLabel), (float) param.Lod16BorderErrorMultiplicator, 0f, 1f);
            param.Lod32BorderErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 6:", borderErrorMultiplicatorLongLabel), (float) param.Lod32BorderErrorMultiplicator, 0f, 1f);
            param.Lod64BorderErrorMultiplicator = EditorGUILayout.Slider(new GUIContent("Max error multiplier LOD 7:", borderErrorMultiplicatorLongLabel), (float) param.Lod64BorderErrorMultiplicator, 0f, 1f);

            EditorGUILayout.Space();
            var nearSurfacePrecisionInternalLongLabel = "This is the distance from the real surface of the terrain where a voxel will be considered to be near the surface. " +
                                                        "When a voxel is near the surface, a special effort is made to compute the geometry around it, avoiding potential holes in the terrain.\n\n" +
                                                        "0.5 is generally a good value";
            EditorGUILayout.LabelField(new GUIContent("Distance to be \"near the surface\"", nearSurfacePrecisionInternalLongLabel));
            param.NearSurfacePrecision = EditorGUILayout.FloatField(new GUIContent("Distance:", nearSurfacePrecisionInternalLongLabel), (float) param.NearSurfacePrecision);

            EditorGUILayout.EndVertical();
        }

        private void MiscSettingsInspector(Param param)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            terrain.MiscSettingsFoldoutForEditor = EditorGUILayout.Foldout(terrain.MiscSettingsFoldoutForEditor,
                                                                           "General Settings",
                                                                           EditorUtils.FoldoutGUIStyle(terrain.MiscSettingsFoldoutForEditor));
            if (!terrain.MiscSettingsFoldoutForEditor) {
                EditorGUILayout.EndVertical();
                return;
            }

            var playerTransformLongLabel = "Select the main player object (could be the main camera). Terrain will be dynamically generated around it.";
            terrain.PlayerTransform = (Transform) EditorGUILayout.ObjectField(new GUIContent("Player transform:", playerTransformLongLabel), terrain.PlayerTransform, typeof(Transform), true);

            var infiniteTerrainLongLabel = "When enabled, the terrain will be dynamically generated around the player while he is moving.";
            param.IsTerrainInfinite = EditorGUILayout.Toggle(new GUIContent("Infinite Terrain:", infiniteTerrainLongLabel), param.IsTerrainInfinite);

            var chunkConnectionModeLongLabel = "Controls the way chunks will be visually connected.\n\n" +
                                               "'Properly Connected Seams' creates perfect connections. This offers the best rendering.\n\n" +
                                               "'Skirts' creates skirts that hides holes between chunks, but may create visual artifacts. This offers better performance.";
            param.ChunkConnectionMode = (ChunkConnectionMode) EditorGUILayout.EnumPopup(new GUIContent("Chunk connection mode:", chunkConnectionModeLongLabel), param.ChunkConnectionMode);

            var duplicateVerticesLongLabel = "When enabled, vertices will be duplicated to prevent textures stretching, but this could increase generation time.";
            param.DuplicateVertices = EditorGUILayout.Toggle(new GUIContent("Duplicate vertices:", duplicateVerticesLongLabel), param.DuplicateVertices);

            var computeTangentsOnMeshesLongLabel = "If this is enabled, tangents of vertices will be computed, which can be useful for some shaders.";
            param.ComputeTangentsOnMeshes = EditorGUILayout.Toggle(new GUIContent("Compute tangents:", computeTangentsOnMeshesLongLabel), param.ComputeTangentsOnMeshes);

            EditorGUILayout.Space();
            var maxLevelWithCacheLongLabel = "All chunks with a level lesser or equal to this will keep their data in cache. Operations will be computed faster on those chunks, but it will use more memory.";
            param.MaxLevelWithCache = EditorGUILayout.IntSlider(new GUIContent("Max LOD with cache:", maxLevelWithCacheLongLabel), param.MaxLevelWithCache, 0, Param.MAX_LEVEL_COUNT);
            var maxLevelColliderLongLabel = "All chunks with a level lesser or equal to this will have a mesh collider.";
            param.MaxLevelIndexCollider = EditorGUILayout.IntSlider(new GUIContent("Max LOD with colliders:", maxLevelColliderLongLabel), param.MaxLevelIndexCollider, 1, Param.MAX_LEVEL_COUNT);
            var maxLevelShadowsLongLabel = "Shadows will be enabled on all chunks with a level lesser or equal to this.";
            param.MaxLevelIndexShadows = EditorGUILayout.IntSlider(new GUIContent("Max LOD with shadows:", maxLevelShadowsLongLabel), param.MaxLevelIndexShadows, 1, Param.MAX_LEVEL_COUNT);
            var maxLevelGrassLongLabel = "Grass will be enabled on all chunks with a level lesser or equal to this.";
            param.MaxLevelIndexGrass = EditorGUILayout.IntSlider(new GUIContent("Max LOD with grass:", maxLevelGrassLongLabel), param.MaxLevelIndexGrass, 1, Param.MAX_LEVEL_COUNT);
            var maxLevelDetailsLongLabel = "Detail objects will be enabled on all chunks with a level lesser or equal to this. Max LOD with details can't be higher than max LOD with colliders.";
            param.MaxLevelIndexDetails = EditorGUILayout.IntSlider(new GUIContent("Max LOD with details:", maxLevelDetailsLongLabel), param.MaxLevelIndexDetails, 1, param.MaxLevelIndexCollider);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            var postBuildMaxElapsedTimePerFrameLongLabel = "This is the approximate maximum number of milliseconds to be spent for post-building, per frame. " +
                                                           "Post-building consists in updating mesh of the mesh filter and the collider, which can only be made on the main thread.";
            EditorGUILayout.LabelField(new GUIContent("Approx max time spent for post-building per frame", postBuildMaxElapsedTimePerFrameLongLabel));
            param.PostBuildMaxElapsedTimePerFrame = EditorGUILayout.IntField(new GUIContent("Max ms / frame:", postBuildMaxElapsedTimePerFrameLongLabel), param.PostBuildMaxElapsedTimePerFrame);
            EditorGUILayout.Space();
            var postBuildMaxCountPerFrameLongLabel = "This is the maximum number of chunks that can be post-builded, per frame. " +
                                                     "Post-building consists in updating mesh of the mesh filter and the collider, which can only be made on the main thread.";
            EditorGUILayout.LabelField(new GUIContent("Max count of post-building per frame", postBuildMaxCountPerFrameLongLabel));
            param.PostBuildMaxCountPerFrame = EditorGUILayout.IntField(new GUIContent("Max post-building / frame:", postBuildMaxCountPerFrameLongLabel), param.PostBuildMaxCountPerFrame);
            // ------------------------------------------------------------------------

            // ------------------------------------------------------------------------
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Choose the layer of terrain's chunks");
            param.ChunkLayer = EditorGUILayout.LayerField("Chunk layer:", param.ChunkLayer);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Choose the layer of terrain's grass");
            param.GrassLayer = EditorGUILayout.LayerField("Grass layer:", param.GrassLayer);

            EditorGUILayout.EndVertical();
        }


        private void DrawDocumentationInspector()
        {
            EditorGUILayout.HelpBox("Latest documentation as well as support are exclusively online.\n\n" +
                                    "If you have some questions, don't get stuck, ask us! And fill free to make some suggestions too.",
                                    MessageType.Info, true);

            if (GUILayout.Button("Open documentation")) {
                Application.OpenURL("https://uterrains.com/doc/");
            }

            if (GUILayout.Button("Ask questions / Find answers")) {
                Application.OpenURL("https://uterrains.com/questions-answers/");
            }

            if (GUILayout.Button("Go to official forum thread")) {
                Application.OpenURL("https://forum.unity.com/threads/uterrains-ultimate-terrains-voxel-terrain-engine.383959/");
            }

            if (GUILayout.Button("Report a bug")) {
                Application.OpenURL("https://uterrains.com/report/");
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Good reviews on the Asset Store help supporting the project a lot. " +
                                    "If you like Ultimate Terrains, please let others know!",
                                    MessageType.Info, true);
            if (GUILayout.Button("Write a review on the Asset Store")) {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/terrain/ultimate-terrains-voxel-terrain-engine-31100");
            }
        }

        [MenuItem("Tools/uTerrains/Support")]
        public static void OpenSupportWebPage()
        {
            Application.OpenURL("https://uterrains.com/questions-answers/");
        }

        [MenuItem("Tools/uTerrains/Report a bug")]
        public static void OpenReportBugWebPage()
        {
            Application.OpenURL("https://uterrains.com/report/");
        }
    }
}