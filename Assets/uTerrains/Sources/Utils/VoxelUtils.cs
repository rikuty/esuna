using System;

namespace UltimateTerrains
{
    public static class VoxelUtils
    {
        public static double Interpolate(double v1, double v2)
        {
            return Math.Max(0d, Math.Min(1d, v1 / (v1 - v2)));
        }

        public static double InterpolateConverseFromV1(double v1, double interpolatedValue)
        {
            return v1 - v1 / interpolatedValue;
        }

        public static double InterpolateConverseFromV2(double v2, double interpolatedValue)
        {
            return interpolatedValue * v2 / (interpolatedValue - 1);
        }
    }
}