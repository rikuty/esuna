using System.Collections.ObjectModel;

namespace UltimateTerrains
{
    public interface IReadOnlyFlowGraph
    {
        /// <summary>
        /// Must be idempotent
        /// </summary>
        void Init();
        
        NodeSerializable GetNode(string guid);

        ReadOnlyCollection<NodeSerializable> AllNodes { get; }

        ReadOnlyCollection<NodeSerializable> FinalNodes { get; }

        bool Is2D(NodeSerializable nodeSerializable);

        bool IsLinkedToFinalNode(NodeSerializable node);

        bool IsCyclic();

        bool WouldBeCyclic(NodeSerializable startNode, NodeSerializable endNode);

        ReadOnlyCollection<Connection> GetAllOutputConnectionsOfNode(NodeSerializable node);

        ReadOnlyCollection<Connection> GetAllInputConnectionsOfNode(NodeSerializable node);
    }
}