using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UltimateTerrains;
using Random = UnityEngine.Random;

namespace UltimateTerrainsEditor.NodeBased
{
    public class NodeBasedEditor : EditorWindow
    {
        [SerializeField] private UltimateTerrain terrain;
        [SerializeField] private int terrainInstanceID;
        [SerializeField] private Biome biome;
        private ReadOnlyCollection<NodeSerializable> nodes;
        private ReadOnlyCollection<Connection> connections;

        private GUIStyle inPointStyle;
        private GUIStyle outPointStyle;

        private ConnectionPointIn selectedInPoint;
        private ConnectionPointOut selectedOutPoint;


        private bool readOnly;
        private IReadOnlyFlowGraph graph3D;

        private const float ZoomMin = 0.2f;
        private const float ZoomMax = 1.0f;

        private Rect zoomArea;
        private float zoom = 1.0f;


        public void Init(UltimateTerrain uTerrain,
                         Biome biome)
        {
            this.terrain = uTerrain;
            this.terrainInstanceID = uTerrain.GetInstanceID();
            this.biome = biome;
            this.graph3D = biome.Graph3D;
            nodes = this.biome.Nodes;
            connections = this.biome.Connections;
        }

        private void OnEnable()
        {
            if (!terrain) {
                terrain = EditorUtility.InstanceIDToObject(terrainInstanceID) as UltimateTerrain;
            }
            
            selectedInPoint = null;
            selectedOutPoint = null;

            var icon = EditorGUIUtility.IconContent("btn left focus").image as Texture2D;
            var icon2x = EditorGUIUtility.IconContent("btn left focus@2x").image as Texture2D;
            inPointStyle = new GUIStyle
            {
                border = new RectOffset(1, 1, 4, 4),
                normal =
                {
                    background = icon,
                    scaledBackgrounds = new[] {icon, icon2x}
                },
                active =
                {
                    background = icon,
                    scaledBackgrounds = new[] {icon, icon2x}
                }
            };

            icon = EditorGUIUtility.IconContent("btn right focus").image as Texture2D;
            icon2x = EditorGUIUtility.IconContent("btn right focus@2x").image as Texture2D;
            outPointStyle = new GUIStyle
            {
                normal =
                {
                    background = icon,
                    scaledBackgrounds = new[] {icon, icon2x}
                },
                active =
                {
                    background = icon,
                    scaledBackgrounds = new[] {icon, icon2x}
                }
            };
        }

        private void OnGUI()
        {
            if (!terrain) {
                terrain = EditorUtility.InstanceIDToObject(terrainInstanceID) as UltimateTerrain;
            }

            if (!terrain || !biome) {
                UDebug.LogError("Reference to terrain and/or biome has been lost. Closing editor window.");
                Close();
                return;
            }

            if (graph3D == null) {
                graph3D = this.biome.Graph3D;
                nodes = this.biome.Nodes;
                connections = this.biome.Connections;
            }

            zoomArea = this.position;
            zoomArea.position = Vector2.zero;

            EditorUtils.EditorZoomArea.Begin(zoom, zoomArea);
            DrawConnections();
            DrawNodes();
            DrawConnectionLine(Event.current);
            EditorUtils.EditorZoomArea.End();

            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);

            if (GUI.changed) {
                Repaint();
            }
        }

        private void DrawNodes()
        {
            BeginWindows();
            if (nodes != null) {
                for (var i = 0; i < nodes.Count; i++) {
                    nodes[i].Draw(i, terrain, graph3D, OnClickInPoint, OnClickOutPoint, inPointStyle, outPointStyle);
                }

                if (GUI.changed) {
                    TryPerformHotReload();
                }
            }

            EndWindows();
        }

        private void DrawConnections()
        {
            if (connections != null) {
                for (int i = 0; i < connections.Count; i++) {
                    connections[i].Draw(OnClickRemoveConnection);
                }
            }
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 0) {
                        ClearConnectionSelection();
                    }

                    if (e.button == 1) {
                        ProcessContextMenu(e.mousePosition / zoom);
                    }

                    EditorUtility.SetDirty(biome);
                    AssetDatabase.SaveAssets();
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0) {
                        OnDrag(e.delta / zoom);
                        e.Use();
                    }

                    break;
                case EventType.ScrollWheel:
                    zoom += -e.delta.y / 150.0f;
                    zoom = Mathf.Clamp(zoom, ZoomMin, ZoomMax);

                    e.Use();
                    break;
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if (nodes != null) {
                for (int i = nodes.Count - 1; i >= 0; i--) {
                    bool guiChanged = nodes[i].ProcessEvents(e, zoom, OnClickRemoveNode);

                    if (guiChanged) {
                        GUI.changed = true;
                    }
                }
            }
        }

        private void DrawConnectionLine(Event e)
        {
            if (selectedInPoint != null && selectedOutPoint == null) {
                Handles.DrawBezier(
                    selectedInPoint.Rect.center,
                    e.mousePosition,
                    selectedInPoint.Rect.center + Vector2.left * 50f,
                    e.mousePosition - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }

            if (selectedOutPoint != null && selectedInPoint == null) {
                Handles.DrawBezier(
                    selectedOutPoint.Rect.center,
                    e.mousePosition,
                    selectedOutPoint.Rect.center - Vector2.left * 50f,
                    e.mousePosition + Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            var genericMenu = new GenericMenu();

            foreach (var availableType in EditorUtils.GetAvailableTypes(typeof(Primitive2DNodeSerializable))) {
                genericMenu.AddItem(new GUIContent("2D Noises/" + EditorUtils.GetPrettyName(availableType)), false, () => OnClickAddNode(mousePosition, availableType));
            }

            foreach (var availableType in EditorUtils.GetAvailableTypes(typeof(Primitive3DNodeSerializable))) {
                genericMenu.AddItem(new GUIContent("3D Noises/" + EditorUtils.GetPrettyName(availableType)), false, () => OnClickAddNode(mousePosition, availableType));
            }

            foreach (var availableType in EditorUtils.GetAvailableTypes(typeof(CombinerNodeSerializable))) {
                genericMenu.AddItem(new GUIContent("Combiners/" + EditorUtils.GetPrettyName(availableType)), false, () => OnClickAddNode(mousePosition, availableType));
            }

            foreach (var availableType in EditorUtils.GetAvailableTypes(typeof(FilterNodeSerializable))) {
                genericMenu.AddItem(new GUIContent("Filters/" + EditorUtils.GetPrettyName(availableType)), false, () => OnClickAddNode(mousePosition, availableType));
            }

            foreach (var availableType in EditorUtils.GetAvailableTypes(typeof(TransformerNodeSerializable))) {
                genericMenu.AddItem(new GUIContent("Transformers/" + EditorUtils.GetPrettyName(availableType)), false, () => OnClickAddNode(mousePosition, availableType));
            }

            if (biome is BiomeSelector) {
                foreach (var availableType in EditorUtils.GetAvailableTypes(typeof(FinalBiomeSelectionNodeSerializable))) {
                    genericMenu.AddItem(new GUIContent("Final/" + EditorUtils.GetPrettyName(availableType)), false, () => OnClickAddNode(mousePosition, availableType));
                }
            } else {
                foreach (var availableType in EditorUtils.GetAvailableTypes(typeof(Final3DVoxelTypeNodeSerializable))) {
                    genericMenu.AddItem(new GUIContent("Final/" + EditorUtils.GetPrettyName(availableType)), false, () => OnClickAddNode(mousePosition, availableType));
                }
            }

            genericMenu.AddItem(new GUIContent("Position/X"), false, () => OnClickAddNode(mousePosition, typeof(PositionXNodeSerializable)));
            genericMenu.AddItem(new GUIContent("Position/Y"), false, () => OnClickAddNode(mousePosition, typeof(PositionYNodeSerializable)));
            genericMenu.AddItem(new GUIContent("Position/Z"), false, () => OnClickAddNode(mousePosition, typeof(PositionZNodeSerializable)));

            genericMenu.AddItem(new GUIContent("Constant"), false, () => OnClickAddNode(mousePosition, typeof(ConstantNodeSerializable)));

            genericMenu.ShowAsContext();
        }

        private void OnDrag(Vector2 delta)
        {
            if (nodes != null) {
                for (var i = 0; i < nodes.Count; i++) {
                    nodes[i].Drag(delta);
                }
            }

            GUI.changed = true;
        }

        private NodeSerializable OnClickAddNode(Vector2 mousePosition, Type type)
        {
            var content = (NodeContentSerializable) CreateInstance(type);
            var nodeSerializable = new NodeSerializable(content, mousePosition, content.EditorWidth);
            biome.AddNode(nodeSerializable);
            return nodeSerializable;
        }

        private void OnClickInPoint(ConnectionPointIn inPoint)
        {
            var existingConnection = GetConnectionForInPoint(inPoint);
            if (existingConnection != null) {
                if (selectedOutPoint != null)
                    return;
                selectedInPoint = null;
                selectedOutPoint = existingConnection.OutPoint;
                OnClickRemoveConnection(existingConnection);
                return;
            }

            selectedInPoint = inPoint;

            if (selectedOutPoint != null) {
                if (selectedOutPoint.Node != selectedInPoint.Node) {
                    if (CreateConnection()) {
                        ClearConnectionSelection();
                    } else {
                        selectedInPoint = null;
                    }
                } else {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickOutPoint(ConnectionPointOut outPoint)
        {
            selectedOutPoint = outPoint;

            if (selectedInPoint != null) {
                if (selectedOutPoint.Node != selectedInPoint.Node) {
                    if (CreateConnection()) {
                        ClearConnectionSelection();
                    } else {
                        selectedOutPoint = null;
                    }
                } else {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickRemoveNode(NodeSerializable nodeSerializable)
        {
            if (connections != null) {
                List<Connection> connectionsToRemove = new List<Connection>();

                for (int i = 0; i < connections.Count; i++) {
                    if (nodeSerializable.InPoints.Contains(connections[i].InPoint) || connections[i].OutPoint == nodeSerializable.OutPoint) {
                        connectionsToRemove.Add(connections[i]);
                    }
                }

                for (int i = 0; i < connectionsToRemove.Count; i++) {
                    biome.RemoveConnection(connectionsToRemove[i]);
                }
            }

            biome.RemoveNode(nodeSerializable);
        }

        private void OnClickRemoveConnection(Connection connection)
        {
            biome.RemoveConnection(connection);
        }

        private bool CreateConnection()
        {
            if (!graph3D.WouldBeCyclic(selectedOutPoint.Node, selectedInPoint.Node)) {
                if (selectedInPoint.Node.Content is Final3DNodeSerializable && graph3D.Is2D(selectedOutPoint.Node)) {
                    var toHeightNode = OnClickAddNode(selectedInPoint.Node.Rect.position + new Vector2(-Random.Range(50, 100), Random.Range(10, 50)), typeof(ToHeightTransformerSerializable));
                    biome.AddConnection(new Connection(selectedInPoint, toHeightNode.OutPoint));
                    biome.AddConnection(new Connection(toHeightNode.InPoints[0], selectedOutPoint));
                } else {
                    biome.AddConnection(new Connection(selectedInPoint, selectedOutPoint));
                }

                return true;
            }

            return false;
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }

        private Connection GetConnectionForInPoint(ConnectionPointIn inPoint)
        {
            foreach (var connection in connections) {
                if (connection.InPoint == inPoint)
                    return connection;
            }

            return null;
        }

        private void TryPerformHotReload()
        {
            if (biome.Validate(terrain, false)) {
                terrain.HotReload();
            } else {
                UDebug.Log("Cannot perform hot-reloading because biome configuration is not valid.");
            }
        }
    }
}