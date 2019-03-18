namespace UltimateTerrains
{
    public sealed class FinalBiomeSelectionNode : IFinalGeneratorNode
    {
        private readonly CallableNode input;
        private readonly int biomeId;
        private readonly GenerationFlow3D generationFlow;

        public FinalBiomeSelectionNode(CallableNode input, int biomeId, GenerationFlow3D generationFlow)
        {
            if (input == null) {
                throw new InvalidFlowException(string.Format("Node of type {0} misses some mandatory input(s).", this.GetType()));
            }

            this.input = input;
            this.biomeId = biomeId;
            this.generationFlow = generationFlow;
        }

        public bool Is2D {
            get { return false; }
        }

        public GenerationFlow3D GenerationFlow {
            get { return generationFlow; }
        }

        public int BiomeId {
            get { return biomeId; }
        }

        public double Execute(double x, double y, double z, CallableNode flow)
        {
            var weight = flow.Call(input, x, y, z);
            return weight < 0 ? 0 : weight;
        }
    }
}