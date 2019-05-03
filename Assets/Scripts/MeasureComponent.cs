using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeasureComponent : UtilComponent {


    Action<Collider> callbackCollisionEnter;

    public int rot;

    public Transform trArmLength;

    public GameObject objBullet;
    public GameObject objEffect;


	public void Init(Action<Collider> callbackCollisionEnter, int rot, float armLength)
    {
        this.callbackCollisionEnter = callbackCollisionEnter;
        this.rot = rot;

        this.transform.localRotation = Quaternion.Euler(rot, 0, 0);
        trArmLength.localPosition = new Vector3(0f, 0f, armLength);
    }


    void OnTriggerEnter(Collider collider)
    {
        //Debug.Log("Stay");

        this.callbackCollisionEnter(collider);

        SetActive(objBullet, false);
        SetActive(objEffect, true);

    }
}
