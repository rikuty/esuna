using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Nest : UtilComponent {

    [NonSerialized] public DEFINE_APP.ANSWER_TYPE_ENUM answerType;
    [NonSerialized] public int answerIndex;
    [SerializeField] childColliderComponent childColliderComponent;
    [SerializeField] GameObject objEffect;
    [SerializeField] Transform trCreateParent;
    [SerializeField] Transform trCreatePos;

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
        GameObject obj = (GameObject)Instantiate(objEffect, new Vector3(0,0,0), Quaternion.identity);

        obj.transform.parent = trCreateParent;
        obj.transform.localPosition = trCreatePos.localPosition;

        SetActive(objEffect, true);
    }
}
