using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public abstract class CombinerNodeSerializable : NodeContentSerializable
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
                    "Input 1",
                    "Input 2"
                };
            }
        }

        public override bool[] MandatoryInputs {
            get { return new[] {true, true}; }
        }

        public override int EditorWidth {
            get { return 40; }
        }
    }
}