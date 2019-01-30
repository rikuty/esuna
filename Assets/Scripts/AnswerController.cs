using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

public class AnswerController : UtilComponent
{

    [SerializeField] Nest startNest;

    [SerializeField] Material[] materials;
    [SerializeField] Nest[] nests;
    [SerializeField] GameObject[] guides;

    [SerializeField] Transform startEggParent;
    [SerializeField] Transform playEggParent;
    [SerializeField] Transform resultEggParent;

    [SerializeField]
    Egg currentEgg;
    [SerializeField]
    Nest resultNest;

    int playEggCount = 0;

    int preNum = 0;

    Context context;


    Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback;


    public void Init(Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback, Context context)
    {
        this.callback = callback;
        this.context = context;

        startNest.Init(DEFINE_APP.ANSWER_TYPE_ENUM.START, -1);
        for (int i = 0; i < nests.Length; i++)
        {
            nests[i].Init(DEFINE_APP.ANSWER_TYPE_ENUM.PLAY, i, guides[i]);
            nests[i].SetActiveNest(false);
        }

        resultNest.Init(DEFINE_APP.ANSWER_TYPE_ENUM.RESULT, -1);
    }


    void CallbackFromEggAnswer(Egg egg)
    {
        if (egg.touchNest != null
            && egg.touchNest.answerType == egg.answerType
            && egg.touchNest.answerIndex == egg.answerIndex)
        {
            callback(egg.answerType);
            switch (egg.answerType)
            {
                case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
                    InstantiateNewEgg(egg.answerType);
                    break;
            }
            return;
        }


        InstantiateNewEgg(egg.answerType);

    }


    public void InstantiateNewEgg(DEFINE_APP.ANSWER_TYPE_ENUM answerType)
    {
        Transform tr;
        switch (answerType)
        {
            case DEFINE_APP.ANSWER_TYPE_ENUM.START:
                tr = startEggParent;
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
                tr = resultEggParent;
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
            default:
                tr = playEggParent;
                break;
        }

        GameObject obj = Instantiate(Resources.Load<GameObject>(DEFINE_PREFAB.EGG), tr);

        //obj.transform.localPosition = Vector3.zero;
        obj.SetActive(true);
        currentEgg = obj.GetComponent<Egg>();
        int num;
        switch (answerType)
        {
            case DEFINE_APP.ANSWER_TYPE_ENUM.START:
            case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
                num = -1;
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
            default:
                if (playEggCount < 8)
                {
                    num = playEggCount;
                }
                else
                {
                    num = UnityEngine.Random.Range(0, materials.Length);
                }
                //SetActive(nests[preNum].gameObject, false);
                playEggCount++;
                preNum = num;
                break;

        }

        currentEgg.Init(CallbackFromEggAnswer, answerType, num);

        context.isAnswering = false;
    }

    public void SetGravity(DEFINE_APP.STATUS_ENUM status)
    {
        currentEgg.GetComponent<Rigidbody>().useGravity = true;


    }


    public void SetActiveNest(bool active)
    {
        nests[preNum].SetActiveNest(active);
    }
}
