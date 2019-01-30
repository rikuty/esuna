using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

public class AnswerController : UtilComponent
{

    [SerializeField] Nest startNest;

    [SerializeField] Material[] materials;
    [SerializeField] Nest nest;
    [SerializeField] GameObject[] guides;

    [SerializeField] Transform startEggParent;
    [SerializeField] Transform playEggParent;
    [SerializeField] Transform resultEggParent;

    [SerializeField] Egg currentEgg;
    [SerializeField] Nest resultNest;

    [SerializeField] BodyScale bodyScale;



    int playEggCount = 1;

    int preNum = 0;

    Context context;


    Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback;


    public void Init(Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback, Context context)
    {
        this.callback = callback;
        this.context = context;

    }


    void CallbackFromEggAnswer(Egg egg)
    {
        if (egg.touchNest != null)
        {
            switch (egg.answerType)
            {
                case DEFINE_APP.ANSWER_TYPE_ENUM.START:
                    if(egg.answerIndex == 8)
                    {
                        callback(egg.answerType);
                    }
                    else
                    {
                        InstantiateNewEgg(egg.answerType);
                    }
                    break;
                case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
                    callback(egg.answerType);
                    InstantiateNewEgg(egg.answerType);
                    break;
            }
            return;
        }


        InstantiateNewEgg(egg.answerType,true);

    }


    public void InstantiateNewEgg(DEFINE_APP.ANSWER_TYPE_ENUM answerType, bool replay = false)
    {
        //Transform tr;
        //switch (answerType)
        //{
        //    case DEFINE_APP.ANSWER_TYPE_ENUM.START:
        //        tr = startEggParent;
        //        break;
        //    case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
        //        tr = resultEggParent;
        //        break;
        //    case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
        //    default:
        //        tr = playEggParent;
        //        break;
        //}

        GameObject obj = Instantiate(Resources.Load<GameObject>(DEFINE_PREFAB.EGG), playEggParent);
        obj.SetActive(true);
        currentEgg = obj.GetComponent<Egg>();
        //if(preNum > 0)
        //{
        //    SetActive(guides[preNum-1], false);
        //}

        if (!replay)
        {

            int num;
            switch (answerType)
            {
                case DEFINE_APP.ANSWER_TYPE_ENUM.START:
                case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
                case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
                default:
                    if (playEggCount <= 8)
                    {
                        num = playEggCount;
                    }
                    else
                    {
                        num = UnityEngine.Random.Range(1, 9);
                    }

                    playEggCount++;
                    preNum = num;
                    break;

            }
        }

        
        bodyScale.SetTransform(preNum);
        nest.Init(answerType, preNum, guides[preNum-1]);


        currentEgg.Init(CallbackFromEggAnswer, answerType, preNum);


        context.isAnswering = false;
    }

    public void SetGravity(bool active)
    {
        currentEgg.GetComponent<Rigidbody>().useGravity = active;


    }


    public void SetActiveNest(bool active)
    {
        nest.SetActiveNest(active);
    }
}
