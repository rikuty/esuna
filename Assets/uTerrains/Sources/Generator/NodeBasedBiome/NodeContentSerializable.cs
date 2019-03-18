using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public abstract class NodeContentSerializable : ScriptableObject, IOnGUI
    {
        public abstract string Title { get; }
        
        public abstract int InputCount { get; }
        
        public abstract string[] InputNames { get; }
        
        public abstract bool[] MandatoryInputs { get; }
        
        public abstract int EditorWidth { get; }

        public virtual NodeLayer Layer {
            get { return NodeLayer.LayerDetermindeByInputs; }
        }

        public virtual bool IsFinal {
            get { return false; }
        }

        public abstract void OnEditorGUI(UltimateTerrain uTerrain);

        public abstract IGeneratorNode CreateModule(UltimateTerrain uTerrain, List<CallableNode> inputs);
    }
}