using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeasureComponent : UtilComponent {


    Action<Collider> callbackCollisionStay;


	public void Init(Action<Collider> callbackCollisionStay)
    {
        this.callbackCollisionStay = callbackCollisionStay;
    }


    void OnCollisionStay(Collision collision)
    {
        //Debug.Log("Stay");

        this.callbackCollisionStay(collision.collider);

    }

    //void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("Enter");

    //    //this.callbackCollisionStay(collision.collider);

    //}

    //void OnCollisionExit(Collision collision)
    //{
    //    Debug.Log("Exit");

    //    //this.callbackCollisionStay(collision.collider);

    //}
}
