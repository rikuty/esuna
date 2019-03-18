using System.IO;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public sealed class BiomeSelector : Biome
    {
        private static readonly string BiomeSelectorsBasePath = Path.Combine(Path.Combine("Assets", "uTerrains"), "BiomeSelectors");

        public override void Init(bool clear = false)
        {
            Name = "Biome Selector";
            
            if (clear || graph3D == null) {
                graph3D = new FlowGraph3D(true);
            }

            graph3D.Init();
            SaveAsAsset();
        }

        protected override void SaveAsAsset()
        {
#if UNITY_EDITOR
            if (Graph3D == null || Graph3D.AllNodes == null || !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
                return;

            if (!Directory.Exists(BiomeSelectorsBasePath))
                Directory.CreateDirectory(BiomeSelectorsBasePath);

            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(BiomeSelectorsBasePath, "biomeSelector.asset"));
            AssetDatabase.CreateAsset(this, path);
            foreach (var nodeSerializable in Graph3D.AllNodes) {
                if (nodeSerializable != null)
                    AssetDatabase.AddObjectToAsset(nodeSerializable.Content, this);
            }
#endif
        }
    }
}