using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bullet : UtilComponent {


    Action<Collider> callbackCollisionEnter;


	public void Init(Action<Collider> callbackCollisionEnter)
    {
        this.callbackCollisionEnter = callbackCollisionEnter;

    }


    void OnTriggerEnter(Collider collider)
    {
        OVRGrabberBothHands bothHands = collider.GetComponent<OVRGrabberBothHands>();
        if (bothHands != null)
        {
            this.callbackCollisionEnter(collider);
        }
    }
}
