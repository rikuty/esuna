using System;
using System.Collections.Generic;
using UltimateTerrainsEditor;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UltimateTerrains
{
    [PrettyTypeName("Biome Selection")]
    [Serializable]
    public sealed class FinalBiomeSelectionNodeSerializable : NodeContentSerializable
    {
        [SerializeField] private int biomeIndex;

        public override string Title {
            get { return "Biome Selection"; }
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

        public override void OnEditorGUI(UltimateTerrain uTerrain)
        {
#if UNITY_EDITOR
            biomeIndex = EditorUtils.BiomeFieldMandatory("Biome", biomeIndex, uTerrain.GetComponent<GeneratorModulesComponent>());
#endif
        }

        public override IGeneratorNode CreateModule(UltimateTerrain uTerrain, List<CallableNode> inputs)
        {
            var biome = uTerrain.GetComponent<GeneratorModulesComponent>().Biomes[biomeIndex];
            return new FinalBiomeSelectionNode(inputs[0], biomeIndex, new GenerationFlow3D(uTerrain, biome.Graph3D));
        }
    }
}