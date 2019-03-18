using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public class ConstantNodeSerializable : NodeContentSerializable
    {
        public override string Title {
            get { return "Constant"; }
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
            get { return 100; }
        }

        public override NodeLayer Layer {
            get { return NodeLayer.Layer2D; }
        }

        public string ConstantName {
            set { constantName = value; }
        }

        [SerializeField] private float value;
        [SerializeField] private string constantName = "Constant name";

        public override void OnEditorGUI(UltimateTerrain uTerrain)
        {
#if UNITY_EDITOR
            constantName = EditorGUILayout.TextField(constantName);
            value = EditorGUILayout.FloatField(value);
#endif
        }

        public override IGeneratorNode CreateModule(UltimateTerrain uTerrain, List<CallableNode> inputs)
        {
            return new ConstantNode(value);
        }
    }
}