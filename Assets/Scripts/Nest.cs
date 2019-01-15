using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Nest : MonoBehaviour {


    [SerializeField] MeshRenderer plateRenderer;


    [NonSerialized] public DEFINE_APP.ANSWER_TYPE_ENUM cubeType;
    [NonSerialized] public int answerIndex;


    public void Init(DEFINE_APP.ANSWER_TYPE_ENUM cubeType, int answerIndex, Material material)
    {
        this.cubeType = cubeType;
        this.answerIndex = answerIndex;
        //plateRenderer.material = material;

        Quaternion r = this.gameObject.transform.rotation;
        this.gameObject.transform.rotation = Quaternion.Euler(-30f, r.eulerAngles.y, r.eulerAngles.z);
    }


}
