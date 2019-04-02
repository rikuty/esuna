using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Nest : UtilComponent {

    [NonSerialized] public DEFINE_APP.ANSWER_TYPE_ENUM answerType;
    [NonSerialized] public int answerIndex;
    [SerializeField] childColliderComponent childColliderComponent;
    [SerializeField] HitEffect hitEffect;
    [SerializeField] Transform trCreateParent;
    [SerializeField] Transform trCreatePos;
    [SerializeField] Transform trCenterAnchor;

    GameObject guide;
    GameObject[] hands;

    public void Init(DEFINE_APP.ANSWER_TYPE_ENUM cubeType, int answerIndex, GameObject guide = null, GameObject[] hands = null)
    {
        this.answerType = cubeType;
        this.answerIndex = answerIndex;
        this.guide = guide;
        this.hands = hands;

        //Quaternion r = this.gameObject.transform.rotation;
        //this.gameObject.transform.rotation = Quaternion.Euler(-30f, r.eulerAngles.y, r.eulerAngles.z);

        childColliderComponent.Init(this);
    }

    public void SetActiveNest(bool active)
    {
        SetActive(guide, active);
        SetActive(hands, active);
        SetActive(this, active);
    }

    public void SetActiveBird()
    {
        HitEffect objEffect = (HitEffect)Instantiate(hitEffect, new Vector3(0,0,0), Quaternion.identity);

        objEffect.transform.parent = trCreateParent;
        objEffect.transform.localPosition = trCreatePos.localPosition;

        objEffect.transform.LookAt(trCenterAnchor);

        SetActive(objEffect, true);
    }
}
