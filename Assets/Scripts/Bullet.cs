using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Bullet : UtilComponent {


    Action<Collider> callbackCollision;
    Collider colliderBullet;

    [SerializeField] Animator anim;
    [SerializeField] String strStateName;

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

    public void Init(Action<Collider> callbackCollision, CollisionEnum collisionStatus = CollisionEnum.ENTER, float stayTime = 0.3f)
    {
        this.callbackCollision = callbackCollision;
        this.collisionStatus = collisionStatus;
        this.stayTime = stayTime;
        this.stayDeltaTime = 0f;
        colliderBullet = this.GetComponent<Collider>();
        if (anim != null)
        {
            anim.Play(strStateName, 0, 0f);
        }
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

        if (this.callbackCollision == null) return;
        this.callbackCollision(collider);
    }


    void OnTriggerStay(Collider collider)
    {
        if (collisionStatus != CollisionEnum.STAY) return;
        if (this.callbackCollision == null) return;


        stayDeltaTime += Time.deltaTime;
        if (stayDeltaTime < stayTime) return;


        this.callbackCollision(collider);
    }


    private void OnTriggerExit(Collider other)
    {
        this.stayDeltaTime = 0f;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collisionStatus != CollisionEnum.ENTER) return;
        if (this.callbackCollision == null) return;

        this.callbackCollision(collision.collider);
    }


    void OnCollisionStay(Collision collision)
    {
        if (collisionStatus != CollisionEnum.STAY) return;
        if (this.callbackCollision == null) return;


        stayDeltaTime += Time.deltaTime;
        if (stayDeltaTime < stayTime) return;

        this.callbackCollision(collision.collider);
    }


    private void OnCollisionExit(Collision collision)
    {
        this.stayDeltaTime = 0f;
    }
}
