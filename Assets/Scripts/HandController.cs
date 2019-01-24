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

    bool isCallbacked = false;

    System.Action callbackCanGrab;
    System.Action callbackGrabbing;

    Context context;

    private bool wasAnswering = false;

    public void Init(System.Action callbackCanGrab, System.Action callbackGrabbing, Context context)
    {
        this.callbackCanGrab = callbackCanGrab;
        this.callbackGrabbing = callbackGrabbing;
        this.context = context;
    }

    private void Update()
    {
        if (context.currentStatus == DEFINE_APP.STATUS_ENUM.PREPARE) return;

        if (wasAnswering && !context.isAnswering)
        {
            rightHand.isGrabberTriggerEnter = false;
            leftHand.isGrabberTriggerEnter = false;
        }

        bool canGrabbable = true;
        canGrabbable &= rightHand.isGrabbableTriggerEnter;
        canGrabbable &= leftHand.isGrabbableTriggerEnter;

        bool canGrabber = true;

        canGrabber &= rightHand.isGrabberTriggerEnter;
        canGrabber &= leftHand.isGrabberTriggerEnter;



        if (canGrabber && !context.isAnswering)
        {
            context.isAnswering = true;
            //rightHand.isGrabberTriggerEnter = false;
            //leftHand.isGrabberTriggerEnter = false;
            callbackCanGrab();
        }

        if (canGrabber && canGrabbable)
        {
            rightHand.isWrapBegin = true;
            //rightHand.isGrabbableTriggerEnter = false;
            //leftHand.isGrabbableTriggerEnter = false;
            callbackGrabbing();
        }
        else
        {
            rightHand.isWrapBegin = false;
            leftHand.isWrapBegin = false;
        }

        wasAnswering = context.isAnswering;
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
