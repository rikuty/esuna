using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController: UtilComponent {

    [SerializeField] OVRGrabberBothHands rightHand;
    [SerializeField] OVRGrabberBothHands leftHand;

    [SerializeField] BoxCollider rightHandCollider;
    [SerializeField] BoxCollider leftHandCollider;

    [SerializeField] OvrAvatar ovrAvatar;

    private SkinnedMeshRenderer rightSkinnedMeshRenderer;
    private SkinnedMeshRenderer leftSkinnedMeshRenderer;

    private bool isInit = false;

    bool isCallbacked = false;

    System.Action callbackRelease;
    System.Action callbackGrabbing;

    Context context;

    System.Action callbackCollision;

    private bool wasAnswering = false;

    bool finishTitleScene = false;

    bool unableGrab = false;


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
        }else if(controller == OVRInput.Controller.Touch)
        {
            rightHand.HapticsHands();
            leftHand.HapticsHands();
        }
    }


    public void Init(System.Action callbackRelease, System.Action callbackGrabbing, Context context)
    {
        this.callbackRelease = callbackRelease;
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
            rightHand.isGrabbableTriggerEnter = false;
            leftHand.isGrabbableTriggerEnter = false;
            rightHand.isGrab = false;
            leftHand.isGrab = false;

            rightHandCollider.size = DEFINE_APP.NORMAL_COLLIDER_SIZE;
            leftHandCollider.size = DEFINE_APP.NORMAL_COLLIDER_SIZE;
            rightHand.m_gripTransform.localPosition = DEFINE_APP.NORMAL_GRIP_R;
        }

        bool canGrabbable = false;
        canGrabbable |= rightHand.isGrabbableTriggerEnter && controller == OVRInput.Controller.RTouch;
        canGrabbable |= leftHand.isGrabbableTriggerEnter && controller == OVRInput.Controller.LTouch;
        canGrabbable |= rightHand.isGrabbableTriggerEnter && leftHand.isGrabbableTriggerEnter && controller == OVRInput.Controller.Touch;
        if(rightHand.isGrabbableTriggerEnter && leftHand.isGrabbableTriggerEnter && controller == OVRInput.Controller.Touch)
        {
            Debug.Log("aaa");
        }

        //bool canGrabber = false;

        //canGrabber |= rightHand.isGrabberTriggerEnter;
        //canGrabber |= leftHand.isGrabberTriggerEnter;


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
        if (/*canGrabber &&*/ !context.isAnswering)
        {
            //rightHand.isGrabberTriggerEnter = false;
            //leftHand.isGrabberTriggerEnter = false;
            //callbackRelease();
        }

        if (/*canGrabber &&*/ canGrabbable && !unableGrab)
        {
            context.isAnswering = true;

            rightHandCollider.size = DEFINE_APP.ANSWERING_COLLIDER_SIZE;
            leftHandCollider.size = DEFINE_APP.ANSWERING_COLLIDER_SIZE;


            if (controller == OVRInput.Controller.RTouch)
            {
                rightHand.isGrab = true;
            }
            else if (controller == OVRInput.Controller.LTouch)
            {
                leftHand.isGrab = true;
            }
            else if(controller == OVRInput.Controller.Touch)
            {
                rightHand.isGrab = true;
                rightHand.m_gripTransform.localPosition = DEFINE_APP.ANSWERING_GRIP;

            }
            //rightHand.isGrabbableTriggerEnter = false;
            //leftHand.isGrabbableTriggerEnter = false;
            callbackGrabbing();
        }
        else
        {
            rightHand.isGrab = false;
            leftHand.isGrab = false;
        }

        wasAnswering = context.isAnswering;


        if (canGrabbable && CheckThumbstickDown() && !unableGrab)
        {
            callbackRelease();
            unableGrab = true;
            StartCoroutine(CorouineUnableGrab());

            rightHandCollider.size = DEFINE_APP.NORMAL_COLLIDER_SIZE;
            leftHandCollider.size = DEFINE_APP.NORMAL_COLLIDER_SIZE;
            rightHand.m_gripTransform.localPosition = DEFINE_APP.NORMAL_GRIP_R;

        }
    }


    IEnumerator CorouineUnableGrab()
    {
        yield return new WaitForSeconds(1.0f);
        unableGrab = false;
    }

    // Update is called once per frame
    void LateUpdate () {
        if (!isInit && ovrAvatar.trackedComponents.Count != 0)
        {
            isInit = true;
            //leftSkinnedMeshRenderer = ovrAvatar.trackedComponents["hand_left"].RenderParts[0].mesh;
            //rightSkinnedMeshRenderer = ovrAvatar.trackedComponents["hand_right"].RenderParts[0].mesh;

            //rightHandCollider.sharedMesh = rightSkinnedMeshRenderer.sharedMesh;
            //leftHandCollider.sharedMesh = leftSkinnedMeshRenderer.sharedMesh;
        }
    }


    
}
