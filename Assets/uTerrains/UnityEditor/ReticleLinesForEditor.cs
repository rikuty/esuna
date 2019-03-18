using UnityEngine;

namespace UltimateTerrainsEditor
{
    public class ReticleLinesForEditor : Reticle
    {
        private Transform[] corners;
        private LineRenderer[] edges;
        private Renderer[] renderers;

        // Use this for initialization
        public override void Initialize()
        {
            corners = new Transform[8];
            edges = new LineRenderer[12];
            renderers = new Renderer[8];

            for (var i = 0; i < 8; ++i) {
                corners[i] = transform.Find("Corner" + i);
                corners[i].gameObject.hideFlags = ReticleHideFlags;
                renderers[i] = corners[i].gameObject.GetComponent<Renderer>();
            }

            for (var i = 0; i < 12; ++i) {
                edges[i] = transform.Find("Edge" + i).GetComponent<LineRenderer>();
                edges[i].gameObject.hideFlags = ReticleHideFlags;
                edges[i].positionCount = 2;
            }
        }

        public void SetCornerAndEdges(Vector3 corner, Vector3 vL, Vector3 vH, Vector3 vl)
        {
            var oppositeCorner = corner + vL + vH + vl;
            transform.position = corner;
            var locTo = oppositeCorner - corner;
            Scale(Mathf.Sqrt(locTo.magnitude) / 10f);

            corners[0].localPosition = Vector3.zero;
            corners[1].localPosition = vL;
            corners[2].localPosition = vL + vl;
            corners[3].localPosition = vl;
            corners[4].localPosition = vH;
            corners[5].localPosition = vH + vL;
            corners[6].localPosition = vH + vL + vl;
            corners[7].localPosition = vH + vl;

            edges[0].SetPosition(0, corners[0].position);
            edges[0].SetPosition(1, corners[1].position);
            edges[1].SetPosition(0, corners[1].position);
            edges[1].SetPosition(1, corners[2].position);
            edges[2].SetPosition(0, corners[2].position);
            edges[2].SetPosition(1, corners[3].position);
            edges[3].SetPosition(0, corners[3].position);
            edges[3].SetPosition(1, corners[0].position);

            edges[0 + 4].SetPosition(0, corners[0].position);
            edges[0 + 4].SetPosition(1, corners[0 + 4].position);
            edges[1 + 4].SetPosition(0, corners[1].position);
            edges[1 + 4].SetPosition(1, corners[1 + 4].position);
            edges[2 + 4].SetPosition(0, corners[2].position);
            edges[2 + 4].SetPosition(1, corners[2 + 4].position);
            edges[3 + 4].SetPosition(0, corners[3].position);
            edges[3 + 4].SetPosition(1, corners[3 + 4].position);

            edges[0 + 8].SetPosition(0, corners[0 + 4].position);
            edges[0 + 8].SetPosition(1, corners[1 + 4].position);
            edges[1 + 8].SetPosition(0, corners[1 + 4].position);
            edges[1 + 8].SetPosition(1, corners[2 + 4].position);
            edges[2 + 8].SetPosition(0, corners[2 + 4].position);
            edges[2 + 8].SetPosition(1, corners[3 + 4].position);
            edges[3 + 8].SetPosition(0, corners[3 + 4].position);
            edges[3 + 8].SetPosition(1, corners[0 + 4].position);
        }

        private void Scale(float scale)
        {
            for (var i = 0; i < 8; ++i) {
                corners[i].localScale = Vector3.one * scale;
            }
        }

        public void EnableRenderer(bool enabled)
        {
            for (var i = 0; i < 8; ++i) {
                renderers[i].enabled = enabled;
            }

            for (var i = 0; i < 12; ++i) {
                edges[i].enabled = enabled;
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