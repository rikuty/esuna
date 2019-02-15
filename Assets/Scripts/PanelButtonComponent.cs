using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PanelButtonComponent : UtilComponent
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject objRightHand;
    [SerializeField] GameObject objLeftHand;
    
    Action callback;

    bool isTouch = false;
    GameObject targetHand = null;
    float posZbase = 0;
    float posZcheck = 0;
    OVRGrabberBothHands grabbeHandsRight;
    OVRGrabberBothHands grabberHandsLeft;


    private void Start()
    {
        if (objRightHand != null && objLeftHand != null)
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
                ResetStatus();
                animator.SetTrigger("PushTrigger");
            }
        }
    }

    
    void OnCollisionEnter(Collision collision)
    {
        grabbeHandsRight.HapticsHands();

        animator.SetTrigger("TouchTrigger");
        isTouch = true;
        //targetHand = collision.collider.gameObject;

        //とりあえず右手で判定
        targetHand = objRightHand;
        posZbase = targetHand.transform.position.z;
    }

    void OnCollisionExit(Collision collision)
    {
        ResetStatus();
        animator.SetTrigger("BackStateTrigger");
    }

    void OnTriggerEnter(Collider collider)
    {
        grabbeHandsRight.HapticsHands();

        animator.SetTrigger("TouchTrigger");
        isTouch = true;
        //targetHand = collision.collider.gameObject;

        //とりあえず右手で判定
        targetHand = objRightHand;
        posZbase = targetHand.transform.position.z;
    }

    void OnTriggerExit(Collider collider)
    {
        ResetStatus();
        animator.SetTrigger("BackStateTrigger");
    }


    void ButtonPushedCallback() {
        //Debug.Log("ButtonPushedCallback");
        this.callback();
        SetActive(this, false);
    }

    void ResetStatus()
    {
        isTouch = false;
        targetHand = null;
        posZbase = 0;
        posZcheck = 0;
    }
}
