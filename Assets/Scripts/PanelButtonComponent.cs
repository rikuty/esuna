using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PanelButtonComponent : UtilComponent
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject objRightHand;
    [SerializeField] GameObject objLeftHand;

    [SerializeField] Material defaultMaterial;
    [SerializeField] Material changeMaterial;
    [SerializeField] Material changeMaterial2;

    Action callback;
    
    bool isTouch = false;
    float posZbase = 0;
    float posZcheck = 0;

    OVRGrabberBothHands grabbeHandsRight;
    OVRGrabberBothHands grabberHandsLeft;


    private void Start()
    {
        if(objRightHand != null && objLeftHand != null)
        {
            grabbeHandsRight = objRightHand.GetComponent<OVRGrabberBothHands>();
            grabberHandsLeft = objLeftHand.GetComponent<OVRGrabberBothHands>();
        }
    }

    public void Init(Action callback)
    {
        this.callback = callback;
    }


    // Update is called once per frame
    void Update()
    {
        if (isTouch)
        {

            posZcheck = objRightHand.transform.position.z - posZbase;
            if (posZcheck > 0.1f)
            {
                //cubeRenderer.material = changeMaterial2;
                animator.SetTrigger("PushTrigger");
                isTouch = false;
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        animator.SetTrigger("TouchTrigger");
        isTouch = true;

        //とりあえず右手で判定
        posZbase = objRightHand.transform.position.z;
        grabbeHandsRight.HapticsHands();

    }

    void OnCollisionExit(Collision collision)
    {
        animator.SetTrigger("BackStateTrigger");
        isTouch = false;
        posZbase = 0;
        posZcheck = 0;
    }

    void ButtonPushedCallback() {
        //Debug.Log("ButtonPushedCallback");
        this.callback();
    }
}
