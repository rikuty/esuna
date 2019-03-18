using UltimateTerrains;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    public sealed class TreesParamEditor
    {
        private readonly ListInspector<TreesParam.TreeObject> objectsInspector;
        private float lastProba;

        public TreesParamEditor()
        {
            objectsInspector = new ListInspector<TreesParam.TreeObject>("Add tree", null, "Do you really want to remove this tree?", false, (item, i) => {
                                                                            item.Object = (GameObject) EditorGUILayout.ObjectField("Tree " + i + ":", item.Object, typeof(Object), false);
                                                                            if (i == 0)
                                                                                lastProba = 0;
                                                                            item.ObjectProbability = EditorGUILayout.Slider("Probability " + i + ":", item.ObjectProbability, lastProba, 1f);
                                                                            lastProba = item.ObjectProbability;
                                                                            return item;
                                                                        },
                                                                        // Create
                                                                        () => { return new TreesParam.TreeObject(); },
                                                                        // Remove
                                                                        i => { return i > 0; });
        }

        public void OnEnable(UltimateTerrain terrain)
        {
        }

        public void OnDisable()
        {
        }

        public void OnInspectorGUI(TreesParam param)
        {
            param.Name = EditorGUILayout.TextField("Group Name:", param.Name);
            param.ObjectsNoiseFrequency = EditorGUILayout.FloatField(new GUIContent("Chooser frequency:", "Noise frequency used to choose which kind of tree to display at some position."),
                                                                     param.ObjectsNoiseFrequency);
            param.MinNormalY = EditorGUILayout.FloatField(new GUIContent("Max slope:", "Max slope of ground (0=vertical, 1=horizontal)."), param.MinNormalY);
            param.VerticalOffset = EditorGUILayout.FloatField(new GUIContent("Vertical offset:", "Useful to place trees deeper in the ground if you set a negative value."), param.VerticalOffset);

            param.MinScale = EditorGUILayout.FloatField(new GUIContent("Min scale:", "Minimum scale of the tree."), param.MinScale);
            param.MaxScale = EditorGUILayout.FloatField(new GUIContent("Max scale:", "Maximum scale of the tree."), param.MaxScale);
            param.ScaleNoiseFrequency = EditorGUILayout.FloatField(new GUIContent("Scale noise frequency:", "Frequency at which the scale of trees will change."), param.ScaleNoiseFrequency);

            param.Rotate = EditorGUILayout.Toggle(new GUIContent("Rotate:", "Should the tree be randomly rotated on Y axis?"), param.Rotate);

            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Prefabs list:", EditorStyles.boldLabel);
            var objects = param.Objects;
            objectsInspector.DisplayArrayInspector(ref objects);
            param.Objects = objects;
        }
    }
}