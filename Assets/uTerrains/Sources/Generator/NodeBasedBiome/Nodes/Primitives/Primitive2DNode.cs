namespace UltimateTerrains
{
    public abstract class Primitive2DNode : IGeneratorNode
    {
        public bool Is2D {
            get { return true; }
        }

        public abstract double Execute(double x, double y, double z, CallableNode flow);
    }
}