using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public abstract class FilterNodeSerializable : NodeContentSerializable
    {
        public override NodeLayer Layer {
            get { return NodeLayer.LayerDetermindeByInputs; }
        }

        public override int InputCount {
            get { return 2; }
        }

        public override string[] InputNames {
            get {
                return new[]
                {
                    "Input",
                    "Mask"
                };
            }
        }

        public override bool[] MandatoryInputs {
            get { return new[] {true, false}; }
        }

        public override int EditorWidth {
            get { return 100; }
        }

        [SerializeField] protected float Intensity = 1f;

        public override void OnEditorGUI(UltimateTerrain uTerrain)
        {
#if UNITY_EDITOR
            Intensity = EditorGUILayout.Slider("Intensity", Intensity, 0f, 1f);
#endif
        }
    }
}