using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ButtonPanelComponent : UtilComponent
{
    //[SerializeField] Animator animator;
    [SerializeField] GameObject objRightHand;
    [SerializeField] GameObject objLeftHand;

    GameObject targetHand = null;
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

    // Update is called once per frame
    void Update()
    {
    }
    
    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("OnCollisionEnter");
        OVRGrabberBothHands hand = collision.gameObject.GetComponent<OVRGrabberBothHands>();
        if (hand == null) return;


        grabbeHandsRight.HapticsHands();
        
        if (hand.m_controller == OVRInput.Controller.RTouch)
        {
            targetHand = objRightHand;
        }
        else if (hand.m_controller == OVRInput.Controller.LTouch)
        {
            targetHand = objLeftHand;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, this.gameObject.transform.localPosition.y, 0);
        targetHand = null;
    }
}
