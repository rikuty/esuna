using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController: UtilComponent {

    [SerializeField] OVRGrabberBothHands rightHand;
    [SerializeField] OVRGrabberBothHands leftHand;

    [SerializeField] MeshCollider rightHandCollider;
    [SerializeField] MeshCollider leftHandCollider;

    [SerializeField] OvrAvatar ovrAvatar;

    private SkinnedMeshRenderer rightSkinnedMeshRenderer;
    private SkinnedMeshRenderer leftSkinnedMeshRenderer;

    private bool isInit = false;

    System.Action callbackCanGrab;
    System.Action callbackGrabbing;

    public void Init(System.Action callbackCanGrab, System.Action callbackGrabbing)
    {
        this.callbackCanGrab = callbackCanGrab;
        this.callbackGrabbing = callbackGrabbing;
    }

    private void Update()
    {
        bool canGrabbable = true;
        canGrabbable &= rightHand.isGrabbableTriggerEnter;
        canGrabbable &= leftHand.isGrabbableTriggerEnter;

        bool canGrabber = true;

        canGrabber &= rightHand.isGrabberTriggerEnter;
        canGrabber &= leftHand.isGrabberTriggerEnter;

        if (canGrabber)
        {
            callbackCanGrab();
        }

        if (canGrabber && canGrabbable)
        {
            rightHand.isWrapBegin = true;
            callbackGrabbing();
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
