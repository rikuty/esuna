using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController: MonoBehaviour {

    [SerializeField] OVRGrabberBothHands rightHand;
    [SerializeField] OVRGrabberBothHands leftHand;

    [SerializeField] MeshCollider rightHandCollider;
    [SerializeField] MeshCollider leftHandCollider;

    [SerializeField] OvrAvatar ovrAvatar;

    private SkinnedMeshRenderer rightSkinnedMeshRenderer;
    private SkinnedMeshRenderer leftSkinnedMeshRenderer;

    private bool isInit = false;

    // Use this for initialization
    void Start () {

    }

    private void Update()
    {
        bool canGrab = true;
        canGrab &= rightHand.isGrabbableTriggerEnter;
        canGrab &= leftHand.isGrabbableTriggerEnter;
        canGrab &= rightHand.isGrabberTriggerEnter;
        canGrab &= leftHand.isGrabberTriggerEnter;
        if (canGrab)
        {
            rightHand.isWrapBegin = true;
        }
        else
        {
            rightHand.isWrapBegin = false;
            leftHand.isWrapBegin = false;
        }
    }

    // Update is called once per frame
    void LateUpdate () {
        if (!isInit && ovrAvatar.trackedComponents.Count != 0)
        {
            isInit = true;
            leftSkinnedMeshRenderer = ovrAvatar.trackedComponents["hand_left"].RenderParts[0].mesh;
            rightSkinnedMeshRenderer = ovrAvatar.trackedComponents["hand_right"].RenderParts[0].mesh;

            rightHandCollider.sharedMesh = rightSkinnedMeshRenderer.sharedMesh;
            leftHandCollider.sharedMesh = leftSkinnedMeshRenderer.sharedMesh;
        }
	}
}
