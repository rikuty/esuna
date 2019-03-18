using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public sealed class VoxelTypeSet
    {
#if UNITY_EDITOR
        [SerializeField] private List<bool> voxelTypesFoldoutForEditor;

        public List<bool> VoxelTypesFoldoutForEditor {
            get {
                if (voxelTypesFoldoutForEditor == null)
                    voxelTypesFoldoutForEditor = new List<bool>();
                return voxelTypesFoldoutForEditor;
            }
        }
#endif

        [SerializeField] private VoxelType[] serializableVoxelTypes;
        private Dictionary<string, VoxelType> voxelTypes;

        [SerializeField] private Material[] materials;

        [SerializeField] private Material[] grassMaterials;

        // Lazy cache
        private readonly Dictionary<int, Material[]> subMaterials = new Dictionary<int, Material[]>(64);
        private int materialsCount;

        public struct MaterialInfo
        {
            public bool IsMegaSplat;
            public bool IsRTP;
        }

        private MaterialInfo[] materialInfos;


        public void Init(Param p)
        {
            materialsCount = materials.Length;
            materialInfos = new MaterialInfo[materials.Length];
            for (var i = 0; i < materialInfos.Length; ++i) {
                materialInfos[i] = new MaterialInfo
                {
                    IsMegaSplat = materials[i].shader.name.Contains("MegaSplat"),
                    IsRTP = materials[i].shader.name.Contains("Relief Pack")
                };
            }

            voxelTypes = new Dictionary<string, VoxelType>();
            if (serializableVoxelTypes != null) {
                for (ushort index = 0; index < serializableVoxelTypes.Length; index++) {
                    var b = serializableVoxelTypes[index];
                    b.Index = index;
                    voxelTypes.Add(b.Name, b);
                    if (b.IsGrassEnabled) {
                        b.GrassParam.Init(p, grassMaterials != null ? grassMaterials.Length : 0);
                    }
                }
            }
        }

        /**
	 * @return a voxel type given its name.
	 */
        public VoxelType GetVoxelType(string name)
        {
            VoxelType b;
            if (voxelTypes.TryGetValue(name, out b)) {
                return b;
            }

            return null;
        }

        public VoxelType GetVoxelType(ushort index)
        {
            return serializableVoxelTypes[index];
        }

        public Material[] GetMaterials(int mask)
        {
            Material[] m;
            if (!subMaterials.TryGetValue(mask, out m)) {
                var subMatList = new List<Material>();
                for (var i = 0; i < materialsCount; ++i) {
                    if ((mask & (1 << i)) != 0) {
                        subMatList.Add(materials[i]);
                    }
                }

                m = subMatList.ToArray();
                subMaterials.Add(mask, m);
            }

            return m;
        }

        public Material[] Materials {
            get { return materials; }
            set { materials = value; }
        }

        public int MaterialsCount {
            get { return materials.Length; }
        }

        public MaterialInfo[] MaterialInfos {
            get { return materialInfos; }
        }

        public Material[] GrassMaterials {
            get { return grassMaterials; }
            set { grassMaterials = value; }
        }

        public string[] VoxelTypeNames {
            get {
                var arr = new string[serializableVoxelTypes.Length];
                for (var i = 0; i < arr.Length && i < serializableVoxelTypes.Length; ++i) {
                    arr[i] = serializableVoxelTypes[i].Name;
                }

                return arr;
            }
        }

        public VoxelType[] SerializableVoxelTypes {
            get { return serializableVoxelTypes; }
            set { serializableVoxelTypes = value; }
        }

        public bool Validate()
        {
            if (serializableVoxelTypes == null || serializableVoxelTypes.Length == 0) {
                UDebug.LogError("At least one voxel type must be defined.");
                return false;
            }

            var prioritySet = new HashSet<int>();
            for (var i = 0; i < serializableVoxelTypes.Length; ++i) {
                var vtype = serializableVoxelTypes[i];
                if (vtype.Priority < 0) {
                    UDebug.LogError("The voxel type " + vtype.Name + " has a priority lesser than 0. Priority must be greater or equal to zero.");
                    return false;
                }

                if (prioritySet.Contains(vtype.Priority)) {
                    UDebug.LogError("Two or more voxel types have the same priority. Each voxel type must have a unique priority.");
                    return false;
                }

                prioritySet.Add(vtype.Priority);
            }

            return true;
        }

        public void AssertVoxelTypeExists(string name)
        {
            if (GetVoxelType(name) == null) {
                UDebug.Fatal("There is no voxel type with name '" + name + "'.");
            }
        }

        public void AssertVoxelTypeExists(int index)
        {
            if (GetVoxelType((ushort) index) == null) {
                UDebug.Fatal("There is no voxel type with index '" + index + "'.");
            }
        }
    }
}