using UnityEngine;

namespace UltimateTerrainsEditor
{
    public class SphericalReticleForEditor : Reticle
    {
        private Transform center;
        private Transform bound;
        private Renderer centerR;
        private Renderer boundR;

        // Use this for initialization
        public override void Initialize()
        {
            center = transform.Find("Center");
            center.gameObject.hideFlags = ReticleHideFlags;
            center.localPosition = Vector3.zero;
            centerR = center.gameObject.GetComponent<Renderer>();
            
            bound = transform.Find("Bound");
            bound.gameObject.hideFlags = ReticleHideFlags;
            bound.localPosition = Vector3.zero;
            boundR = bound.gameObject.GetComponent<Renderer>();
        }

        public void SetPositionAndSize(Vector3 position, float radius)
        {
            transform.position = position;
            bound.localScale = Vector3.one * radius * 2f;
            center.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);
        }

        public void EnableRenderer(bool enabled)
        {
            centerR.enabled = enabled;
            boundR.enabled = enabled;
        }

        private void OnDestroy()
        {
            if (center) DestroyImmediate(center.gameObject);
            if (bound) DestroyImmediate(bound.gameObject);
        }
    }
}