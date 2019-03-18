using System;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public class ConnectionPointOut
    {
        public Rect Rect;
        private readonly NodeSerializable node;

        public ConnectionPointOut(NodeSerializable node)
        {
            this.node = node;
            Rect = new Rect(0, 0, 10f, 12f);
        }

        public NodeSerializable Node {
            get { return node; }
        }

#if UNITY_EDITOR
        public void Draw(Action<ConnectionPointOut> onClickConnectionPoint, GUIStyle style)
        {
            Rect.y = Node.Rect.y + 20;
            Rect.x = node.Rect.x + node.Rect.width;
            if (GUI.Button(Rect, "", style)) {
                if (onClickConnectionPoint != null) {
                    onClickConnectionPoint(this);
                }
            }
        }
#endif
    }
}