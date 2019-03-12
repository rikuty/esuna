using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ButtonPanelComponent : UtilComponent
{
    //[SerializeField] Animator animator;
    [SerializeField] GameObject objRightHand;
    [SerializeField] GameObject objLeftHand;
    [SerializeField] GameObject objBackPanel;

    Action callback;
    
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

    public void Init(Action callback)
    {
        this.callback = callback;
    }
    

    // Update is called once per frame
    void Update()
    {
    }

    
    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("OnCollisionEnter");

        if (collision.gameObject.name == objBackPanel.name)
        {
            this.callback();
            SetActive(this, false);
        } else {
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
    }

    void OnCollisionExit(Collision collision)
    {
        ResetStatus();
    }

    void ButtonPushedCallback() {
        this.callback();
        SetActive(this, false);
    }

    void ResetStatus()
    {
        targetHand = null;
    }
}
