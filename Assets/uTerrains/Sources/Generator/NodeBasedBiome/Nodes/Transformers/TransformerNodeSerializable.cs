using System.Collections.Generic;
using UltimateTerrainsEditor;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public abstract class TransformerNodeSerializable : NodeContentSerializable
    {
        public override int InputCount {
            get { return 1; }
        }

        public override string[] InputNames {
            get {
                return new[]
                {
                    "Input"
                };
            }
        }

        public override bool[] MandatoryInputs {
            get { return new[] {true}; }
        }

        public override int EditorWidth {
            get { return 100; }
        }
    }
}