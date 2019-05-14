using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeasureComponent : UtilComponent {


    Action<MeasureComponent> callbackCollision;

    public int rot;

    public Transform trArmLength;
    public Transform trRootObj;

    public GameObject objBullet;
    public GameObject objEffect;

    public Bullet bullet;

    public string strhand;

    private bool isRightTouch = false;
    private bool isLeftTouch = false;


    public void Init(Action<MeasureComponent> callbackCollision, int rot, float armLength, string strHand, Bullet.CollisionEnum collisionStatus = Bullet.CollisionEnum.ENTER, float stayTime = 0.3f)
    {
        this.callbackCollision = callbackCollision;
        this.rot = rot;
        this.strhand = strHand;

        this.transform.localRotation = Quaternion.Euler(rot, 0, 0);
        trArmLength.localPosition = new Vector3(0f, 0f, armLength);
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

    void CallbackFromBullet(OVRGrabberBothHands bothHands)
    {
        bool result = false;
        result |= (bothHands.m_controller == OVRInput.Controller.RTouch && this.strhand == "R");
        result |= (bothHands.m_controller == OVRInput.Controller.LTouch && this.strhand == "L");

        if(this.strhand == "C")
        {
            if(bothHands.m_controller == OVRInput.Controller.LTouch)
            {
                isLeftTouch = true;
            }
            else if (bothHands.m_controller == OVRInput.Controller.RTouch)
            {
                isRightTouch = true;
            }

            if(isLeftTouch && isRightTouch)
            {
                result = true;
            }
        }

        if (result)
        {
            this.callbackCollision(this);

            SetActive(objBullet, false);
            SetActive(objEffect, true);
        }

    }
}
