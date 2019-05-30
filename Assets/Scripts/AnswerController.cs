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
    [SerializeField] GameObject rightHand;
    [SerializeField] GameObject leftHand;

    [SerializeField] Transform startEggParent;
    [SerializeField] Transform playEggParent;
    [SerializeField] Transform resultEggParent;

    [SerializeField] Egg currentEgg;

    [SerializeField] BodyScale bodyScale;

    HandController handController;

    [SerializeField] CoinController coinController;


    List<int> tutorialTargets = new List<int> { 1, 5, 7 };


    int playEggCount = 1;

    public int targetNumber = 0;

    Context context;


    Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback;


    public void Init(Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback, Context context, HandController handController)
    {
        this.callback = callback;
        this.context = context;
        this.handController = handController;
        bodyScale.SetTransformBodyAndBullet();
        coinController.Init(CallbackHitCoin, context);
    }



    private void CallbackHitCoin()
    {

    }


    public void ResetCoinController()
    {
        coinController.Reset();

    }


    void CallbackFromEggAnswer(Egg egg)
    {
        //再度やらないのでコメントアウト
        //if (egg.touchNest != null)
        //{
            switch (egg.answerType)
            {
                case DEFINE_APP.ANSWER_TYPE_ENUM.TUTORIAL:
                    if(tutorialTargets.Count == 0)
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
        //    return;
        //}

        OVRInput.Controller result = DEFINE_APP.HAND_TARGET[targetNumber - 1];

        if (result == OVRInput.Controller.RTouch)
        {
            handController.PlayHaptics(result);

        }
        else if (result == OVRInput.Controller.LTouch)
        {
            handController.PlayHaptics(result);
        }
        else if (result == OVRInput.Controller.Touch)
        {
            handController.PlayHaptics(OVRInput.Controller.Touch);
        }


        //InstantiateNewEgg(egg.answerType,true);

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
        SetActive(obj, replay);

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
                case DEFINE_APP.ANSWER_TYPE_ENUM.TUTORIAL:
                    num = tutorialTargets[0];
                    tutorialTargets.Remove(num);
                    break;
                case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
                case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
                default:
                    num = SetNextAnswers();
                    break;

            }
            playEggCount++;
            targetNumber = num;
        }

        
        bodyScale.SetTransformTarget(targetNumber);
        bodyScale.SetDisplay(targetNumber);
        if (!replay)
        {
            coinController.Reset(targetNumber);
        }
        OVRInput.Controller result = DEFINE_APP.HAND_TARGET[targetNumber - 1];

        GameObject[] objs = new GameObject[2];
        if (result == OVRInput.Controller.RTouch)
        {
            objs[0] = rightHand;
        }else if (result == OVRInput.Controller.LTouch)
        {
            objs[1] = leftHand;
        }else if (result == OVRInput.Controller.Touch)
        {
            objs[0] = rightHand;
            objs[1] = leftHand;
        }

        nest.Init(answerType, targetNumber, guides[targetNumber - 1], objs);

        SetActiveNest(true);

        if (result == OVRInput.Controller.RTouch)
        {
            handController.SetCanTouchController(result);
            handController.PlayHaptics(result);

        }
        else if (result == OVRInput.Controller.LTouch)
        {
            handController.SetCanTouchController(result);
            handController.PlayHaptics(result);
        }
        else if (result == OVRInput.Controller.Touch)
        {
            handController.SetCanTouchController(result);
            handController.PlayHaptics(OVRInput.Controller.LTouch);
            handController.PlayHaptics(OVRInput.Controller.RTouch);
        }


        currentEgg.Init(CallbackFromEggAnswer, answerType, targetNumber);


        context.isAnswering = false;
    }


    int[] randoms = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
    public int SetNextAnswers()
    {
        int currentAnswer = 1;

        bool plus = false;
        foreach (int value in this.randoms)
        {
            if (value != -1)
            {
                plus = true;
            }
        }
        if (!plus)
        {
            randoms = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        }

        bool hit = false;
        while (hit == false)
        {
            int ran = UnityEngine.Random.Range(1, 9);
            foreach (int answer in this.randoms)
            {
                if (answer == ran)
                {
                    hit = true;
                }
            }
            if (hit)
            {
                randoms[ran - 1] = -1;
                currentAnswer = ran;
            }
        }

        return currentAnswer;
    }


    public void SetGravity(bool active)
    {
        currentEgg.GetComponent<Rigidbody>().useGravity = active;


    }


    public void SetTransformTarget()
    {
        bodyScale.SetCloseTarget(targetNumber);
    }


    public void SetActiveNest(bool active)
    {
        foreach(GameObject obj in guides)
        {
            SetActive(obj, false);
        }
        SetActive(leftHand, false);
        SetActive(rightHand, false);

        nest.SetActiveNest(active);
    }


    public void SetActiveCurrentEgg(bool isActive)
    {
        if (currentEgg != null)
        {
            SetActive(currentEgg.gameObject, isActive);

        }
    }
}