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

    System.Action callbackCollision;

    private bool wasAnswering = false;

    bool finishTitleScene = false;


    OVRInput.Controller controller = OVRInput.Controller.RTouch;


    public void SetCanTouchController(OVRInput.Controller controller)
    {
        this.controller = controller;
    }

    public void PlayHaptics(OVRInput.Controller controller)
    {
        if(controller == OVRInput.Controller.RTouch)
        {
            rightHand.HapticsHands();
        }else if(controller == OVRInput.Controller.LTouch)
        {
            leftHand.HapticsHands();
        }
    }


    public void Init(System.Action callbackCanGrab, System.Action callbackGrabbing, Context context)
    {
        this.callbackCanGrab = callbackCanGrab;
        this.callbackGrabbing = callbackGrabbing;
        this.context = context;
    }

    public void Init(System.Action callbackCollision)
    {
        this.callbackCollision = callbackCollision;
    }

    private void Update()
    {



        if (wasAnswering && !context.isAnswering)
        {
            rightHand.isGrabberTriggerEnter = false;
            leftHand.isGrabberTriggerEnter = false;
        }

        bool canGrabbable = false;
        canGrabbable |= rightHand.isGrabbableTriggerEnter && controller == OVRInput.Controller.RTouch;
        canGrabbable |= leftHand.isGrabbableTriggerEnter && controller == OVRInput.Controller.LTouch;

        bool canGrabber = false;

        canGrabber |= rightHand.isGrabberTriggerEnter;
        canGrabber |= leftHand.isGrabberTriggerEnter;


        bool isCollisionStartCube = false;

        isCollisionStartCube |= rightHand.isCollisionEnter && rightHand.collisionObjName == "Cube";
        isCollisionStartCube |= leftHand.isCollisionEnter && leftHand.collisionObjName == "Cube";

        //  タイトルシーンでの処理
        if (isCollisionStartCube && !finishTitleScene)
        {
            //Debug.Log("Collision Start");
            finishTitleScene = true;
            //callbackCollision();
        }

        if (context == null) return;
        if (canGrabber && !context.isAnswering)
        {
            context.isAnswering = true;
            //rightHand.isGrabberTriggerEnter = false;
            //leftHand.isGrabberTriggerEnter = false;
            callbackCanGrab();
        }

        if (canGrabber && canGrabbable)
        {
            if (controller == OVRInput.Controller.RTouch)
            {
                rightHand.isWrapBegin = true;
            }
            else if (controller == OVRInput.Controller.LTouch)
            {
                leftHand.isWrapBegin = true;
            }
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
