using System;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public sealed class DetailsParam
    {
        [Serializable]
        public sealed class DetailObject : IEquatable<DetailObject>
        {
            public int Index;
            public Mesh Mesh;
            public Material Material;
            public float ObjectProbability = 1f;

            public bool Equals(DetailObject other)
            {
                return Mesh == other.Mesh && Material == other.Material;
            }
        }

        // Properties
        public string Name = "New Detail";
        public float MinNormalY = 0.75f;
        public float MaxNormalY = 1.01f;
        public float VerticalOffset = 0f;
        public float ObjectsNoiseFrequency = 10f;
        [SerializeField] private DetailObject[] objects;
        private int objectCount;
        public float DensityDistance = 6f;
        private double densityVoxX;
        private double densityVoxXInverse;
        private double densityVoxZ;
        private double densityVoxZInverse;
        public int MaxLevelIndex = 1;
        public float MinScale = 0.5f;
        public float MaxScale = 1.5f;
        public float ScaleNoiseFrequency = 1f;
        public bool Rotate = true;
        private int maxLevel;

        public int MaxLevel {
            get { return maxLevel; }
        }

        public int ObjectCount {
            get { return objectCount; }
        }

        public void Init(Param p)
        {
            objectCount = Objects.Length;
            densityVoxX = p.SizeXVoxel * DensityDistance;
            densityVoxXInverse = 1.0 / densityVoxX;
            densityVoxZ = p.SizeZVoxel * DensityDistance;
            densityVoxZInverse = 1.0 / densityVoxZ;
            maxLevel = UnitConverter.LevelIndexToLevel(MaxLevelIndex - 1);

            Validate();
        }

        private void Validate()
        {
            if (DensityDistance < 0.1f) {
                UDebug.Fatal("Density distance must be greater than or equal to 0.1");
            }

            if (objects == null) {
                UDebug.Fatal("List of detail items is null");
            }

            foreach (var obj in objects) {
                if (obj.Mesh == null || obj.Material == null) {
                    UDebug.Fatal("At least one detail item doesn't have a mesh and/or a material.");
                }
            }
        }

        public DetailObject[] Objects {
            get {
                if (objects == null || objects.Length == 0) {
                    objects = new DetailObject[1];
                    objects[0] = new DetailObject();
                }

                return objects;
            }
            set { objects = value; }
        }

        public double DensityX {
            get { return densityVoxX; }
        }

        public double DensityXInverse {
            get { return densityVoxXInverse; }
        }

        public double DensityZ {
            get { return densityVoxZ; }
        }

        public double DensityZInverse {
            get { return densityVoxZInverse; }
        }
    }
}