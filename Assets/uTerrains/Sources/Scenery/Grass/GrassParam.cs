using System;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public sealed class GrassParam
    {
        [Serializable]
        public sealed class GrassMaterial
        {
            public int GrassMaterialIndex;
            public float MaterialProbability = 1f;
        }

        // Properties
        public float TileX = 2f;
        public Color BaseColor = new Color(0.274f, 0.784f, 0.156f, 0f);
        public float BaseHeight = 3f;
        public Color DirtyColor = new Color(0.384f, 0.439f, 0.027f, 0f);
        public float DirtyHeight = 0.8f;
        public float MinHeight = 0.5f;
        public float MinNormalY = 0.8f;
        public float GrassSize = 0.5f;
        public float NoiseFrequency = 0.01f;
        public float MaterialNoiseFrequency = 0.08f;

        [SerializeField] private GrassMaterial[]
            materials;

        public float GrassDensity = 1f;
        public float Dissemination = 0.25f;
        private float grassDensityVoxX;
        private float grassDensityVoxXInverse;
        private float grassDensityVoxZ;
        private float grassDensityVoxZInverse;

        public void Init(Param p, int grassMaterialsCount)
        {
            grassDensityVoxX = 1.0f / GrassDensity;
            grassDensityVoxXInverse = GrassDensity;
            grassDensityVoxZ = 1.0f / GrassDensity;
            grassDensityVoxZInverse = GrassDensity;

            Validate(grassMaterialsCount);
        }

        private void Validate(int grassMaterialsCount)
        {
            if (GrassDensity < 0.01f) {
                UDebug.Fatal("GrassDensity must be greater than or equal to 0.01");
            }

            if (materials == null || materials.Length == 0) {
                UDebug.Fatal("At least one material must be defined for grass.");
            }

            for (var i = 0; i < materials.Length; ++i) {
                if (materials[i].GrassMaterialIndex < 0) {
                    UDebug.Fatal("The grass material index must be greater than or equal to 0.");
                }

                if (materials[i].GrassMaterialIndex >= grassMaterialsCount) {
                    UDebug.Fatal("The grass material index is equal to " + materials[i].GrassMaterialIndex + " but there is only " + grassMaterialsCount + " grass material(s).");
                }
            }
        }

        public GrassMaterial[] Materials {
            get {
                if (materials == null || materials.Length == 0) {
                    materials = new GrassMaterial[1];
                    materials[0] = new GrassMaterial();
                }

                return materials;
            }
            set { materials = value; }
        }

        public float GrassDensityX {
            get { return grassDensityVoxX; }
        }

        public float GrassDensityXInverse {
            get { return grassDensityVoxXInverse; }
        }

        public float GrassDensityZ {
            get { return grassDensityVoxZ; }
        }

        public float GrassDensityZInverse {
            get { return grassDensityVoxZInverse; }
        }
    }
}