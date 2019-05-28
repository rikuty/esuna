using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class CoinController : UtilComponent {

    /// <summary>
    ///　コインシステムのルート
    /// </summary>
    public GameObject objRootCoinSystem;


    public CoinComponent[] coinComponents;
    public MeasureStartComponent measureStartRightComponent;
    public MeasureStartComponent measureStartLeftComponent;
    public OvrAvatar ovrAvatar;


    private Action callbackHitCoin;

    public AnswerController answerController;



    bool preIsAnswering = false;


    /// <summary>
    /// 回旋・屈曲・伸展の現在測定している方向 DEFINEのDIAGNOSIS_DIRECTのIndex
    /// </summary>
    int currentIndex = 1;

    Context context;


    // 手がスタート地点に入り、OKとなったか
    enum DirectionEnum
    {
        NONE, // Initもされていな状態
        START, // RされてからmeasureComponentの表示処理
        STANDBY, // コライダーのセット
        LEFT, // 両手がスタンバイ位置にくる必要があるときに片方が来ていない
        RIGHT, // 両手がスタンバイ位置にくる必要があるときに片方が来ていない
        PREPARED, // 両手もしくは片手がスタンバイ位置に来ている
        WAITING, // スタート位置に入ったあと、最初の目標物に触れるまで。PREPAREDの1フレーム後になる。
        MEASURING, // コライダーが当たってからボタンが押されるか数秒立つまで
    }
    DirectionEnum directionStatus = DirectionEnum.NONE;


    //　次のBulletに当たるまでの時間をカウント　※一定時間経過すると測定終了
    float hitDeltaTime = 0f;


    public void Init(Action callbackHitCoin, Context context)
    {
        this.context = context;
        this.callbackHitCoin = callbackHitCoin;

        for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex]; i++)
        {
            coinComponents[i].Init(currentIndex, (float)(i + 1) / (float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex], Hit);
            coinComponents[i].SetActiveBullet(false);
            coinComponents[i].ColliderEnabled(false);
        }

        StartCoroutine("CoroutineInit");
    }


    IEnumerator CoroutineInit()
    {
        yield return new WaitForSeconds(2f);
        InitDirection();

    }


    public void Reset(int index)
    {
        currentIndex = index;
        this.directionStatus = DirectionEnum.START;
        PreparingDirection();
    }



    void InitDirection()
    {
        GameObject objLeft = Instantiate(ovrAvatar.trackedComponents["hand_left"].gameObject, measureStartLeftComponent.trRootObj.transform);
        objLeft.transform.localPosition = Vector3.zero;
        objLeft.transform.localRotation = Quaternion.identity;
        SetActive(objLeft.transform.GetChild(0), true);
        //measureStartLeftComponent.SetActiveBullet(false);

        GameObject objRight = Instantiate(ovrAvatar.trackedComponents["hand_right"].gameObject, measureStartRightComponent.trRootObj.transform);
        objRight.transform.localPosition = Vector3.zero;
        objRight.transform.localRotation = Quaternion.identity;
        SetActive(objRight.transform.GetChild(0), true);
        //measureStartRightComponent.SetActiveBullet(false);

    }



    void PreparingDirection()
    {

        StartCoroutine("CoroutineInstantiateBullets");

    }


    int count;
    IEnumerator CoroutineInstantiateBullets()
    {
        Vector3 backRot = DEFINE_APP.BODY_SCALE.GOAL_DIC[currentIndex][DEFINE_APP.BODY_SCALE.BACK_ROT];
        Vector3 shoulderRot = DEFINE_APP.BODY_SCALE.GOAL_DIC[currentIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT];

        int rotateBack = (int)(Mathf.Abs(backRot.x) + Mathf.Abs(backRot.y) + Mathf.Abs(backRot.z));
        int inRotateBack = 360 - rotateBack;
        int result1 = (rotateBack > inRotateBack) ? inRotateBack : rotateBack;

        int rotateShoulder = (int)(Mathf.Abs(shoulderRot.x) + Mathf.Abs(shoulderRot.y) + Mathf.Abs(shoulderRot.z));
        int inRotateShoulder = 360 - rotateShoulder;
        int result2 = (rotateShoulder > inRotateShoulder) ? inRotateShoulder : rotateShoulder;

        count = ((result1+result2) / DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex])-3;

        for (int i = 0; i < count; i++)
        {
            coinComponents[i].Init(currentIndex, (float)(i+1)/(float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex], Hit);
            coinComponents[i].SetActiveBullet(true);
            coinComponents[i].ColliderEnabled(false);

            yield return new WaitForSeconds(0.1f);
        }

        measureStartLeftComponent.Init(currentIndex, OVRInput.Controller.LTouch, HitStartMeasure);
        measureStartRightComponent.Init(currentIndex, OVRInput.Controller.RTouch, HitStartMeasure);

        PreparedDirection();


    }


    void PreparedDirection()
    {
        directionStatus = DirectionEnum.STANDBY;

        OVRInput.Controller result = DEFINE_APP.HAND_TARGET[currentIndex - 1];

        if (result == OVRInput.Controller.RTouch)
        {
            measureStartRightComponent.SetActiveBullet(true);
        }
        else if (result == OVRInput.Controller.LTouch)
        {
            measureStartLeftComponent.SetActiveBullet(true);
        }
        else if (result == OVRInput.Controller.Touch)
        {
            measureStartLeftComponent.SetActiveBullet(true);
            measureStartRightComponent.SetActiveBullet(true);
        }
    }


    void Hit(CoinComponent coinComponent)
    {

        hitDeltaTime = 0f;
        directionStatus = DirectionEnum.MEASURING;
        context.AddGamePoint();
        callbackHitCoin();

    }


    void HitStartMeasure(MeasureStartComponent measureComponent)
    {
        if(DEFINE_APP.HAND_TARGET[currentIndex - 1] == OVRInput.Controller.LTouch || DEFINE_APP.HAND_TARGET[currentIndex - 1] == OVRInput.Controller.RTouch)
        {
            if(DEFINE_APP.HAND_TARGET[currentIndex - 1] == measureComponent.controller)
            {
                directionStatus = DirectionEnum.PREPARED;
            }
        }
        else
        {
            if (directionStatus == DirectionEnum.STANDBY)
            {
                if (measureComponent.controller == OVRInput.Controller.RTouch)
                {
                    directionStatus = DirectionEnum.RIGHT;
                }else if(measureComponent.controller == OVRInput.Controller.LTouch)
                {
                    directionStatus = DirectionEnum.LEFT;
                }
            }
            if ((directionStatus == DirectionEnum.LEFT && measureComponent.controller == OVRInput.Controller.RTouch)
                || (directionStatus == DirectionEnum.RIGHT && measureComponent.controller == OVRInput.Controller.LTouch))
            {
                directionStatus = DirectionEnum.PREPARED;
            }
        }

        if(directionStatus == DirectionEnum.PREPARED)
        {
            for (int i = 0; i < count; i++)
            {
                coinComponents[i].ColliderEnabled(true);
                answerController.SetActiveCurrentEgg(true);
            }
        }
    }

    /// <summary>
    /// 球体表示用
    /// </summary>
    /// <param name="isActive"></param>
    void SetActiveBullets(bool isActive)
    {
        for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex]; i++)
        {
            coinComponents[i].SetActiveBullet(isActive);
        }
    }


     void Update()
    {
        // ボタン押下、最大角度確定処理
        if (preIsAnswering && !context.isAnswering)
        {
            directionStatus = DirectionEnum.NONE;
            SetActiveBullets(false);
        }

        // 測定開始部分
        if (directionStatus == DirectionEnum.PREPARED)
        {
            directionStatus = DirectionEnum.WAITING;
            // 測定器具のコライダーセット
            for (int i = 0; i < count; i++)
            {
                coinComponents[i].ColliderEnabled(true);
            }

        }
        preIsAnswering = context.isAnswering;

    }


}
