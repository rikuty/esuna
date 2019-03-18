using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public class Final3DNodeSerializable : NodeContentSerializable
    {
        public override string Title {
            get { return "Final"; }
        }

        public override int InputCount {
            get { return 1; }
        }

        public override string[] InputNames {
            get {
                return new[]
                {
                    "Voxel value"
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

        public override void OnEditorGUI(UltimateTerrain uTerrain)
        {
#if UNITY_EDITOR
            EditorGUILayout.LabelField("Voxel final value");
#endif
        }

        public override IGeneratorNode CreateModule(UltimateTerrain uTerrain, List<CallableNode> inputs)
        {
            return new Final3DNode(inputs[0]);
        }
    }
}