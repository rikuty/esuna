using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeasureComponent : UtilComponent {


    Action<MeasureComponent> callbackCollisionEnter;

    public int rot;

    public Transform trArmLength;

    public GameObject objBullet;
    public GameObject objEffect;

    public Bullet bullet;


    public void Init(Action<MeasureComponent> callbackCollisionEnter, int rot, float armLength)
    {
        this.callbackCollisionEnter = callbackCollisionEnter;
        this.rot = rot;

        this.transform.localRotation = Quaternion.Euler(rot, 0, 0);
        trArmLength.localPosition = new Vector3(0f, 0f, armLength);
        this.bullet.Init(CallbackFromBullet);
    }


    void CallbackFromBullet(Collider collider)
    {
        //Debug.Log("Stay");

        this.callbackCollisionEnter(this);

        SetActive(objBullet, false);
        SetActive(objEffect, true);

    }
}
