namespace UltimateTerrains
{
    public class FinalBiomeSelectionCallableNode : FinalCallableNode
    {
        private readonly int biomeId;
        private readonly GenerationFlow3D generationFlow;

        public FinalBiomeSelectionCallableNode(IGeneratorNode node, int biomeId, GenerationFlow3D generationFlow) : base(node)
        {
            this.biomeId = biomeId;
            this.generationFlow = generationFlow;
        }

        public GenerationFlow3D GenerationFlow {
            get { return generationFlow; }
        }

        public int BiomeId {
            get { return biomeId; }
        }
    }
}