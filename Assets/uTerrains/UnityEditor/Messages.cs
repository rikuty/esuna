using UnityEngine;

namespace UltimateTerrainsEditor
{
    public static class Messages
    {
        public static GUIContent LibnoiseDisplacement = new GUIContent("Displacement:", "This noise module assigns each Voronoi cell with a random constant value from a coherent-noise function. The displacement value controls the range of random values to assign to each cell. The range of random values is +/- the displacement value.");
        public static GUIContent LibnoiseApplyDistance = new GUIContent("Apply distance:", "Applying the distance from the nearest seed point to the output value causes the points in the Voronoi cells to increase in value the further away that point is from the nearest seed point.");
        public static GUIContent LibnoiseFrequency = new GUIContent("Frequency:", "Frequency of the first octave");
        public static GUIContent LibnoiseLacunarity = new GUIContent("Lacunarity:", "The lacunarity specifies the frequency multipler between successive octaves.\n\nThe effect of modifying the lacunarity is subtle; you may need to play with the lacunarity value to determine the effects.  For best results, set the lacunarity to a number between 1.5 and 3.5.");
        public static GUIContent LibnoiseOctaveCount = new GUIContent("Octave count:", "The number of octaves control the amount of detail of the noise.  Adding more octaves increases the detail of the noise, but with the drawback of increasing the calculation time.\n\nAn octave is one of the coherent-noise functions in a series of coherent-noise functions that are added together to form noise.");
        public static GUIContent LibnoiseOffset = new GUIContent("Offset:", "");
        public static GUIContent LibnoiseGain = new GUIContent("Gain:", "");
        public static GUIContent Libnoise = new GUIContent("Apply:", "");
    }
}