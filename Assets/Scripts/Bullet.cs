using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bullet : UtilComponent {


    Action<OVRGrabberBothHands> callbackCollision;
    Collider colliderBullet;

    public enum CollisionEnum
    {
        ENTER,
        STAY
    }
    CollisionEnum collisionStatus = CollisionEnum.ENTER;

    float stayTime;
    float stayDeltaTime = 0f;

    private void Start()
    {
    }

    public void Init(Action<OVRGrabberBothHands> callbackCollision, CollisionEnum collisionStatus = CollisionEnum.ENTER, float stayTime = 0.3f)
    {
        this.callbackCollision = callbackCollision;
        this.collisionStatus = collisionStatus;
        this.stayTime = stayTime;
        this.stayDeltaTime = 0f;
        colliderBullet = this.GetComponent<Collider>();
    }

    public void Reset()
    {
        this.stayDeltaTime = 0f;
    }

    public void ColliderEnabled(bool isEnabled)
    {
        colliderBullet.enabled = isEnabled;
    }


    void OnTriggerEnter(Collider collider)
    {
        if (collisionStatus != CollisionEnum.ENTER) return;


        OVRGrabberBothHands bothHands = collider.GetComponent<OVRGrabberBothHands>();
        if (bothHands != null)
        {
            this.callbackCollision(bothHands);
        }
    }


    void OnTriggerStay(Collider collider)
    {
        if (collisionStatus != CollisionEnum.STAY) return;


        stayDeltaTime += Time.deltaTime;
        if (stayDeltaTime < stayTime) return;

        OVRGrabberBothHands bothHands = collider.GetComponent<OVRGrabberBothHands>();
        if (bothHands != null)
        {
            this.callbackCollision(bothHands);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        this.stayDeltaTime = 0f;
    }
}
