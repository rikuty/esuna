using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    [Serializable]
    public class NodeSerializable
    {
        [SerializeField] public string GUID;
        [SerializeField] private NodeContentSerializable content;

        [SerializeField] public Rect Rect;
        [SerializeField] private bool isDragged;
        [SerializeField] private bool isSelected;
        [SerializeField] private bool isRemovable;

        private List<ConnectionPointIn> inPoints;
        private ConnectionPointOut outPoint;

        private UltimateTerrain terrain;
        private Action<NodeSerializable> onRemoveNode;

        public NodeSerializable()
        {
        }

        public ConnectionPointOut OutPoint {
            get { return outPoint; }
        }

        public List<ConnectionPointIn> InPoints {
            get { return inPoints; }
        }

        public NodeContentSerializable Content {
            get { return content; }
        }

        public bool IsFinal {
            get { return content.IsFinal; }
        }

        public NodeSerializable(NodeContentSerializable content, Vector2 position, float width, bool removable = true)
        {
            this.isRemovable = removable;
            Rect = new Rect(position.x, position.y, width, 10 + 15 * content.InputCount);
            this.content = content;
            Init();
        }


        public void Init()
        {
            inPoints = new List<ConnectionPointIn>();
            for (var i = 0; i < Content.InputCount; ++i) {
                InPoints.Add(new ConnectionPointIn(i, this));
            }

            if (!IsFinal) {
                outPoint = new ConnectionPointOut(this);
            }
        }

#if UNITY_EDITOR
        public void Drag(Vector2 delta)
        {
            Rect.position += delta;
        }

        public void Draw(int id, UltimateTerrain uTerrain,
                         IReadOnlyFlowGraph flowGraph,
                         Action<ConnectionPointIn> onClickInPoint,
                         Action<ConnectionPointOut> onClickOutPoint,
                         GUIStyle inPointStyle,
                         GUIStyle outPointStyle)
        {
            this.terrain = uTerrain;
            var bkgCol = GUI.backgroundColor;
            var nodeInOutColor = GetNodeBackground(flowGraph);
            var nodeColor = nodeInOutColor;
            nodeInOutColor.a = 0.9f;
            GUI.backgroundColor = nodeInOutColor;
            for (var i = 0; i < InPoints.Count; i++) {
                var inPoint = InPoints[i];
                var inName = Content.InputNames.Length > i && Content.InputNames[i] != null ? Content.InputNames[i] : "Input";
                inPoint.Draw(onClickInPoint, inPointStyle, inName);
            }

            if (!IsFinal) {
                OutPoint.Draw(onClickOutPoint, outPointStyle);
            }

            GUI.backgroundColor = nodeColor;
            Rect = GUILayout.Window(id, Rect, DrawNodeContent, Content.Title, GUILayout.MinHeight(30), GUILayout.Width(Rect.width));
            GUI.backgroundColor = bkgCol;
        }

        private Color GetNodeBackground(IReadOnlyFlowGraph flowGraph)
        {
            if (!flowGraph.IsLinkedToFinalNode(this)) {
                return new Color(0.67f, 0.67f, 0.67f, 0.39f);
            }

            if (flowGraph.Is2D(this)) {
                return new Color(0f, 0.75f, 1f, 0.47f);
            }

            return new Color(0f, 1f, 0.36f, 0.47f);
        }

        private void DrawNodeContent(int id)
        {
            Undo.RecordObject(Content, "Biome configuration change");
            for (var i = 0; i < InPoints.Count; ++i) {
                GUILayoutUtility.GetRect(6f, 20f);
            }
            Content.OnEditorGUI(terrain);
        }

        public bool ProcessEvents(Event e, float zoom, Action<NodeSerializable> doOnClickRemoveNode)
        {
            onRemoveNode = doOnClickRemoveNode;

            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 0) {
                        if (Rect.Contains(e.mousePosition / zoom)) {
                            isDragged = true;
                            GUI.changed = true;
                            isSelected = true;
                        } else {
                            GUI.changed = true;
                            isSelected = false;
                        }
                    }

                    if (e.button == 1 && isSelected && Rect.Contains(e.mousePosition / zoom)) {
                        ProcessContextMenu();
                        e.Use();
                    }

                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged) {
                        Drag(e.delta / zoom);
                        e.Use();
                        return true;
                    }

                    break;
            }

            return false;
        }


        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            if (isRemovable) {
                genericMenu.AddItem(new GUIContent("Remove node"), false, SafeOnRemoveNode);
            }

            genericMenu.ShowAsContext();
        }

        private void SafeOnRemoveNode()
        {
            if (isRemovable && onRemoveNode != null) {
                onRemoveNode(this);
            }
        }

#endif
    }
}