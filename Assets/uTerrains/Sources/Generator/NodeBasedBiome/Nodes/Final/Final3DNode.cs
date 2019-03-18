namespace UltimateTerrains
{
    public class Final3DNode : IFinalGeneratorNode
    {
        private readonly CallableNode input;

        public Final3DNode(CallableNode input)
        {
            if (input == null) {
                throw new InvalidFlowException(string.Format("Node of type {0} misses some mandatory input(s).", this.GetType()));
            }
            
            this.input = input;
        }

        public bool Is2D {
            get { return false; }
        }

        public double Execute(double x, double y, double z, CallableNode flow)
        {
            return flow.Call(input, x, y, z);
        }
    }
}