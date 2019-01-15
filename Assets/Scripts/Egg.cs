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


    void OnTriggerEnter(Collider collider)
    {
        Nest plate = collider.GetComponent<Nest>();
        if (plate == null) return;
        if (plate.cubeType == answerType && plate.answerIndex == answerIndex)
        {
            enter = true;
        }
    }
}
