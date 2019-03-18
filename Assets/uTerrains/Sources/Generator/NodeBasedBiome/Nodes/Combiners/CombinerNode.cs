namespace UltimateTerrains
{
    public abstract class CombinerNode : IGeneratorNode
    {
        protected readonly CallableNode Right;
        protected readonly CallableNode Left;

        protected CombinerNode(CallableNode right, CallableNode left)
        {
            if (right == null) {
                throw new InvalidFlowException(string.Format("Node of type {0} misses some mandatory input(s).", this.GetType()));
            }

            if (left == null) {
                throw new InvalidFlowException(string.Format("Node of type {0} misses some mandatory input(s).", this.GetType()));
            }

            Right = right;
            Left = left;
        }

        public bool Is2D {
            get { return Right.Is2D && Left.Is2D; }
        }

        public abstract double Execute(double x, double y, double z, CallableNode flow);
    }
}