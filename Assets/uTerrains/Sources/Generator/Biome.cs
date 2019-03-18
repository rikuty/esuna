using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public class Biome : ScriptableObject
    {
        private static readonly string BiomesBasePath = Path.Combine(Path.Combine("Assets", "uTerrains"), "Biomes");

        public string Name; // For editor purpose only

        [SerializeField] protected FlowGraph3D graph3D;

        public IReadOnlyFlowGraph Graph3D {
            get { return graph3D; }
        }

        public ReadOnlyCollection<Connection> Connections {
            get { return graph3D != null ? graph3D.Connections : null; }
        }

        public ReadOnlyCollection<NodeSerializable> Nodes {
            get { return graph3D != null ? graph3D.AllNodes : null; }
        }

        public ReadOnlyCollection<NodeSerializable> FinalNodes {
            get { return graph3D != null ? graph3D.FinalNodes : null; }
        }

        public virtual void Init(bool clear = false)
        {
            if (clear || graph3D == null) {
                graph3D = new FlowGraph3D(false);
            }

            graph3D.Init();
            SaveAsAsset();
        }

        public void AddNode(NodeSerializable node)
        {
            graph3D.AddNode(node);
            AddNodeToAsset(node);
        }

        public void RemoveNode(NodeSerializable node)
        {
            RemoveNode(node.GUID);
        }

        public void RemoveNode(string guid)
        {
            graph3D.RemoveNode(guid);
        }

        public void AddConnection(Connection connection)
        {
            graph3D.AddConnection(connection);
        }

        public void RemoveConnection(Connection connection)
        {
            graph3D.RemoveConnection(connection);
        }

        public bool Validate(UltimateTerrain terrain, bool logErrorsFatal)
        {
            try {
                new GenerationFlow3D(terrain, graph3D);
            }
            catch (InvalidFlowException e) {
                if (logErrorsFatal)
                    UDebug.Fatal(string.Format("Error in '{0}': {1}", Name, e.Message));
                return false;
            }

            return true;
        }

        public void Clear()
        {
            graph3D = null;
        }

        protected virtual void SaveAsAsset()
        {
#if UNITY_EDITOR
            if (Graph3D == null || Graph3D.AllNodes == null || !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
                return;

            if (!Directory.Exists(BiomesBasePath))
                Directory.CreateDirectory(BiomesBasePath);

            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(BiomesBasePath, "biome.asset"));
            AssetDatabase.CreateAsset(this, path);
            foreach (var nodeSerializable in Graph3D.AllNodes) {
                if (nodeSerializable != null)
                    AssetDatabase.AddObjectToAsset(nodeSerializable.Content, this);
            }
#endif
        }

        private void AddNodeToAsset(NodeSerializable nodeSerializable)
        {
#if UNITY_EDITOR
            if (nodeSerializable != null)
                AssetDatabase.AddObjectToAsset(nodeSerializable.Content, this);
#endif
        }
    }
}