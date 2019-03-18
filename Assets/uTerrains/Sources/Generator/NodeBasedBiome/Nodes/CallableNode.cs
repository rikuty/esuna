namespace UltimateTerrains
{
    public class CallableNode
    {
        protected readonly IGeneratorNode node;
        private double cachedValue;
        private double cachedX, cachedY, cachedZ;
        private bool isEvenCall;

        public CallableNode(IGeneratorNode node)
        {
            this.node = node;
        }

        public bool Is2D {
            get { return node.Is2D; }
        }

        protected virtual double CallExecute(double x, double y, double z, bool evenCall)
        {
            if (isEvenCall == evenCall && UMath.Approximately(cachedX, x) && UMath.Approximately(cachedY, y) && UMath.Approximately(cachedZ, z)) {
                return cachedValue;
            }

            cachedValue = node.Execute(x, y, z, this);
            isEvenCall = evenCall;
            cachedX = x;
            cachedY = y;
            cachedZ = z;
            return cachedValue;
        }

        /// <summary>
        /// Performs a call on the given node and returns its output value.
        /// </summary>
        /// <param name="nodeToCall">The node to be called. Must not be null.</param>
        /// <param name="x">X coordinate of the position where the value must be computed.</param>
        /// <param name="y">Y coordinate of the position where the value must be computed.</param>
        /// <param name="z">Z coordinate of the position where the value must be computed.</param>
        /// <returns>The output of the valled node.</returns>
        public double Call(CallableNode nodeToCall, double x, double y, double z)
        {
            return nodeToCall.CallExecute(x, y, z, isEvenCall);
        }
    }
}