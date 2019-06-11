using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NRSComponent : UtilComponent {


    Action<NRSComponent> callbackCollision;

    public GameObject objBullet;
    public GameObject objEffect;

    public Bullet bullet;

    public int num;

    public OVRInput.Controller controller = OVRInput.Controller.Touch;



    public void Init(int num, Action<NRSComponent> callbackCollision, Bullet.CollisionEnum collisionStatus = Bullet.CollisionEnum.ENTER, float stayTime = 0.3f)
    {
        this.callbackCollision = callbackCollision;
        this.num = num;



        bullet.Init(CallbackFromBullet, collisionStatus, stayTime);

        Reset();
    }


    public void ColliderEnabled(bool isEnabled)
    {
        if(bullet != null)
        {
            bullet.ColliderEnabled(isEnabled);
        }
    }


    public void SetActiveBullet(bool isActive)
    {
        SetActive(objBullet, isActive);
    }


    public void Reset()
    {
        SetActive(objBullet, false);
        SetActive(objEffect, false);
        if (bullet != null)
        {
            bullet.Reset();
        }
    }

    void CallbackFromBullet(Collider collider)
    {
        OVRGrabberBothHands bothHands = collider.GetComponent<OVRGrabberBothHands>();
        if (bothHands == null) return;

        this.controller = bothHands.m_controller;
        this.callbackCollision(this);

        SetActive(objBullet, false);
        SetActive(objEffect, true);

    }
}
