namespace UltimateTerrains
{
    public abstract class Primitive3DNode : IGeneratorNode
    {
        public bool Is2D {
            get { return false; }
        }

        public abstract double Execute(double x, double y, double z, CallableNode flow);
    }
}