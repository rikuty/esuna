using System;
using System.Collections.Generic;
using UltimateTerrainsEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    [PrettyTypeName("Voxel Type")]
    [Serializable]
    public class Final3DVoxelTypeNodeSerializable : NodeContentSerializable
    {
        [SerializeField] private int voxelTypeIndex;

        public override string Title {
            get { return "Voxel Type"; }
        }

        public override int InputCount {
            get { return 1; }
        }

        public override string[] InputNames {
            get {
                return new[]
                {
                    "Weight"
                };
            }
        }

        public override bool[] MandatoryInputs {
            get { return new[] {true}; }
        }

        public override int EditorWidth {
            get { return 100; }
        }

        public override bool IsFinal {
            get { return true; }
        }

        public override NodeLayer Layer {
            get { return NodeLayer.Layer3D; }
        }

        public ushort VoxelTypeIndex {
            get { return (ushort) voxelTypeIndex; }
        }

        public override void OnEditorGUI(UltimateTerrain uTerrain)
        {
#if UNITY_EDITOR
            EditorGUILayout.LabelField("Voxel type's quantity");
            voxelTypeIndex = EditorUtils.VoxelTypeFieldMandatory("Voxel type", voxelTypeIndex, uTerrain.VoxelTypeSet);
#endif
        }

        public override IGeneratorNode CreateModule(UltimateTerrain uTerrain, List<CallableNode> inputs)
        {
            var voxelType = uTerrain.VoxelTypeSet.GetVoxelType((ushort) voxelTypeIndex);
            return new Final3DVoxelTypeNode(inputs[0], voxelType);
        }
    }
}