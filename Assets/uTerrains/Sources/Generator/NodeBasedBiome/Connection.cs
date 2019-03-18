using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    [Serializable]
    public class Connection
    {
        [SerializeField] private string inNodeGUID;
        [SerializeField] private string outNodeGUID;
        [SerializeField] private int inPointIndex;
        private ConnectionPointIn inPoint;
        private ConnectionPointOut outPoint;

        public Connection()
        {
        }

        public Connection(ConnectionPointIn inPoint, ConnectionPointOut outPoint)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;
            inNodeGUID = inPoint.Node.GUID;
            outNodeGUID = outPoint.Node.GUID;
            inPointIndex = inPoint.GetIndexInNode();
        }

        public void PostConstruct(IReadOnlyFlowGraph graph)
        {
            inPoint = graph.GetNode(inNodeGUID).InPoints[inPointIndex];
            outPoint = graph.GetNode(outNodeGUID).OutPoint;
        }

        public ConnectionPointIn InPoint {
            get { return inPoint; }
        }

        public ConnectionPointOut OutPoint {
            get { return outPoint; }
        }


#if UNITY_EDITOR
        public void Draw(Action<Connection> onClickRemoveConnection)
        {
            Handles.DrawBezier(
                InPoint.Rect.center,
                OutPoint.Rect.center,
                InPoint.Rect.center + Vector2.left * 50f,
                OutPoint.Rect.center - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            if (Handles.Button((InPoint.Rect.center + OutPoint.Rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap)) {
                if (onClickRemoveConnection != null) {
                    onClickRemoveConnection(this);
                }
            }
        }
#endif
    }
}