﻿using UnityEngine;

namespace UltimateTerrainsEditor
{
    public class CapsuleReticleForEditor : Reticle
    {
        private Transform start;
        private Transform end;
        private Transform cylinder;
        private Transform startBounds;
        private Transform endBounds;
        
        private Renderer startR;
        private Renderer endR;
        private Renderer cylinderR;
        private Renderer startBoundsR;
        private Renderer endBoundsR;

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
            
            startBounds = transform.Find("StartBounds");
            startBounds.gameObject.hideFlags = ReticleHideFlags;
            startBounds.localPosition = Vector3.zero;
            startBoundsR = startBounds.gameObject.GetComponent<Renderer>();
            
            endBounds = transform.Find("EndBounds");
            endBounds.gameObject.hideFlags = ReticleHideFlags;
            endBounds.localPosition = Vector3.zero;
            endBoundsR = endBounds.gameObject.GetComponent<Renderer>();
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
            
            startBounds.position = startPos;
            startBounds.localScale = Vector3.one * radius * 2f;
            endBounds.position = endPos;
            endBounds.localScale = Vector3.one * radius * 2f;
        }

        public void EnableRenderer(bool enabled)
        {
            startR.enabled = enabled;
            endR.enabled = enabled;
            cylinderR.enabled = enabled;
            startBoundsR.enabled = enabled;
            endBoundsR.enabled = enabled;
        }
        
        private void OnDestroy()
        {
            if (start) DestroyImmediate(start.gameObject);
            if (end) DestroyImmediate(end.gameObject);
            if (cylinder) DestroyImmediate(cylinder.gameObject);
        }
    }
}