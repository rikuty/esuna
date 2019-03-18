namespace UltimateTerrainsEditor
{
    public class UltimateAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            return paths;
        }
    }
}