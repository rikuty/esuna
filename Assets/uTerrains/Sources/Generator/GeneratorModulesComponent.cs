using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    public sealed class GeneratorModulesComponent : MonoBehaviour
    {
        [SerializeField] public int
            MinX, MinY, MinZ, MaxX, MaxY, MaxZ;

        [SerializeField] public bool
            HasMinX, HasMinY, HasMinZ, HasMaxX, HasMaxY, HasMaxZ;

        [SerializeField] private List<Biome>
            biomes;

        [SerializeField] private BiomeSelector
            biomeSelector;

        public List<Biome> Biomes {
            get { return biomes; }
        }

        public BiomeSelector BiomeSelector {
            get { return biomeSelector; }
            set { biomeSelector = value; }
        }

        public void Init(bool clear = false)
        {
            if (clear || biomes == null || biomes.Count == 0) {
                biomes = new List<Biome>(8) {(Biome) ScriptableObject.CreateInstance(typeof(Biome))};
            }

            if (clear || biomeSelector == null) {
                biomeSelector = (BiomeSelector) ScriptableObject.CreateInstance(typeof(BiomeSelector));
                biomeSelector.Init(true);
            }
        }

        public bool Validate(UltimateTerrain terrain, bool logErrors)
        {
            if (biomes.Count < 1) {
                if (logErrors)
                    UDebug.LogError("There must be at least one biome.");
                return false;
            }

            for (var i = 0; i < biomes.Count; ++i) {
                if (biomes[i] == null) {
                    if (logErrors)
                        UDebug.LogError("Some biome(s) is/are undefined.");
                    return false;
                }

                if (!biomes[i].Validate(terrain, logErrors)) {
                    return false;
                }
            }

            if (biomeSelector == null) {
                if (logErrors)
                    UDebug.LogError("Biome selector is undefined.");
                return false;
            }

            if (!biomeSelector.Validate(terrain, logErrors)) {
                return false;
            }

            return true;
        }
    }
}