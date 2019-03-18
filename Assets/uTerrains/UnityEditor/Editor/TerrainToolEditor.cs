using System;
using System.Linq;
using UltimateTerrains;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UltimateTerrainsEditor
{
    public sealed class TerrainToolEditor
    {
        private UltimateTerrain terrain;
        private GeneratorModulesComponent generatorModulesComponent;
        private bool showLoadingLabel;

        private string[] operationTypesString;
        private IOperationEditor[] operations;
        private int chosenOpIndex;


        public bool ContinuousEditing { get; private set; }
        public bool Clicking { get; private set; }
        private bool continuousEditing;
        private bool needSave;
        private Vector3 reticleWorldPos;

        private GameObject sceneViewCameraPivot;

        public UltimateTerrain Terrain {
            get { return terrain; }
        }

        public bool Active {
            get { return terrain != null && EditorPrefs.GetBool("uTerrainsEditorToolActive", false); }
            set { EditorPrefs.SetBool("uTerrainsEditorToolActive", value); }
        }

        public static bool FollowSceneViewCamera {
            get { return EditorPrefs.GetBool("uTerrainsEditorToolFollowSceneViewCamera", true); }
            set { EditorPrefs.SetBool("uTerrainsEditorToolFollowSceneViewCamera", value); }
        }

        public static bool DoAutosave {
            get { return EditorPrefs.GetBool("uTerrainsEditorToolDoAutosave", true); }
            set { EditorPrefs.SetBool("uTerrainsEditorToolDoAutosave", value); }
        }

        public static bool DoAutoMerge {
            get { return EditorPrefs.GetBool("uTerrainsEditorToolDoAutoMerge", true); }
            set { EditorPrefs.SetBool("uTerrainsEditorToolDoAutoMerge", value); }
        }

        public static int LODCountForEditor {
            get { return EditorPrefs.GetInt("uTerrainsLODCountForEditor", 6); }
            set { EditorPrefs.SetInt("uTerrainsLODCountForEditor", value); }
        }

        public static int MinLODLevelInEditor {
            get { return EditorPrefs.GetInt("uTerrainsMinLODLevelInEditor", 1); }
            set { EditorPrefs.SetInt("uTerrainsMinLODLevelInEditor", value); }
        }

        public static bool DoNotGenerateTreesInEditor {
            get { return EditorPrefs.GetBool("uTerrainsDoNotGenerateTreesInEditor", true); }
            set { EditorPrefs.SetBool("uTerrainsDoNotGenerateTreesInEditor", value); }
        }

        public TerrainToolEditor()
        {
            UltimateTerrain.OnLoaded += OnTerrainLoaded;
        }

        public void OnEnable(UltimateTerrain terrain, GeneratorModulesComponent generatorModulesComponent)
        {
            this.terrain = terrain;
            this.generatorModulesComponent = generatorModulesComponent;
            this.chosenOpIndex = 0;
            GetAllTypesOfOperations();
            SceneView.onSceneGUIDelegate += OnScene;
        }

        public void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnScene;
            chosenOpIndex = 0;
            DestroyAllReticles();
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("This tool allows you to edit the terrain from Unity's editor, within the scene view, as you would do with a standard Unity terrain.", EditorStyles.wordWrappedLabel);

            ToggleDrawTerrainInEditor();
            if (MinLODLevelInEditor > 1) {
                var bkgCol = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                EditorGUILayout.HelpBox("CAUTION: it is recommended to keep 'Lowest LOD to compute' value to 1 when you are " +
                                        "using the Editor-Tool. " +
                                        "Otherwise you will miss some details and operations won't be accurate.\n\n" +
                                        "You should set 'Lowest LOD to compute' value to 1.",
                                        MessageType.Error, true);
                GUI.backgroundColor = bkgCol;
            }

            EditorGUILayout.LabelField("Choose an operation to perform on the terrain from the scene view:", EditorStyles.wordWrappedLabel);
            var oldChosenOpIndex = chosenOpIndex;
            chosenOpIndex = EditorGUILayout.Popup(chosenOpIndex, operationTypesString);
            // Destroy all reticles when changing operation
            if (oldChosenOpIndex != chosenOpIndex) {
                DestroyAllReticles();
            }

            ContinuousEditing = EditorGUILayout.ToggleLeft("Continuous editing", ContinuousEditing);

            // Display Operation Inspector
            if (chosenOpIndex > 0) {
                operations[chosenOpIndex].OnInspector(this);
            }

            if (GUILayout.Button("Undo")) {
                UndoOperation();
            }
        }

        public void ToggleDrawTerrainInEditor(bool showBuildOptions = true, bool showAllOptions = true, string label = "Generate & display terrain in scene view")
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var bkgCol = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.62f, 1f, 0f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = bkgCol;
            var style = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Bold};
            var newActive = EditorGUILayout.ToggleLeft(label, Active, style);
            if (newActive && (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)) {
                newActive = false;
            }

            if (!Active && newActive && !generatorModulesComponent.Validate(terrain, true)) {
                newActive = false;
                EditorUtility.DisplayDialog("Configuration Issue",
                                            "Looks like your terrain's configuration is not valid.\n" +
                                            "There should be some error messages in the Console that give more details about the issue.",
                                            "Ok");
            }

            EditorGUILayout.EndVertical();

            if (showBuildOptions) {
                style = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Italic};
                GUI.enabled = !Active;
                LODCountForEditor = EditorGUILayout.IntSlider("LOD count in editor", LODCountForEditor, 1, Param.MAX_LEVEL_COUNT);
                MinLODLevelInEditor = EditorGUILayout.IntSlider("Lowest LOD to compute", MinLODLevelInEditor, 1, LODCountForEditor);
                DoNotGenerateTreesInEditor = EditorGUILayout.ToggleLeft("Do NOT generate trees in editor", DoNotGenerateTreesInEditor, style);
                GUI.enabled = true;
            }

            if (showAllOptions) {
                style = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Italic};
                FollowSceneViewCamera = EditorGUILayout.ToggleLeft("Generate around scene-view camera instead of player object",
                                                                   FollowSceneViewCamera, style);
                DoAutoMerge = EditorGUILayout.ToggleLeft("Auto-merge operations (optimize operations but cannot be undone)",
                                                         DoAutoMerge, style);
                DoAutosave = EditorGUILayout.ToggleLeft("Auto-save (persist operations as soon as they are performed)",
                                                        DoAutosave, style);

                GUI.enabled = terrain.IsStarted && !terrain.IsStarting && Application.isEditor && !Application.isPlaying;
                if (!DoAutosave && GUILayout.Button("Save (persist operations to disk)")) {
                    Save(false);
                }

                GUI.enabled = true;
            }

            if (showLoadingLabel) {
                style = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.BoldAndItalic,
                    normal = {textColor = new Color32(255, 159, 0, 255)},
                    alignment = TextAnchor.MiddleCenter
                };
                EditorGUILayout.LabelField("Generating terrain, please wait...", style);
            }

            EditorGUILayout.EndVertical();

            if (Active != newActive) {
                Active = newActive;
                if (Active) {
                    StartTerrain();
                    showLoadingLabel = true;
                } else {
                    ClearTerrain();
                    showLoadingLabel = false;
                }
            }
        }

        private void OnTerrainLoaded(UltimateTerrain sender)
        {
            showLoadingLabel = false;
            SceneView.RepaintAll();
        }

        public void OnUpdate()
        {
            if (!Active)
                return;

            OverridePlayerForTerrain();
            terrain.Update();
            terrain.Orchestrator.MoveNextFromEditor();
        }

        private void OnScene(SceneView sceneview)
        {
            if (!Active)
                return;

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;
            if (e.type == EventType.Layout || e.type == EventType.Repaint) {
                HandleUtility.AddDefaultControl(controlId);
                return;
            }

            if (!e.alt && e.type == EventType.MouseDown && e.button == 0) {
                Clicking = true;
            } else if (!ContinuousEditing || e.type == EventType.MouseUp) {
                Clicking = false;
            }

            if (chosenOpIndex > 0) {
                operations[chosenOpIndex].OnScene(this, sceneview);
                HandleUtility.Repaint();
            }

            if (e.type != EventType.Used && needSave) {
                needSave = false;
                Autosave(false);
            }
        }

        public void PerformOperation(IOperation operation, SceneView sceneview = null)
        {
            terrain.OperationsManager.Add(operation, DoAutoMerge).PerformAll(false);
            needSave = true;
            if (sceneview != null) sceneview.Repaint();
        }

        private void UndoOperation(int undoCount = 1)
        {
            terrain.OperationsManager.Undo(undoCount, false);
            Autosave(true);
        }

        private void Autosave(bool clearOldData)
        {
            if (DoAutosave) {
                Save(clearOldData);
            }
        }

        private void Save(bool clearOldData)
        {
            if (terrain.IsStarted && !terrain.IsStarting && Application.isEditor && !Application.isPlaying) {
                terrain.OperationsManager.Persist(clearOldData);
            }
        }

        private void StartTerrain()
        {
            if (!terrain.IsStarted) {
                ClearTerrain();
                terrain.StartFromEditor(OverrideParam, UnitConverter.LevelIndexToLevel(MinLODLevelInEditor - 1));
                OverridePlayerForTerrain();
            } else {
                // start threads in case they've been stopped.
                terrain.Unpause();
            }
        }

        private void OverrideParam(Param param)
        {
            param.LevelCount = LODCountForEditor;
            //param.BuildDistance = 4;
            //param.VerticalBuildDistance = 4;
            param.PreventTreesSpawning = DoNotGenerateTreesInEditor;
        }

        private void OverridePlayerForTerrain()
        {
            if (FollowSceneViewCamera) {
                if (!sceneViewCameraPivot) {
                    sceneViewCameraPivot = new GameObject("sceneViewCameraPivot");
                    sceneViewCameraPivot.hideFlags = HideFlags.HideAndDontSave;
                }

                if (SceneView.lastActiveSceneView != null) {
                    sceneViewCameraPivot.transform.position = SceneView.lastActiveSceneView.pivot;
                }

                terrain.OverridePlayerTransform(sceneViewCameraPivot.transform);
            } else {
                // reset to default
                terrain.OverridePlayerTransform(terrain.PlayerTransform);
            }
        }

        private void StopTerrain()
        {
            if (terrain.IsStarted) {
                terrain.Pause();
            }

            DestroyAllReticles();
            GC.Collect();
        }

        private void DestroyAllReticles()
        {
            for (var i = 0; i < operations.Length; ++i) {
                if (operations[i] != null) {
                    operations[i].DestroyReticle();
                }
            }
        }

        private void ClearTerrain()
        {
            StopTerrain();
            terrain.Reset();
            Resources.UnloadUnusedAssets();
            SceneView.RepaintAll();
            GC.Collect();
        }

        private void GetAllTypesOfOperations()
        {
            // Get all Operations (ie. all types implementing IOperation interface)
            var operationTypes = EditorUtils.GetAvailableTypes(typeof(IOperationEditor));

            // Compute equivalent array of operation type names
            var opCount = operationTypes.Length + 1;
            operationTypesString = new string[opCount];
            operations = new IOperationEditor[opCount];
            operationTypesString[0] = "-- select --";
            operations[0] = null;
            for (var i = 1; i < opCount; ++i) {
                operations[i] = (IOperationEditor) Activator.CreateInstance(operationTypes[i - 1]);
                operationTypesString[i] = operations[i].Name;
            }
        }

        public Vector3 GetFollowGrid(Vector3 worldPosition)
        {
            return (Vector3) terrain.Converter.VoxelToUnityPosition(terrain.Converter.UnityToVoxelPositionRound(worldPosition));
        }

        // Ray cast
        public RaycastHit? GetIntersectionWithTerrain(bool followGrid, bool moreOutside = false)
        {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 10000f)) {
                if (moreOutside)
                    hit.point += hit.normal * 0.5f;
                if (followGrid)
                    hit.point = GetFollowGrid(hit.point);

                return hit;
            }

            return default(RaycastHit);
        }

        public Vector3 SizeField(string label, Vector3 value, bool followGrid)
        {
            if (followGrid && terrain != null && terrain.Converter != null) {
                var newValueInt = terrain.Converter.UnityToVoxelPositionRound(EditorGUILayout.Vector3Field(label, value));
                if (newValueInt.x < 1)
                    newValueInt.x = 1;
                if (newValueInt.y < 1)
                    newValueInt.y = 1;
                if (newValueInt.z < 1)
                    newValueInt.z = 1;
                return (Vector3) terrain.Converter.VoxelToUnityPosition(newValueInt);
            }

            var newValue = EditorGUILayout.Vector3Field(label, value);
            if (newValue.x < 1f)
                newValue.x = 1f;
            if (newValue.y < 1f)
                newValue.y = 1f;
            if (newValue.z < 1f)
                newValue.z = 1f;
            return newValue;
        }

        public T LoadReticle<T>(string name) where T : Reticle
        {
            var reticlePrefab = Resources.Load(name) as GameObject;
            var retAddObj = Object.Instantiate(reticlePrefab, Vector3.zero, Quaternion.identity);
            var reticle = retAddObj.GetComponent<T>();
            reticle.Initialize();
            return reticle;
        }
    }
}