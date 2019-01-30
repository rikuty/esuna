using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Nest : UtilComponent {

    [NonSerialized] public DEFINE_APP.ANSWER_TYPE_ENUM answerType;
    [NonSerialized] public int answerIndex;
    [SerializeField] childColliderComponent childColliderComponent;

    GameObject guide;


    public void Init(DEFINE_APP.ANSWER_TYPE_ENUM cubeType, int answerIndex, GameObject guide = null)
    {
        this.answerType = cubeType;
        this.answerIndex = answerIndex;
        this.guide = guide;

        Quaternion r = this.gameObject.transform.rotation;
        this.gameObject.transform.rotation = Quaternion.Euler(-30f, r.eulerAngles.y, r.eulerAngles.z);

        childColliderComponent.Init(this);
    }

    public void SetActiveNest(bool active)
    {
        SetActive(guide, active);
        SetActive(this, active);
    }

}
