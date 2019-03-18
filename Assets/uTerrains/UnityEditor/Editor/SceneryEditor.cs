using UltimateTerrains;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    public sealed class SceneryEditor
    {
        private const int DetailsSubTab = 0;
        private const int TreesSubTab = 1;

        private UltimateTerrain terrain;
        private DetailsParamEditor detailsEditor;
        private TreesParamEditor treesEditor;
        private ListInspector<DetailsParam> detailsInspector;
        private ListInspector<TreesParam> treesInspector;
        
        private Texture2D detailIcon;
        private Texture2D treeIcon;


        public void OnEnable(UltimateTerrain terrain)
        {
            this.terrain = terrain;
            detailIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/detail");
            treeIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/tree");

            if (detailsEditor == null)
                detailsEditor = new DetailsParamEditor();
            detailsEditor.OnEnable(terrain);

            if (treesEditor == null)
                treesEditor = new TreesParamEditor();
            treesEditor.OnEnable(terrain);

            if (detailsInspector == null) {
                detailsInspector = new ListInspector<DetailsParam>(
                    "Add detail group", null, "Do you really want to delete this detail group?", false, (item, i) => {
                        detailsEditor.OnInspectorGUI(item);
                        return item;
                    },
                    // Create
                    () => new DetailsParam(),
                    // Remove
                    i => i >= 0,
                    false,
                    "Name");
            }

            if (treesInspector == null) {
                treesInspector = new ListInspector<TreesParam>(
                    "Add tree group", null, "Do you really want to delete this tree group?", false, (item, i) => {
                        treesEditor.OnInspectorGUI(item);
                        return item;
                    },
                    // Create
                    () => new TreesParam(),
                    // Remove
                    i => i >= 0,
                    false,
                    "Name");
            }
        }

        public void OnInspectorGUI()
        {
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            var currentSubTab = EditorPrefs.GetInt("uTerrainsScenerySubTab", 0);
            var subTab = GUILayout.Toolbar(currentSubTab, new[]
            {
                new GUIContent(" Details", detailIcon),
                new GUIContent(" Trees / Prefabs", treeIcon)
            });
            if (currentSubTab != subTab) {
                EditorPrefs.SetInt("uTerrainsScenerySubTab", subTab);
            }
            EditorGUIUtility.SetIconSize(Vector2.zero);

            switch (subTab) {
                case DetailsSubTab:
                    EditorGUILayout.Space();
                    DrawDetailsInspector();
                    break;
                case TreesSubTab:
                    EditorGUILayout.Space();
                    DrawTreesInspector();
                    break;
            }
        }

        public void OnDisable()
        {
            detailsEditor.OnDisable();
            treesEditor.OnDisable();
        }

        private void DrawDetailsInspector()
        {
            var param = terrain.ParamsForEditor;

            EditorGUILayout.HelpBox("Details are GPU instanced for speed. A detail is defined by a mesh and a material. " +
                                    "It cannot have colliders.\n\nUse details to add small objects to decorate the ground, like stones, wood sticks, plants, etc.",
                                    MessageType.Info, true);

            param.MaxDetailsCountPerChunkPerType = EditorGUILayout.IntSlider(
                new GUIContent("Max details count per chunk:", "Max details objects count to spawn per chunk and per detail type. Let it as low as possible to avoid using to much memory."),
                param.MaxDetailsCountPerChunkPerType, 1, 1023);

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Details Groups:", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var details = param.DetailsParams;
            detailsInspector.DisplayArrayInspector(ref details, param.DetailsFoldoutForEditor);
            param.DetailsParams = details;

            EditorGUILayout.EndVertical();
        }

        private void DrawTreesInspector()
        {
            var param = terrain.ParamsForEditor;

            EditorGUILayout.HelpBox("Trees are spawned as normal GameObjects. They can have colliders.\n\n" +
                                    "Note that if they do have colliders, they will automatically be removed when a terrain operation " +
                                    "affects them. This behavior can be disabled by adding the 'TreeComponent' component to your tree prefab " +
                                    "and disabling 'RemoveOnAffectedByOperation'.",
                                    MessageType.Info, true);

            var maxLevelTreesLongLabel = "Trees will be enabled on all chunks with a level lesser or equal to this.";
            EditorGUILayout.LabelField(new GUIContent("Max LOD with trees.", maxLevelTreesLongLabel));
            param.MaxLevelIndexTrees = EditorGUILayout.IntSlider(new GUIContent("Max level:", maxLevelTreesLongLabel), param.MaxLevelIndexTrees, 1, 7);
            var treeSpawningMaxCountPerFrameLongLabel = "This is the maximum number of trees that can be spawned, per frame.";
            EditorGUILayout.LabelField(new GUIContent("Max tree count spawned per frame", treeSpawningMaxCountPerFrameLongLabel));
            param.TreeSpawningMaxCountPerFrame = EditorGUILayout.IntField(new GUIContent("Max trees spawned / frame:", treeSpawningMaxCountPerFrameLongLabel), param.TreeSpawningMaxCountPerFrame);
            EditorGUILayout.Space();

            var treeDensityStepLongLabel = "Average voxel count between trees (ie. average distance between two trees)";
            EditorGUILayout.LabelField(new GUIContent("Trees density", treeDensityStepLongLabel));
            param.TreeDensityStep = EditorGUILayout.IntField(new GUIContent("Density:", treeDensityStepLongLabel), param.TreeDensityStep);
            var treeDensityStepNoiseFrequencyLongLabel = "Density variation noise frequency";
            EditorGUILayout.LabelField(new GUIContent("Trees density noise frequency", treeDensityStepNoiseFrequencyLongLabel));
            param.TreeDensityStepNoiseFrequency = EditorGUILayout.FloatField(new GUIContent("Density noise:", treeDensityStepNoiseFrequencyLongLabel), (float) param.TreeDensityStepNoiseFrequency);
            var treeVerticalStepLongLabel = "Min height a trees need to have enough space.";
            EditorGUILayout.LabelField(new GUIContent("Trees height", treeVerticalStepLongLabel));
            param.TreeVerticalStep = EditorGUILayout.IntField(new GUIContent("Trees height:", treeVerticalStepLongLabel), param.TreeVerticalStep);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Trees groups:", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var trees = param.TreesParams;
            treesInspector.DisplayArrayInspector(ref trees, param.TreesFoldoutForEditor);
            param.TreesParams = trees;

            EditorGUILayout.EndVertical();
        }
    }
}