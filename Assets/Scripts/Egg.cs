using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Egg : UtilComponent {


    [SerializeField] MeshRenderer cubeRenderer;

    //[SerializeField] GameObject objParticle;

    [SerializeField] GameObject objCube;

    [SerializeField] AudioSource audioSource;

    Action<Egg> callbackAnswer;


    [NonSerialized] public DEFINE_APP.ANSWER_TYPE_ENUM answerType;
    [NonSerialized] public int answerIndex;
    [NonSerialized] public Nest touchNest;


    float TIME_STAY = 0f;

    bool enter = false;

    float deltaTime = 0f;

    Rigidbody rigidbody;

    //bool collisionFromUpper = false;
    //bool collisionFromLower = false;

    public void Init(Action<Egg> callbackAnswer,DEFINE_APP.ANSWER_TYPE_ENUM cubeType, int answerIndex)
    {
        this.callbackAnswer = callbackAnswer;
        this.answerType = cubeType;
        this.answerIndex = answerIndex;

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

            //objParticle.SetActive(true);
            objCube.SetActive(false);
            if (touchNest != null)
            {
                // エフェクト生成
                touchNest.SetActiveBird();

                touchNest.SetActiveNest(false);
                audioSource.Play();

            }
            StartCoroutine("Coroutine");
        
        }

	}


    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(1.0f);

        gameObject.SetActive(false);
        callbackAnswer(this);
        Destroy(this.gameObject);
    }


    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("OnCollisionEnter" + collision.gameObject.name);
        childColliderComponent childColliderComponent = collision.collider.GetComponent<childColliderComponent>();
        
        if (childColliderComponent != null && childColliderComponent.nest != null)
        {
            //if (nest.answerType == answerType
            //    && nest.answerIndex == answerIndex
            //    /*&& collisionFromUpper*/)
            //{
            touchNest = childColliderComponent.nest;
            enter = true;
            //}
        }

        Terrain terrain = collision.collider.GetComponent<Terrain>();
        if(terrain != null)
        {
            enter = true;
            deltaTime = 0f;
            touchNest = null;
        }
        
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        //Debug.Log("OnCollisionEnter" + collision.gameObject.name);
        childColliderComponent childColliderComponent = otherCollider.GetComponent<childColliderComponent>();

        if (childColliderComponent != null && childColliderComponent.nest != null)
        {
            //if (nest.answerType == answerType
            //    && nest.answerIndex == answerIndex
            //    /*&& collisionFromUpper*/)
            //{
            touchNest = childColliderComponent.nest;
            enter = true;
            //}
        }

        Terrain terrain = otherCollider.GetComponent<Terrain>();
        if (terrain != null)
        {
            enter = true;
            deltaTime = 0f;
            touchNest = null;
        }


    }


    private void OnCollisionExit(Collision collision)
    {
        enter = false;
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
