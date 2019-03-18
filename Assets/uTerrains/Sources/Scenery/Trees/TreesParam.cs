using System;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public sealed class TreesParam
    {
        [Serializable]
        public sealed class TreeObject
        {
            public int Index;
            public GameObject Object;
            public float ObjectProbability = 1f;
        }

        // Properties
        public string Name = "New Tree Group";
        public float MinNormalY = 0.75f;
        public float VerticalOffset = -0.5f;
        public float MinScale = 0.5f;
        public float MaxScale = 1.5f;
        public float ScaleNoiseFrequency = 1f;
        public float ObjectsNoiseFrequency = 1f;
        public bool Rotate = true;
        [SerializeField] private TreeObject[] objects;
        private int objectCount;

        public int ObjectCount {
            get { return objectCount; }
        }

        public void Init(Param p)
        {
            objectCount = Objects.Length;
            Validate();
        }

        private void Validate()
        {
        }

        public TreeObject[] Objects {
            get {
                if (objects == null || objects.Length == 0) {
                    objects = new TreeObject[1];
                    objects[0] = new TreeObject();
                }

                return objects;
            }
            set { objects = value; }
        }
    }
}