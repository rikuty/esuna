using UltimateTerrains;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    public sealed class VoxelTypeSetEditor
    {
        private GUIStyle div;

        private UltimateTerrain terrain;

        private VoxelTypeSet voxelTypeSet;
        private readonly ListInspector<Material> materialsInspector;
        private readonly ListInspector<Material> grassMaterialsInspector;
        private readonly ListInspector<VoxelType> voxelTypesInspector;
        private readonly GrassParamEditor grassInspector;

        public VoxelTypeSetEditor()
        {
            grassInspector = new GrassParamEditor();

            materialsInspector = new ListInspector<Material>(
                "Add Terrain Material", null, "Do you really want to remove this Material?", true, (item, i) => {
                    item = (Material) EditorGUILayout.ObjectField("Material " + i + ":", item, typeof(Material), false);
                    return item;
                },
                // Create
                () => { return null; },
                // Remove
                i => { return i > 0; });

            grassMaterialsInspector = new ListInspector<Material>(
                "Add Grass Material", null, "Do you really want to remove this Grass Material?", true, (item, i) => {
                    item = (Material) EditorGUILayout.ObjectField("Grass material " + i + ":", item, typeof(Material), false);
                    return item;
                },
                // Create
                () => { return null; },
                // Remove
                i => { return true; });

            voxelTypesInspector = new ListInspector<VoxelType>(
                "Add Voxel Type", null, "Do you really want to remove this Voxel Type?", false,
                // Voxel Type inspector
                (vtype, i) => {
                    var param = terrain.ParamsForEditor;
                    
                    vtype.Name = EditorGUILayout.TextField("Name:", vtype.Name);
                    if (i > 0 && !IsVoxelTypeNameUnique(vtype.Name)) {
                        EditorGUILayout.HelpBox("Name must be unique.",
                                                MessageType.Error, true);
                    }

                    vtype.MaterialIndex = (ushort) EditorGUILayout.IntSlider("Material index:", vtype.MaterialIndex, 0, voxelTypeSet.Materials.Length - 1);
                    vtype.Priority = EditorGUILayout.IntField("Priority:", vtype.Priority);
                    vtype.Blockiness = EditorGUILayout.Slider("Blockiness:", vtype.Blockiness, 0f, 1f);

                    // Functions
                    var currentScript = EditorUtils.GetMonoScriptOf(vtype.Functions);
                    var newScript = (MonoScript) EditorGUILayout.ObjectField(currentScript, typeof(MonoScript), false);
                    if (currentScript != newScript) {
                        vtype.Functions = UEditor.GetSerializableInstance<AbstractVoxelTypeFunctions>(newScript != null ? newScript.GetClass() : null);
                    }

                    if (vtype.Functions != null) {
                        vtype.Functions.OnEditorGUI(terrain, vtype);
                    } else {
                        vtype.Functions = (AbstractVoxelTypeFunctions) ScriptableObject.CreateInstance(typeof(DefaultVoxelTypeFunctions));
                        // It's annoying to set each AbstractVoxelTypeFunctions for 50 MegaSplat types, so we add it automatically if we detect that MegaSplat is used
                        if (voxelTypeSet.Materials.Length > vtype.MaterialIndex &&
                            voxelTypeSet.Materials[vtype.MaterialIndex] != null &&
                            voxelTypeSet.Materials[vtype.MaterialIndex].shader.name.Contains("MegaSplat")) {
                            vtype.Functions = (AbstractVoxelTypeFunctions) ScriptableObject.CreateInstance(typeof(MegaSplatVoxelTypeFunctions));
                        } else {
                            vtype.Functions = (AbstractVoxelTypeFunctions) ScriptableObject.CreateInstance(typeof(DefaultVoxelTypeFunctions));
                        }
                    }

                    // Grass
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Grass:", EditorStyles.boldLabel);
                    vtype.IsGrassEnabled = EditorGUILayout.Toggle("Enable grass:", vtype.IsGrassEnabled);
                    if (vtype.IsGrassEnabled) {
                        if (vtype.GrassParam == null) {
                            vtype.GrassParam = new GrassParam();
                        }

                        grassInspector.OnInspectorGUI(vtype.GrassParam);
                    }

                    EditorGUILayout.EndVertical();

                    // Details
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Details:", EditorStyles.boldLabel);
                    var details = param.DetailsParams;
                    if (details != null) {
                        var selectedDetails = new bool[details.Length];
                        for (var d = 0; d < details.Length; ++d) {
                            if (vtype.EnabledDetails != null && vtype.EnabledDetails.Length > d)
                                selectedDetails[d] = vtype.EnabledDetails[d];
                            selectedDetails[d] = EditorGUILayout.Toggle("Enable details '" + details[d].Name + "':", selectedDetails[d]);
                        }

                        vtype.EnabledDetails = selectedDetails;
                    }

                    EditorGUILayout.EndVertical();

                    // Trees
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Trees:", EditorStyles.boldLabel);
                    var trees = param.TreesParams;
                    if (trees != null) {
                        vtype.EnabledTreesIndex = EditorGUILayout.IntPopup("Tree group", vtype.EnabledTreesIndex,
                                                                           param.TreesParamsNames,
                                                                           param.TreesParamsIndices);
                    }

                    EditorGUILayout.EndVertical();

                    return vtype;
                },
                // Create Voxel Type
                () => {
                    var vtype = new VoxelType();
                    vtype.Name = GenerateVoxelTypeName();
                    return vtype;
                },
                // Remove Voxel Type
                i => { return i > 0; },
                false,
                "Name");
        }

        public void OnEnable(VoxelTypeSet voxelTypeSet, UltimateTerrain terrain)
        {
            this.terrain = terrain;
            this.voxelTypeSet = voxelTypeSet;
            div = new GUIStyle();
            div.border = new RectOffset(1, 1, 1, 1);
        }

        public void OnDisable()
        {
        }

        public void OnInspectorGUI()
        {
            //GUI.backgroundColor = new Color32(56, 56, 56, 255);
            //EditorGUILayout.BeginVertical("Box");
            Undo.RecordObject(terrain, "Voxel Type configuration change");

            MaterialsInspectorGUI();
            EditorGUILayout.Space();
            GrassGeneralInspectorGUI();
            EditorGUILayout.Space();
            VoxelTypesInspectorGUI();
            //EditorGUILayout.EndVertical();
        }

        private void MaterialsInspectorGUI()
        {
            EditorUtils.BeginColoredVerticalBox(EditorUtils.MaterialYellow);

            EditorGUILayout.LabelField("Terrain Materials:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Ultimate Terrains supports multiple materials. This is where you add them. You can then use them in your Voxel Types. Note that using several materials increases the amount of draw calls.",
                                    MessageType.Info, true);
            EditorGUILayout.Space();

            var materials = voxelTypeSet.Materials;
            materialsInspector.DisplayArrayInspector(ref materials);
            voxelTypeSet.Materials = materials;

            EditorUtils.EndColoredVerticalBox();
        }

        private void GrassGeneralInspectorGUI()
        {
            var param = terrain.ParamsForEditor;
            
            EditorUtils.BeginColoredVerticalBox(EditorUtils.GrassGreen);

            EditorGUILayout.LabelField("Grass Global Settings:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Grass global settings and materials are defined here. You can then use grass materials by adding some grass to your Voxel Types.",
                                    MessageType.Info, true);

            param.GrassCastShadows = EditorGUILayout.ToggleLeft("Grass casts shadows", param.GrassCastShadows);
            param.GrassReceiveShadows = EditorGUILayout.ToggleLeft("Grass receives shadows", param.GrassReceiveShadows);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Grass Materials:", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var grassMaterials = voxelTypeSet.GrassMaterials;
            grassMaterialsInspector.DisplayArrayInspector(ref grassMaterials);
            voxelTypeSet.GrassMaterials = grassMaterials;

            EditorUtils.EndColoredVerticalBox();
        }

        private void VoxelTypesInspectorGUI()
        {
            EditorUtils.BeginColoredVerticalBox(EditorUtils.VoxelBlue);

            EditorGUILayout.LabelField("Voxel Types:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This is where you define the Voxel Types. " +
                                    "Creating different Voxel Types allows you to define different types of ground for your terrain. " +
                                    "A Voxel Type defines which Terrain Material to use, how vertex colors should be set (to be used by the shader), " +
                                    "if grass should be generated on it, which details objects should be added on it and which kind of trees " +
                                    "should be spawned hover it.",
                                    MessageType.Info, true);
            EditorGUILayout.Space();

            var voxelTypes = voxelTypeSet.SerializableVoxelTypes;
            voxelTypesInspector.DisplayArrayInspector(ref voxelTypes, voxelTypeSet.VoxelTypesFoldoutForEditor);
            voxelTypeSet.SerializableVoxelTypes = voxelTypes;

            EditorUtils.EndColoredVerticalBox();
        }

        private string GenerateVoxelTypeName()
        {
            // Update dictionary
            voxelTypeSet.Init(terrain.ParamsForEditor);

            var baseName = "New Voxel Type ";
            var name = baseName + 1;
            for (var i = 2; i < 10000 && voxelTypeSet.GetVoxelType(name) != null; ++i) {
                name = baseName + i;
            }

            return name;
        }

        private bool IsVoxelTypeNameUnique(string name)
        {
            var count = 0;
            foreach (var b in voxelTypeSet.SerializableVoxelTypes) {
                if (b.Name == name) {
                    count++;
                    if (count > 1) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}