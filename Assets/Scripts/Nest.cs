using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Nest : MonoBehaviour {

    [NonSerialized] public DEFINE_APP.ANSWER_TYPE_ENUM answerType;
    [NonSerialized] public int answerIndex;


    public void Init(DEFINE_APP.ANSWER_TYPE_ENUM cubeType, int answerIndex, Material material)
    {
        this.answerType = cubeType;
        this.answerIndex = answerIndex;
        //plateRenderer.material = material;

        Quaternion r = this.gameObject.transform.rotation;
        this.gameObject.transform.parent.rotation = Quaternion.Euler(-30f, r.eulerAngles.y, r.eulerAngles.z);
    }


}
