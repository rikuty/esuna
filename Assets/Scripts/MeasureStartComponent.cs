using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeasureStartComponent : UtilComponent {


    Action<MeasureStartComponent> callbackCollision;

    public Transform trBackRoot;

    public Transform trSholderRoot;

    public Transform trArmLength;

    public Transform trRootObj;

    public GameObject objBullet;
    public GameObject objEffect;

    public Bullet bullet;

    public OVRInput.Controller controller;

    private bool isRightTouch = false;
    private bool isLeftTouch = false;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="callbackCollision">手がヒットした時のコールバック</param>
    /// <param name="collisionStatus"></param>
    /// <param name="stayTime"></param>
    public void Init(int directIndex, OVRInput.Controller controller, Action<MeasureStartComponent> callbackCollision, Bullet.CollisionEnum collisionStatus = Bullet.CollisionEnum.ENTER, float stayTime = 0.3f)
    {
        this.callbackCollision = callbackCollision;
        this.controller = controller;


        trBackRoot.localPosition = DEFINE_APP.BODY_SCALE.BACK_POS;
        trSholderRoot.localPosition = DEFINE_APP.SHOULDER_POS_DIC[controller];
        trSholderRoot.localRotation = Quaternion.Euler(0f, 0f, DEFINE_APP.BODY_SCALE.SHOULDER_ROT_Z[directIndex]);
        trArmLength.localPosition = DEFINE_APP.HAND_POS_DIC[controller];


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

        bool result = false;
        result |= (bothHands.m_controller == OVRInput.Controller.RTouch && controller == OVRInput.Controller.RTouch);
        result |= (bothHands.m_controller == OVRInput.Controller.LTouch && controller == OVRInput.Controller.LTouch);

        if(controller == OVRInput.Controller.Touch)
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
