using System;
using UnityEngine;

namespace UltimateTerrains
{
    public abstract class FilterNode : IGeneratorNode
    {
        protected readonly CallableNode Input;
        private readonly CallableNode maskInput;
        private readonly double intensity;

        protected FilterNode(CallableNode input, CallableNode maskInput, double intensity)
        {
            if (input == null) {
                throw new InvalidFlowException(string.Format("Node of type {0} misses some mandatory input(s).", this.GetType()));
            }

            if (maskInput == null) {
                maskInput = new CallableNode(new ConstantNode(1.0));
            }

            this.Input = input;
            this.maskInput = maskInput;
            this.intensity = intensity;
        }

        public bool Is2D {
            get { return Input.Is2D && maskInput.Is2D; }
        }

        public double Execute(double x, double y, double z, CallableNode flow)
        {
            var inValue = flow.Call(Input, x, y, z);
            var maskValue = flow.Call(maskInput, x, y, z);
            var maskedIntensity = UMath.Clamp(0, 1, intensity * maskValue);
            return maskedIntensity * ExecuteFilter(x, y, z, flow, inValue);
        }

        protected abstract double ExecuteFilter(double x, double y, double z, CallableNode flow, double inputValue);
    }
}