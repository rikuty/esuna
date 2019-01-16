using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Egg : MonoBehaviour {


    [SerializeField] MeshRenderer cubeRenderer;

    [SerializeField] GameObject objParticle;

    [SerializeField] GameObject objCube;


    Action<Egg> callback;


    [NonSerialized] public DEFINE_APP.ANSWER_TYPE_ENUM answerType;
    [NonSerialized] public int answerIndex;


    float TIME_STAY = 1f;

    bool enter = false;

    float deltaTime = 0f;

    //bool collisionFromUpper = false;
    //bool collisionFromLower = false;

    public void Init(Action<Egg> callback, DEFINE_APP.ANSWER_TYPE_ENUM cubeType, int answerIndex, Material material)
    {
        this.callback = callback;
        this.answerType = cubeType;
        this.answerIndex = answerIndex;
        cubeRenderer.material = material;

    }

    // Update is called once per frame
    void Update () {
        if (!enter) return;
        deltaTime += Time.deltaTime;
        if(deltaTime> TIME_STAY)
        {
            enter = false;
            //collisionFromUpper = false;
            deltaTime = 0f;

            objParticle.SetActive(true);
            objCube.SetActive(false);

            StartCoroutine("Coroutine");
        
        }

	}


    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(1.0f);

        gameObject.SetActive(false);
        callback(this);
    }


    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("OnCollisionEnter" + collision.gameObject.name + collisionFromUpper.ToString() + collisionFromLower.ToString());

        Nest nest = collision.collider.GetComponent<Nest>();
        if (nest == null) return;
        if (nest.answerType == answerType 
            && nest.answerIndex == answerIndex
            /*&& collisionFromUpper*/)
        {
            enter = true;
        }
        //collisionFromUpper = false;
        //collisionFromLower = true;
    }

    //一旦上から入る判定は入れない
    //private void OnCollisionExit(Collision collision)
    //{
    //    Nest nest = collision.collider.GetComponent<Nest>();
    //    if (nest == null) return;
    //}

    //void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("OnTriggerEnter" + other.gameObject.name + collisionFromUpper.ToString() + collisionFromLower.ToString());

    //    if (other.gameObject.name == "nest" && !collisionFromLower)
    //    {
    //        collisionFromUpper = true;

    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    Debug.Log("OnCollisionExit" + other.gameObject.name + collisionFromUpper.ToString() + collisionFromLower.ToString());

    //    if (other.gameObject.name == "nest")
    //    {
    //        collisionFromUpper = false;
    //        collisionFromLower = false;


    //    }
    //}
}
