using System;
using UnityEngine;

namespace UltimateTerrains
{
    public class ConnectionPointIn
    {
        public Rect Rect;
        private readonly int index;
        private readonly NodeSerializable node;

        public NodeSerializable Node {
            get { return node; }
        }

        public ConnectionPointIn(int index, NodeSerializable node)
        {
            this.index = index;
            this.node = node;
            Rect = new Rect(0, 0, 10f, 12f);
        }

        public int GetIndexInNode()
        {
            return index;
        }

#if UNITY_EDITOR
        public void Draw(Action<ConnectionPointIn> onClickConnectionPoint, GUIStyle style, string name)
        {
            Rect.y = Node.Rect.y + 20 + 20 * index;
            Rect.x = Node.Rect.x - Rect.width + 1f;
            var labelRect = Rect;
            labelRect.position += new Vector2(Rect.width, -1f);
            labelRect.width = 100f;
            labelRect.height = 20f;
            GUI.Label(labelRect, name);
            if (GUI.Button(Rect, "", style)) {
                if (onClickConnectionPoint != null) {
                    onClickConnectionPoint(this);
                }
            }
        }
#endif
    }
}