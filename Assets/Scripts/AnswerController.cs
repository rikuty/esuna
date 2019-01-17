using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

public class AnswerController : UtilComponent
{

    [FormerlySerializedAs("startCube")]
    [SerializeField] Egg startEgg;

    [FormerlySerializedAs("startPlate")]
    [SerializeField] Nest startNest;

    [SerializeField] Material[] materials;
    [FormerlySerializedAs("plates")]
    [SerializeField] Nest[] nests;

    [SerializeField] Transform cubeParent;

    [SerializeField] GameObject objOriginal;
    [FormerlySerializedAs("resultCube")]
    [SerializeField]
    Egg resultEgg;
    [FormerlySerializedAs("resultPlate")]
    [SerializeField]
    Nest resultNest;

    int playCubeCount = 0;

    int preNum = 0;


    Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback;


    public void Init(Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback)
    {
        this.callback = callback;

        startEgg.Init(CallbackFromCube, DEFINE_APP.ANSWER_TYPE_ENUM.START, -1);
        startNest.Init(DEFINE_APP.ANSWER_TYPE_ENUM.START, -1);
        for (int i = 0; i < nests.Length; i++)
        {
            nests[i].Init(DEFINE_APP.ANSWER_TYPE_ENUM.PLAY, i);
            SetActive(nests[i].gameObject, false);
        }

        resultEgg.Init(CallbackFromCube, DEFINE_APP.ANSWER_TYPE_ENUM.RESULT, -1);
        resultNest.Init(DEFINE_APP.ANSWER_TYPE_ENUM.RESULT, -1);
    }


    void CallbackFromCube(Egg cube)
    {
        callback(cube.answerType);

        switch (cube.answerType)
        {
            case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
                InstantiateNewCube();
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.START:
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
                break;
        }

    }


    public void InstantiateNewCube()
    {
        GameObject obj = Instantiate(objOriginal, cubeParent);
        //obj.transform.localPosition = Vector3.zero;
        obj.SetActive(true);
        Egg cube = obj.GetComponent<Egg>();
        int num;
        //Debug.Log(playCubeCount.ToString());
        if (playCubeCount < 8)
        {
            num = playCubeCount;
        }
        else
        {
            num = UnityEngine.Random.Range(0, materials.Length);
        }
        cube.Init(CallbackFromCube, DEFINE_APP.ANSWER_TYPE_ENUM.PLAY, num);
        SetActive(nests[preNum].gameObject, false);
        //SetActive(nests[num].gameObject, true);
        playCubeCount++;
        preNum = num;
    }

    public void SetGravity(DEFINE_APP.STATUS_ENUM status)
    {
        switch (status)
        {
            case DEFINE_APP.STATUS_ENUM.START:
                startEgg.GetComponent<Rigidbody>().useGravity = true;
                break;
            case DEFINE_APP.STATUS_ENUM.SHOW_RESLUT:
                resultEgg.GetComponent<Rigidbody>().useGravity = true;
                break;
        }

    }


    public void SetActiveNest(bool active)
    {
        SetActive(nests[preNum], active);
    }
}
