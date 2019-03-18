namespace UltimateTerrains
{
    public class ConstantNode : IGeneratorNode
    {
        private readonly double value;

        public ConstantNode(double value)
        {
            this.value = value;
        }

        public bool Is2D {
            get { return true; }
        }

        public double Execute(double x, double y, double z, CallableNode flow)
        {
            return value;
        }
    }
}