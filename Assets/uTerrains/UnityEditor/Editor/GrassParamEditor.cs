using UltimateTerrains;
using UnityEditor;

namespace UltimateTerrainsEditor
{
    public sealed class GrassParamEditor
    {
        private readonly ListInspector<GrassParam.GrassMaterial> materialsInspector;
        private float lastProba;

        public GrassParamEditor()
        {
            materialsInspector = new ListInspector<GrassParam.GrassMaterial>("Add grass material", null, "Do you really want to delete this grass material?", false, (item, i) => {
                                                                                 if (i == 0)
                                                                                     lastProba = 0;
                                                                                 item.GrassMaterialIndex = EditorGUILayout.IntField("Grass material index " + i + ":", item.GrassMaterialIndex);
                                                                                 item.MaterialProbability = EditorGUILayout.Slider("Probability " + i + ":", item.MaterialProbability, lastProba, 1f);
                                                                                 lastProba = item.MaterialProbability;
                                                                                 return item;
                                                                             },
                                                                             // Create
                                                                             () => { return new GrassParam.GrassMaterial(); },
                                                                             // Remove
                                                                             i => { return i > 0; });
        }

        public void OnInspectorGUI(GrassParam param)
        {
            param.GrassDensity = EditorGUILayout.Slider("Density:", param.GrassDensity, 0.01f, 10f);
            param.BaseColor = EditorGUILayout.ColorField("Grass color:", param.BaseColor);
            param.DirtyColor = EditorGUILayout.ColorField("Dirty grass color:", param.DirtyColor);
            param.BaseHeight = EditorGUILayout.FloatField("Grass height:", param.BaseHeight);
            param.DirtyHeight = EditorGUILayout.FloatField("Dirty grass height:", param.DirtyHeight);
            param.GrassSize = EditorGUILayout.FloatField("Grass horizontal size:", param.GrassSize);
            param.MinHeight = EditorGUILayout.FloatField("Min height of grass:", param.MinHeight);
            param.TileX = EditorGUILayout.FloatField("Texture tile X:", param.TileX);
            param.MaterialNoiseFrequency = EditorGUILayout.FloatField("Grass material noise frequency:", param.MaterialNoiseFrequency);
            param.MinNormalY = EditorGUILayout.FloatField("Max slope of ground (0=vertical, 1=horizontal):", param.MinNormalY);
            param.NoiseFrequency = EditorGUILayout.FloatField("Noise frequency:", param.NoiseFrequency);
            param.Dissemination = EditorGUILayout.Slider("Dissemination:", param.Dissemination, 0f, 0.5f);

            var materials = param.Materials;
            materialsInspector.DisplayArrayInspector(ref materials);
            param.Materials = materials;
        }
    }
}