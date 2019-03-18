namespace UltimateTerrains
{
    public abstract class TransformerNode : IGeneratorNode
    {
        protected readonly CallableNode Input;

        protected TransformerNode(CallableNode input)
        {
            if (input == null) {
                throw new InvalidFlowException(string.Format("Node of type {0} misses some mandatory input(s).", this.GetType()));
            }

            Input = input;
        }

        public abstract bool Is2D { get; }
        public abstract double Execute(double x, double y, double z, CallableNode flow);
    }
}