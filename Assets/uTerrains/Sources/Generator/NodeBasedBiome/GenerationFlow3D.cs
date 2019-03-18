using System.Collections.Generic;

namespace UltimateTerrains
{
    public class GenerationFlow3D
    {
        private readonly UltimateTerrain terrain;
        private readonly IReadOnlyFlowGraph graph3D;
        private readonly FinalCallableNode finalNode;
        private readonly FinalVoxelTypeCallableNode[] finalVoxelTypeNodes;
        private readonly FinalBiomeSelectionCallableNode[] finalBiomeSelectionNodes;
        private readonly CachableCallableNode[] cachable2DNodes;

        private bool evenCall;

        public GenerationFlow3D(UltimateTerrain terrain, IReadOnlyFlowGraph graph3D)
        {
            this.terrain = terrain;
            this.graph3D = graph3D;
            this.evenCall = true;
            graph3D.Init();

            if (graph3D.IsCyclic()) {
                throw new InvalidFlowException("Flow is cyclic");
            }

            var callableNodes = new Dictionary<string, CallableNode>();
            var cachable2DNodeList = new List<CachableCallableNode>();

            finalNode = null;
            var finalVoxelTypeNodeList = new List<FinalVoxelTypeCallableNode>();
            var finalBiomeSelectionNodeList = new List<FinalBiomeSelectionCallableNode>();

            foreach (var node in graph3D.FinalNodes) {
                if (node.Content is Final3DVoxelTypeNodeSerializable) {
                    var voxelType = terrain.VoxelTypeSet.GetVoxelType(((Final3DVoxelTypeNodeSerializable) node.Content).VoxelTypeIndex);
                    finalVoxelTypeNodeList.Add(CreateFinalVoxelTypeCallableNode(node, voxelType, cachable2DNodeList, callableNodes));
                }
                // 3D flow has one and only one final node
                else if (node.Content is FinalBiomeSelectionNodeSerializable) {
                    finalBiomeSelectionNodeList.Add(CreateFinalBiomeSelectionCallableNode(node, cachable2DNodeList, callableNodes));
                }
                // 3D flow has one and only one final node
                else if (node.Content is Final3DNodeSerializable && finalNode == null) {
                    finalNode = CreateFinalCallableNode(node, cachable2DNodeList, callableNodes);
                } else {
                    throw new InvalidFlowException(string.Format("Final Node '{0}' with GUID '{1}' has wrong type or there are too many of this type.", node.Content.Title, node.GUID));
                }
            }

            finalVoxelTypeNodes = finalVoxelTypeNodeList.ToArray();
            finalBiomeSelectionNodes = finalBiomeSelectionNodeList.ToArray();
            // This is mandatory to preserve the order so inputs are computed before.
            // Indeed, some cachable-nodes' inputs might be other cachable-nodes! So we must be sure they will be computed first!
            cachable2DNodes = cachable2DNodeList.ToArray();
        }

        public void Set2DValuesFromCacheAndComputeWhenNeeded(double x, double y, double z, double[] cached2DValues)
        {
            for (var i = 0; i < cachable2DNodes.Length; i++) {
                if (i < cached2DValues.Length) {
                    cachable2DNodes[i].SetPreComputedValueFromCache(cached2DValues[i]);
                } else {
                    cachable2DNodes[i].PreCompute(x, y, z);
                }
            }
        }

        public void Compute2DValues(double x, double y, double z, double[] out_2DValues)
        {
            for (var i = 0; i < cachable2DNodes.Length; i++) {
                var value = cachable2DNodes[i].PreCompute(x, y, z);
                if (i < out_2DValues.Length) {
                    out_2DValues[i] = value;
                }
            }
        }

        public void Compute2DValuesNoCache(double x, double y, double z)
        {
            foreach (var node in cachable2DNodes) {
                node.PreCompute(x, y, z);
            }
        }

        public void Execute(double x, double y, double z, out double voxelValue, out VoxelType voxelType)
        {
            voxelValue = finalNode.Execute(x, y, z, evenCall);

            voxelType = null;
            var voxelTypeIntensity = double.MinValue;
            foreach (var finalVoxelTypeNode in finalVoxelTypeNodes) {
                var newVoxelTypeIntensity = finalVoxelTypeNode.Execute(x, y, z, evenCall);
                if (newVoxelTypeIntensity > voxelTypeIntensity) {
                    voxelType = finalVoxelTypeNode.VoxelType;
                    voxelTypeIntensity = newVoxelTypeIntensity;
                }
            }

            evenCall = !evenCall;
        }

        public void ExecuteAsBiomeSelector(double x, double y, double z,
                                           out int biomeId1, out GenerationFlow3D biomeFlow1,
                                           out int biomeId2, out GenerationFlow3D biomeFlow2,
                                           out double blend)
        {
            var defaultBiome = finalBiomeSelectionNodes[0];
            var weight1 = defaultBiome.Execute(x, y, z, evenCall);
            biomeId1 = defaultBiome.BiomeId;
            biomeFlow1 = defaultBiome.GenerationFlow;

            var weight2 = 0.0;
            biomeId2 = -1;
            biomeFlow2 = null;
            blend = 0;

            for (var i = 1; i < finalBiomeSelectionNodes.Length; i++) {
                var biomeSelectionNode = finalBiomeSelectionNodes[i];
                var newWeight = biomeSelectionNode.Execute(x, y, z, evenCall);
                if (newWeight > weight1) {
                    biomeId2 = biomeId1;
                    biomeFlow2 = biomeFlow1;
                    weight2 = weight1;

                    biomeId1 = biomeSelectionNode.BiomeId;
                    biomeFlow1 = biomeSelectionNode.GenerationFlow;
                    weight1 = newWeight;
                }
            }

            if (biomeId2 >= 0 && weight1 > 0.01 && weight2 > 0.01) {
                blend = weight1 / (weight1 + weight2);
                if (blend > 0.99) {
                    blend = 0;
                    biomeId2 = -1;
                    biomeFlow2 = null;
                }
            } else {
                blend = 0;
                biomeId2 = -1;
                biomeFlow2 = null;
            }

            evenCall = !evenCall;
        }

        private FinalCallableNode CreateFinalCallableNode(NodeSerializable node, List<CachableCallableNode> cachable2DNodes, Dictionary<string, CallableNode> callableNodes)
        {
            var inputs = CreateInputNodes(node, cachable2DNodes, callableNodes);
            return new FinalCallableNode(node.Content.CreateModule(terrain, inputs));
        }

        private FinalVoxelTypeCallableNode CreateFinalVoxelTypeCallableNode(NodeSerializable node, VoxelType voxelType, List<CachableCallableNode> cachable2DNodes, Dictionary<string, CallableNode> callableNodes)
        {
            var inputs = CreateInputNodes(node, cachable2DNodes, callableNodes);
            return new FinalVoxelTypeCallableNode(node.Content.CreateModule(terrain, inputs), voxelType);
        }

        private FinalBiomeSelectionCallableNode CreateFinalBiomeSelectionCallableNode(NodeSerializable node, List<CachableCallableNode> cachable2DNodes, Dictionary<string, CallableNode> callableNodes)
        {
            var inputs = CreateInputNodes(node, cachable2DNodes, callableNodes);
            var module = (FinalBiomeSelectionNode) node.Content.CreateModule(terrain, inputs);
            return new FinalBiomeSelectionCallableNode(module, module.BiomeId, module.GenerationFlow);
        }

        private CachableCallableNode CreateCachable2DCallableNode(NodeSerializable node, List<CachableCallableNode> cachable2DNodes, Dictionary<string, CallableNode> callableNodes)
        {
            var inputs = CreateInputNodes(node, cachable2DNodes, callableNodes);
            var cachableNode = new CachableCallableNode(node.Content.CreateModule(terrain, inputs));
            cachable2DNodes.Add(cachableNode);
            return cachableNode;
        }

        private CallableNode CreateCallableNode(NodeSerializable node, List<CachableCallableNode> cachable2DNodes, Dictionary<string, CallableNode> callableNodes)
        {
            var inputs = CreateInputNodes(node, cachable2DNodes, callableNodes);
            return new CallableNode(node.Content.CreateModule(terrain, inputs));
        }

        private List<CallableNode> CreateInputNodes(NodeSerializable node, List<CachableCallableNode> cachable2DNodes, Dictionary<string, CallableNode> callableNodes)
        {
            var connections = graph3D.GetAllInputConnectionsOfNode(node);
            var inputs = new List<CallableNode>(node.Content.InputCount);
            var ci = 0;

            for (var i = 0; i < node.Content.InputCount; ++i) {
                if (ci < connections.Count && connections[ci].InPoint.GetIndexInNode() == i) {
                    var connection = connections[ci];
                    ++ci;
                    CallableNode newModule;
                    if (!callableNodes.TryGetValue(connection.OutPoint.Node.GUID, out newModule)) {
                        if (IsCachable2DNode(connection.OutPoint.Node)) {
                            newModule = CreateCachable2DCallableNode(connection.OutPoint.Node, cachable2DNodes, callableNodes);
                        } else {
                            newModule = CreateCallableNode(connection.OutPoint.Node, cachable2DNodes, callableNodes);
                        }

                        callableNodes.Add(connection.OutPoint.Node.GUID, newModule);
                    }

                    inputs.Add(newModule);
                } else if (ci >= connections.Count || connections[ci].InPoint.GetIndexInNode() > i) {
                    if (node.Content.MandatoryInputs[i]) {
                        throw new InvalidFlowException(string.Format("Node '{0}' with GUID '{1}' misses some input(s).", node.Content.Title, node.GUID));
                    }

                    inputs.Add(null);
                } else {
                    throw new InvalidFlowException(string.Format("Node '{0}' with GUID '{1}' has some invalid connections.", node.Content.Title, node.GUID));
                }
            }


            return inputs;
        }

        private bool IsCachable2DNode(NodeSerializable node)
        {
            if (!graph3D.Is2D(node))
                return false;

            var outConnections = graph3D.GetAllOutputConnectionsOfNode(node);
            foreach (var outConnection in outConnections) {
                if (!graph3D.Is2D(outConnection.InPoint.Node))
                    return true;
            }

            return false;
        }
    }
}