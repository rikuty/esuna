namespace UltimateTerrains
{
    public class FinalCallableNode : CallableNode
    {
        public FinalCallableNode(IGeneratorNode node) : base(node)
        {
        }

        public double Execute(double x, double y, double z, bool evenCall)
        {
            return this.CallExecute(x, y, z, evenCall);
        }
    }
}