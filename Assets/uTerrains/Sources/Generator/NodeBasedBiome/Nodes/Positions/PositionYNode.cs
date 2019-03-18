namespace UltimateTerrains
{
    public class PositionYNode : IGeneratorNode
    {
        public bool Is2D {
            get { return false; }
        }

        public double Execute(double x, double y, double z, CallableNode flow)
        {
            return y;
        }
    }
}