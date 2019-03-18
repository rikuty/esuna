using UnityEngine;

namespace UltimateTerrainsEditor
{
    public class ReticleForEditor : Reticle
    {
        private Transform[] corners;
        private Transform[] edges;
        private Renderer[] renderers;

        public Vector3 CenterPosition { get; private set; }

        // Use this for initialization
        public override void Initialize()
        {
            corners = new Transform[8];
            edges = new Transform[12];
            renderers = new Renderer[8 + 12];

            for (var i = 0; i < 8; ++i) {
                corners[i] = transform.Find("Corner" + i);
                corners[i].gameObject.hideFlags = ReticleHideFlags;
                renderers[i] = corners[i].gameObject.GetComponent<Renderer>();
            }

            for (var i = 0; i < 12; ++i) {
                edges[i] = transform.Find("Edge" + i);
                edges[i].gameObject.hideFlags = ReticleHideFlags;
                renderers[i + 8] = edges[i].gameObject.GetComponent<Renderer>();
            }
        }

        public void SetPositionAndSize(Vector3 position, Vector3 size)
        {
            CenterPosition = position;
            var from = position - size * 0.5f;
            var to = position + size * 0.5f;
            transform.position = from;
            var locTo = to - from;
            Scale(Mathf.Sqrt(locTo.magnitude) / 10f);
            corners[0].localPosition = new Vector3(0, 0, 0);
            corners[1].localPosition = new Vector3(locTo.x, 0, 0);
            corners[2].localPosition = new Vector3(locTo.x, 0, locTo.z);
            corners[3].localPosition = new Vector3(0, 0, locTo.z);
            corners[4].localPosition = new Vector3(0, locTo.y, 0);
            corners[5].localPosition = new Vector3(locTo.x, locTo.y, 0);
            corners[6].localPosition = new Vector3(locTo.x, locTo.y, locTo.z);
            corners[7].localPosition = new Vector3(0, locTo.y, locTo.z);

            var originalScale = edges[0].localScale.x;
            edges[0].localPosition = new Vector3(0.5f * locTo.x, 0, 0);
            edges[0].localScale = new Vector3(locTo.x, originalScale, originalScale);

            edges[1].localPosition = new Vector3(locTo.x, 0, 0.5f * locTo.z);
            edges[1].localScale = new Vector3(originalScale, originalScale, locTo.z);

            edges[2].localPosition = new Vector3(0.5f * locTo.x, 0, locTo.z);
            edges[2].localScale = new Vector3(locTo.x, originalScale, originalScale);

            edges[3].localPosition = new Vector3(0, 0, 0.5f * locTo.z);
            edges[3].localScale = new Vector3(originalScale, originalScale, locTo.z);

            edges[4].localPosition = new Vector3(0.5f * locTo.x, locTo.y, 0);
            edges[4].localScale = new Vector3(locTo.x, originalScale, originalScale);

            edges[5].localPosition = new Vector3(locTo.x, locTo.y, 0.5f * locTo.z);
            edges[5].localScale = new Vector3(originalScale, originalScale, locTo.z);

            edges[6].localPosition = new Vector3(0.5f * locTo.x, locTo.y, locTo.z);
            edges[6].localScale = new Vector3(locTo.x, originalScale, originalScale);

            edges[7].localPosition = new Vector3(0, locTo.y, 0.5f * locTo.z);
            edges[7].localScale = new Vector3(originalScale, originalScale, locTo.z);

            edges[8].localPosition = new Vector3(0, 0.5f * locTo.y, 0);
            edges[8].localScale = new Vector3(originalScale, locTo.y, originalScale);

            edges[9].localPosition = new Vector3(locTo.x, 0.5f * locTo.y, 0);
            edges[9].localScale = new Vector3(originalScale, locTo.y, originalScale);

            edges[10].localPosition = new Vector3(locTo.x, 0.5f * locTo.y, locTo.z);
            edges[10].localScale = new Vector3(originalScale, locTo.y, originalScale);

            edges[11].localPosition = new Vector3(0, 0.5f * locTo.y, locTo.z);
            edges[11].localScale = new Vector3(originalScale, locTo.y, originalScale);
        }

        private void Scale(float scale)
        {
            for (var i = 0; i < 8; ++i) {
                corners[i].localScale = Vector3.one * scale;
            }

            for (var i = 0; i < 12; ++i) {
                edges[i].localScale = Vector3.one * 0.5f * scale;
            }
        }

        public void EnableRenderer(bool enabled)
        {
            for (var i = 0; i < 8 + 12; ++i) {
                renderers[i].enabled = enabled;
            }
        }
        
        private void OnDestroy()
        {
            foreach (var corner in corners) {
                if (corner) DestroyImmediate(corner.gameObject);
            }

            foreach (var edge in edges) {
                if (edge) DestroyImmediate(edge.gameObject);
            }
        }
    }
}