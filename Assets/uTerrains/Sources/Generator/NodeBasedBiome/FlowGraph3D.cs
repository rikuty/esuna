using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public sealed class FlowGraph3D : IReadOnlyFlowGraph
    {
        [SerializeField] private bool isBiomeSelector;
        [SerializeField] private GUIDNodeDictionary nodes;
        [SerializeField] private List<Connection> connections;

        public ReadOnlyCollection<Connection> Connections {
            get { return connections != null ? connections.AsReadOnly() : null; }
        }

        public ReadOnlyCollection<NodeSerializable> AllNodes {
            get { return nodes != null ? nodes.Values : null; }
        }

        public ReadOnlyCollection<NodeSerializable> FinalNodes {
            get { return nodes != null && nodes.FinalNodes != null ? nodes.FinalNodes.AsReadOnly() : null; }
        }

        public FlowGraph3D(bool isBiomeSelector)
        {
            this.isBiomeSelector = isBiomeSelector;
        }

        public void Init()
        {
            if (nodes == null) {
                nodes = new GUIDNodeDictionary();
            }

            if (connections == null) {
                connections = new List<Connection>();
            }

            if (nodes.FinalNodes.Count == 0) {
                if (isBiomeSelector) {
                    var content = (NodeContentSerializable) ScriptableObject.CreateInstance(typeof(FinalBiomeSelectionNodeSerializable));
                    var finalNodeSerializable = new NodeSerializable(content, new Vector2(200, 0), content.EditorWidth, false);
                    AddNode(finalNodeSerializable);

                    content = (NodeContentSerializable) ScriptableObject.CreateInstance(typeof(ConstantNodeSerializable));
                    var constantNodeSerializable = new NodeSerializable(content, new Vector2(0, 0), content.EditorWidth, true);
                    AddNode(constantNodeSerializable);

                    var connection = new Connection(finalNodeSerializable.InPoints[0], constantNodeSerializable.OutPoint);
                    AddConnection(connection);
                } else {
                    var content = (NodeContentSerializable) ScriptableObject.CreateInstance(typeof(Final3DNodeSerializable));
                    AddNode(new NodeSerializable(content, new Vector2(200, 0), content.EditorWidth, false));

                    content = (NodeContentSerializable) ScriptableObject.CreateInstance(typeof(Final3DVoxelTypeNodeSerializable));
                    var nodeSerializable = new NodeSerializable(content, new Vector2(200, 100), content.EditorWidth, false);
                    AddNode(nodeSerializable);

                    var cstContent = (ConstantNodeSerializable) ScriptableObject.CreateInstance(typeof(ConstantNodeSerializable));
                    cstContent.ConstantName = "VoxelType weight";
                    var constantNodeSerializable = new NodeSerializable(cstContent, new Vector2(0, 100), cstContent.EditorWidth, true);
                    AddNode(constantNodeSerializable);

                    var connection = new Connection(nodeSerializable.InPoints[0], constantNodeSerializable.OutPoint);
                    AddConnection(connection);
                }
            }

            foreach (var node in nodes.Values) {
                node.Init();
            }

            foreach (var connection in connections) {
                connection.PostConstruct(this);
            }
        }

        public NodeSerializable GetNode(string guid)
        {
            return nodes[guid];
        }

        public void AddNode(NodeSerializable node)
        {
            nodes.Add(node);
        }

        public void RemoveNode(NodeSerializable node)
        {
            nodes.Remove(node);
        }

        public void RemoveNode(string guid)
        {
            nodes.Remove(guid);
        }

        public void AddConnection(Connection connection)
        {
            connections.Add(connection);
        }

        public void RemoveConnection(Connection connection)
        {
            connections.Remove(connection);
        }

        public ReadOnlyCollection<Connection> GetAllInputConnectionsOfNode(NodeSerializable node)
        {
            var connectionsOfNode = new List<Connection>();
            foreach (var connection in connections) {
                if (connection.InPoint.Node == node) {
                    connectionsOfNode.Add(connection);
                }
            }

            connectionsOfNode.Sort((c1, c2) => c1.InPoint.GetIndexInNode().CompareTo(c2.InPoint.GetIndexInNode()));

            return connectionsOfNode.AsReadOnly();
        }

        public ReadOnlyCollection<Connection> GetAllOutputConnectionsOfNode(NodeSerializable node)
        {
            var connectionsOfNode = new List<Connection>();
            foreach (var connection in connections) {
                if (connection.OutPoint.Node == node) {
                    connectionsOfNode.Add(connection);
                }
            }

            return connectionsOfNode.AsReadOnly();
        }

        public bool IsLinkedToFinalNode(NodeSerializable node)
        {
            if (node.IsFinal)
                return true;

            var outConnections = GetAllOutputConnectionsOfNode(node);
            foreach (var outConnection in outConnections) {
                if (IsLinkedToFinalNode(outConnection.InPoint.Node))
                    return true;
            }

            return false;
        }

        public bool Is2D(NodeSerializable node)
        {
            switch (node.Content.Layer) {
                case NodeLayer.Layer2D:
                    return true;
                case NodeLayer.Layer3D:
                    return false;
                case NodeLayer.LayerDetermindeByInputs:
                    var inputs = GetAllInputsOfNode(node);
                    foreach (var input in inputs) {
                        if (!Is2D(input))
                            return false;
                    }

                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<NodeSerializable> GetAllInputsOfNode(NodeSerializable node)
        {
            var inputsOfNode = new List<NodeSerializable>();
            foreach (var connection in connections) {
                if (connection.InPoint.Node == node) {
                    inputsOfNode.Add(connection.OutPoint.Node);
                }
            }

            return inputsOfNode;
        }

        public bool IsCyclic()
        {
            foreach (var node in nodes.Values) {
                if (IsCyclic(node, node)) {
                    return true;
                }
            }

            return false;
        }

        public bool WouldBeCyclic(NodeSerializable startNode, NodeSerializable endNode)
        {
            return IsCyclic(startNode, endNode);
        }

        private bool IsCyclic(NodeSerializable startNode, NodeSerializable currentNode)
        {
            if (currentNode.IsFinal)
                return false;

            var outConnections = GetAllOutputConnectionsOfNode(currentNode);
            foreach (var outConnection in outConnections) {
                if (outConnection.InPoint.Node == startNode)
                    return true;
                if (IsCyclic(startNode, outConnection.InPoint.Node))
                    return true;
            }

            return false;
        }
    }
}