using System.Collections.Generic;
using UltimateTerrainsEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public class PositionZNodeSerializable : NodeContentSerializable
    {
        public override string Title {
            get { return "Position"; }
        }

        public override int InputCount {
            get { return 0; }
        }

        public override string[] InputNames {
            get { return new string[] { }; }
        }

        public override bool[] MandatoryInputs {
            get { return new bool[] { }; }
        }

        public override int EditorWidth {
            get { return 20; }
        }

        public override NodeLayer Layer {
            get { return NodeLayer.Layer2D; }
        }

        public override void OnEditorGUI(UltimateTerrain uTerrain)
        {
#if UNITY_EDITOR
            EditorUtils.CenteredLabelField("Z", GUILayout.Width(44));
#endif
        }

        public override IGeneratorNode CreateModule(UltimateTerrain uTerrain, List<CallableNode> inputs)
        {
            return new PositionZNode();
        }
    }
}