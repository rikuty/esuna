using UnityEngine;

namespace UltimateTerrainsEditor
{
    public class CylinderReticleForEditor : Reticle
    {
        private Transform start;
        private Transform end;
        private Transform cylinder;
        private Renderer startR;
        private Renderer endR;
        private Renderer cylinderR;

        // Use this for initialization
        public override void Initialize()
        {
            start = transform.Find("Start");
            start.gameObject.hideFlags = ReticleHideFlags;
            start.localPosition = Vector3.zero;
            startR = start.gameObject.GetComponent<Renderer>();

            end = transform.Find("End");
            end.gameObject.hideFlags = ReticleHideFlags;
            end.localPosition = Vector3.zero;
            endR = end.gameObject.GetComponent<Renderer>();

            cylinder = transform.Find("Cylinder");
            cylinder.gameObject.hideFlags = ReticleHideFlags;
            cylinder.localPosition = Vector3.zero;
            cylinderR = cylinder.gameObject.GetComponent<Renderer>();
        }

        public void SetPositionsAndRadius(Vector3 startPos, Vector3 endPos, float radius)
        {
            if (endPos == startPos) {
                endPos = startPos + Vector3.right;
            }

            start.position = startPos;
            start.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);
            end.position = endPos;
            end.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);
            cylinder.position = (endPos + startPos) * 0.5f;
            cylinder.localScale = new Vector3(radius * 2f, (endPos - startPos).magnitude * 0.5f, radius * 2f);
            var euler = Quaternion.LookRotation(endPos - startPos).eulerAngles;
            euler.x -= 90f;
            cylinder.rotation = Quaternion.Euler(euler);
        }

        public void EnableRenderer(bool enabled)
        {
            startR.enabled = enabled;
            endR.enabled = enabled;
            cylinderR.enabled = enabled;
        }

        private void OnDestroy()
        {
            if (start) DestroyImmediate(start.gameObject);
            if (end) DestroyImmediate(end.gameObject);
            if (cylinder) DestroyImmediate(cylinder.gameObject);
        }
    }
}