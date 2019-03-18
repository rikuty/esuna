namespace UltimateTerrains
{
    public interface IGeneratorNode
    {
        bool Is2D { get; }
        double Execute(double x, double y, double z, CallableNode flow);
    }
}