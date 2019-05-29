﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CoinComponent : UtilComponent {


    Action<CoinComponent> callbackCollision;

    public Transform trBackRoot;
    public Transform trSholderRoot;

    public Transform trArmLength;

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
    /// <param name="directIndex">８方向のどの方向かのIndex</param>
    /// <param name="measureIndex">測定方向のうち、Maxのどれくらいの可動域率かの数値</param>
    /// <param name="collisionStatus"></param>
    /// <param name="stayTime"></param>
    public void Init(int directIndex, float measureRate, Action<CoinComponent> callbackCollision, Bullet.CollisionEnum collisionStatus = Bullet.CollisionEnum.ENTER, float stayTime = 0.3f)
    {
        this.callbackCollision = callbackCollision;
        this.controller = DEFINE_APP.HAND_TARGET[directIndex-1];

        trBackRoot.localPosition = DEFINE_APP.BODY_SCALE.BACK_POS;
        //Debug.Log((DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[directIndex][DEFINE_APP.BODY_SCALE.BACK_ROT] * measureRate).ToString());
        trBackRoot.localRotation = Quaternion.Euler(DEFINE_APP.BODY_SCALE.GOAL_DIC[directIndex][DEFINE_APP.BODY_SCALE.BACK_ROT]*measureRate);
        trSholderRoot.localPosition = DEFINE_APP.SHOULDER_POS_DIC[controller];
        trSholderRoot.localRotation = Quaternion.Euler(DEFINE_APP.BODY_SCALE.GOAL_DIC[directIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT]*measureRate);
        //Debug.Log((DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[directIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT] * measureRate).ToString());        //Debug.Log((DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[directIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT] * measureRate).ToString());
        trArmLength.localRotation = Quaternion.Euler(0f, 0f, DEFINE_APP.BODY_SCALE.SHOULDER_ROT_Z[directIndex]);
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

        Egg egg = collider.GetComponent<Egg>();
        if (egg == null) return;

        this.callbackCollision(this);

        SetActive(objBullet, false);
        SetActive(objEffect, true);
    }
}
