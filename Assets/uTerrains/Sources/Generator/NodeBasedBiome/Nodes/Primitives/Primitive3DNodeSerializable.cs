using System.Collections.Generic;
using UltimateTerrainsEditor;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    public abstract class Primitive3DNodeSerializable : NodeContentSerializable
    {
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
            get { return 300; }
        }

        public override NodeLayer Layer {
            get { return NodeLayer.Layer3D; }
        }
    }
}