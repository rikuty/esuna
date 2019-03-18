using UltimateTerrains;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    public sealed class DetailsParamEditor
    {
        private readonly ListInspector<DetailsParam.DetailObject> objectsInspector;
        private float lastProba;

        public DetailsParamEditor()
        {
            objectsInspector = new ListInspector<DetailsParam.DetailObject>("Add detail", null, "Do you really want to remove this object?", false, (item, i) => {
                                                                                item.Mesh = (Mesh) EditorGUILayout.ObjectField("Mesh " + i + ":", item.Mesh, typeof(Mesh), false);
                                                                                item.Material = (Material) EditorGUILayout.ObjectField("Material " + i + ":", item.Material, typeof(Material), false);
                                                                                if (i == 0)
                                                                                    lastProba = 0;
                                                                                item.ObjectProbability = EditorGUILayout.Slider("Probability " + i + ":", item.ObjectProbability, lastProba, 1f);
                                                                                lastProba = item.ObjectProbability;
                                                                                return item;
                                                                            },
                                                                            // Create
                                                                            () => { return new DetailsParam.DetailObject(); },
                                                                            // Remove
                                                                            i => { return i > 0; });
        }

        public void OnEnable(UltimateTerrain terrain)
        {
        }

        public void OnDisable()
        {
        }

        public void OnInspectorGUI(DetailsParam param)
        {
            param.Name = EditorGUILayout.TextField("Group Name:", param.Name);
            var maxLevelDetailsLongLabel = "This detail objects will be enabled on all chunks with a level lesser or equal to this.";
            param.MaxLevelIndex = EditorGUILayout.IntSlider(new GUIContent("Max LOD:", maxLevelDetailsLongLabel), param.MaxLevelIndex, 1, Param.MAX_LEVEL_COUNT);

            param.DensityDistance = EditorGUILayout.FloatField(new GUIContent("Density-distance:", "Average distance between objects."), param.DensityDistance);
            if (param.DensityDistance < 0.1f)
                param.DensityDistance = 0.1f;

            param.ObjectsNoiseFrequency = EditorGUILayout.FloatField(new GUIContent("Chooser frequency:", "Noise frequency used to choose which kind of object to display at some position."),
                                                                     param.ObjectsNoiseFrequency);
            EditorGUILayout.MinMaxSlider(new GUIContent("Min/max slope:", "Min/max slope of ground (-1=upside-down, 0=vertical, 1=horizontal)."), ref param.MinNormalY, ref param.MaxNormalY, -1.01f, 1.01f);
            param.VerticalOffset = EditorGUILayout.FloatField(new GUIContent("Vertical offset:", "Useful to place objects deeper in the ground if you set a negative value."), param.VerticalOffset);

            param.MinScale = EditorGUILayout.FloatField(new GUIContent("Min scale:", "Minimum scale of the detail."), param.MinScale);
            param.MaxScale = EditorGUILayout.FloatField(new GUIContent("Max scale:", "Maximum scale of the detail."), param.MaxScale);
            param.ScaleNoiseFrequency = EditorGUILayout.FloatField(new GUIContent("Scale noise frequency:", "Frequency at which the scale of details will change."), param.ScaleNoiseFrequency);

            param.Rotate = EditorGUILayout.Toggle(new GUIContent("Rotate:", "Should the detail be randomly rotated on Y axis?"), param.Rotate);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Objects list:", EditorStyles.boldLabel);
            var objects = param.Objects;
            objectsInspector.DisplayArrayInspector(ref objects);
            param.Objects = objects;
        }
    }
}