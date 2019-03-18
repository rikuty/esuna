namespace UltimateTerrains
{
    public class CachableCallableNode : CallableNode
    {
        private double precomputedValue;
        
        public CachableCallableNode(IGeneratorNode node) : base(node)
        {
        }

        public void SetPreComputedValueFromCache(double value)
        {
            precomputedValue = value;
        }

        public double PreCompute(double x, double y, double z)
        {
            precomputedValue = node.Execute(x, y, z, this);
            return precomputedValue;
        }
        
        protected override double CallExecute(double x, double y, double z, bool evenCall)
        {
            return precomputedValue;
        } 
    }
}