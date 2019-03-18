using UnityEngine;

namespace UltimateTerrainsEditor
{
    public abstract class Reticle : MonoBehaviour
    {
        protected HideFlags ReticleHideFlags {
            get { return Application.isPlaying ? HideFlags.None : HideFlags.HideAndDontSave; }
        }

        public abstract void Initialize();
    }
}