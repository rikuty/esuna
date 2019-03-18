using UltimateTerrains;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    public class CreateTerrainWindow : EditorWindow
    {
        private const int WIDTH = 500;
        private const int HEIGHT = 240;

        private Vector2 scrollPosition;

        private readonly string objName = "uTerrain";
        private Transform player;
        private Material mainMaterial;


        // Add menu item
        [MenuItem("Tools/uTerrains/New terrain...")]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (CreateTerrainWindow) GetWindow(typeof(CreateTerrainWindow), true, "New ultimate terrain");
            window.position = new Rect(100, 100, WIDTH, HEIGHT);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Base settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Don't worry, you'll be able to change any of these settings later.");
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This assistant let's you define some base settings of your terrain, " +
                                    "but you will have to configure at least one biome by yourself. " +
                                    "For a first try, it may be easier to load a scene from uTerrains/Scenes folder.", MessageType.Info, true);
            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select the player Object from your scene (can be the main camera).");
            player = (Transform) EditorGUILayout.ObjectField("Player Object:", player, typeof(Transform), true);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select the main material of the terrain.");
            mainMaterial = (Material) EditorGUILayout.ObjectField("Main Material:", mainMaterial, typeof(Material), false);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel")) {
                Close();
            }

            if (GUILayout.Button("Ok")) {
                if (Validate()) {
                    CreateTerrain();
                    Close();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool Validate()
        {
            if (player == null) {
                if (!EditorUtility.DisplayDialog("No player object selected",
                                                 "No player-object has been selected. If you continue, the terrain will be created but " +
                                                 "you will have to assign the 'player' field by yourself in the terrain's inspector.", "Ok", "Cancel")) {
                    return false;
                }
            }

            return true;
        }

        private void CreateTerrain()
        {
            var go = new GameObject(GenerateTerrainName());
            var terrain = go.AddComponent<UltimateTerrain>();
            terrain.PlayerTransform = player;

            var voxelTypeSet = terrain.VoxelTypeSet;
            var voxelTypes = new VoxelType[1];
            voxelTypes[0] = new VoxelType();
            voxelTypes[0].Name = "My Voxel Type";
            voxelTypeSet.SerializableVoxelTypes = voxelTypes;
            voxelTypeSet.Materials = new[] {mainMaterial};
            voxelTypeSet.Init(terrain.ParamsForEditor);

            if (!go.GetComponent<GeneratorModulesComponent>())
                go.AddComponent<GeneratorModulesComponent>();

            if (!go.GetComponent<Orchestrator>())
                go.AddComponent<Orchestrator>();

            if (!go.GetComponent<AsyncOperationOrchestrator>())
                go.AddComponent<AsyncOperationOrchestrator>();
        }

        private string GenerateTerrainName()
        {
            var realName = objName;
            for (var i = 2; i < 100 && GameObject.Find(realName) != null; ++i) {
                realName = objName + " " + i;
            }

            return realName;
        }
    }
}