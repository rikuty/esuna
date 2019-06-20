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

    public HandController handController;



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


        StartCoroutine("CoroutineInit");
    }


    IEnumerator CoroutineInit()
    {
        yield return new WaitForSeconds(1.5f);
        InitDirection();

    }


    public void Reset()
    {
        for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex]; i++)
        {
            coinComponents[i].Init(currentIndex, (float)(i + 1) / (float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex], Hit);
            coinComponents[i].SetActiveBullet(false);
            coinComponents[i].ColliderEnabled(false);
        }
    }


    public void Reset(int index)
    {
        currentIndex = index;
        this.directionStatus = DirectionEnum.START;

        for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex]; i++)
        {
            coinComponents[i].Init(currentIndex, (float)(i + 1) / (float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex], Hit);
            coinComponents[i].SetActiveBullet(false);
            coinComponents[i].ColliderEnabled(false);
        }

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
        directionStatus = DirectionEnum.STANDBY;

        StartCoroutine("CoroutineInstantiateBullets");

    }


    int count;
    IEnumerator CoroutineInstantiateBullets()
    {
		/*
        Vector3 backRot = DEFINE_APP.BODY_SCALE.GOAL_CURRENT_DIC[currentIndex][DEFINE_APP.BODY_SCALE.BACK_ROT];
        Vector3 shoulderRot = DEFINE_APP.BODY_SCALE.GOAL_CURRENT_DIC[currentIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT];

        int rotateXBack = (int)(Mathf.Abs(backRot.x));
        int inRotateXBack = 360 - rotateXBack;
        int resultXBack = (rotateXBack > inRotateXBack) ? inRotateXBack : rotateXBack;

        int rotateXShoulder = (int)(Mathf.Abs(shoulderRot.x));
        int inRotateXShoulder = 360 - rotateXShoulder;
        int resultXShoulder = (rotateXShoulder > inRotateXShoulder) ? inRotateXShoulder : rotateXShoulder;

        int rotateYBack = (int)(Mathf.Abs(backRot.y));
        int inRotateYBack = 360 - rotateYBack;
        int resultYBack = (rotateYBack > inRotateYBack) ? inRotateYBack : rotateYBack;

        int rotateYShoulder = (int)(Mathf.Abs(shoulderRot.y));
        int inRotateYShoulder = 360 - rotateYShoulder;
        int resultYShoulder = (rotateYShoulder > inRotateYShoulder) ? inRotateYShoulder : rotateYShoulder;

        int rotateZBack = (int)(Mathf.Abs(backRot.z));
        int inRotatezZBack = 360 - rotateZBack;
        int resultZBack = (rotateZBack > inRotatezZBack) ? inRotatezZBack : rotateZBack;

        int rotateZShoulder = (int)(Mathf.Abs(shoulderRot.z));
        int inRotateZShoulder = 360 - rotateZShoulder;
        int resultZShoulder = (rotateZShoulder > inRotateZShoulder) ? inRotateZShoulder : rotateZShoulder;


        Vector3 ROT_MAX_BACK = DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[currentIndex][DEFINE_APP.BODY_SCALE.BACK_ROT];
        Vector3 ROT_MAX_SHOL = DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[currentIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT];

        int rotateXBackMax = (int)(Mathf.Abs(ROT_MAX_BACK.x));
        int inRotateXBackMax = 360 - rotateXBackMax;
        int resultXBackMax = (rotateXBackMax > inRotateXBackMax) ? inRotateXBackMax : rotateXBackMax;

        int rotateXShoulderMax = (int)(Mathf.Abs(ROT_MAX_SHOL.x));
        int inRotateXShoulderMax = 360 - rotateXShoulderMax;
        int resultXShoulderMax = (rotateXShoulderMax > inRotateXShoulderMax) ? inRotateXShoulderMax : rotateXShoulderMax;

        int rotateYBackMax = (int)(Mathf.Abs(ROT_MAX_BACK.y));
        int inRotateYBackMax = 360 - rotateYBackMax;
        int resultYBackMax = (rotateYBackMax > inRotateYBackMax) ? inRotateYBackMax : rotateYBackMax;

        int rotateYShoulderMax = (int)(Mathf.Abs(ROT_MAX_SHOL.y));
        int inRotateYShoulderMax = 360 - rotateYShoulderMax;
        int resultYShoulderMax = (rotateYShoulderMax > inRotateYShoulderMax) ? inRotateYShoulderMax : rotateYShoulderMax;

        int rotateZBackMax = (int)(Mathf.Abs(ROT_MAX_BACK.z));
        int inRotatezZBackMax = 360 - rotateZBackMax;
        int resultZBackMax = (rotateZBackMax > inRotatezZBackMax) ? inRotatezZBackMax : rotateZBackMax;

        int rotateZShoulderMax = (int)(Mathf.Abs(ROT_MAX_SHOL.z));
        int inRotateZShoulderMax = 360 - rotateZShoulderMax;
        int resultZShoulderMax = (rotateZShoulderMax > inRotateZShoulderMax) ? inRotateZShoulderMax : rotateZShoulderMax;


        count =(int)(((float)(resultXBack+resultYBack+resultZBack+resultXShoulder+resultYShoulder+resultZShoulder) / (float)(resultXBackMax + resultYBackMax + resultZBackMax + resultXShoulderMax + resultYShoulderMax + resultZShoulderMax)) * (float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex])-1;
		*/

		float backAngle = Cache.user.BodyScaleData.goalCurrentDic[this.currentIndex][DEFINE_APP.BODY_SCALE.BACK_ROT];
		float shoulderAngle = Cache.user.BodyScaleData.goalCurrentDic[this.currentIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT];

		int rotateBack = (int)Mathf.Abs(backAngle);
		int inRotateBack = 360 - rotateBack;
		int resultBack = (rotateBack > inRotateBack) ? inRotateBack : rotateBack;

		int rotateShoulder = (int)Mathf.Abs(shoulderAngle);
		int inRotateShoulder = 360 - rotateShoulder;
		int resultShoulder = (rotateShoulder > inRotateShoulder) ? inRotateShoulder : rotateShoulder;

		float backAngleMax = DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[this.currentIndex][DEFINE_APP.BODY_SCALE.BACK_ROT];
		float shoulderAngleMax = DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[this.currentIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT];

		int rotateBackMax = (int)Mathf.Abs(backAngleMax);
		int inRotateBackMax = 360 - rotateBackMax;
		int resultBackMax = (rotateBackMax > inRotateBackMax) ? inRotateBackMax : rotateBackMax;

		int rotateShoulderMax = (int)Mathf.Abs(shoulderAngleMax);
		int inRotateShoulderMax = 360 - rotateShoulderMax;
		int resultShoulderMax = (rotateShoulderMax > inRotateShoulderMax) ? inRotateShoulderMax : rotateShoulderMax;

		count = (int)((float)(resultBack + resultShoulder) / (float)(resultBackMax + resultShoulderMax) * (float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[this.currentIndex]) - 1;

		for (int i = 0; i < count-1; i++)
        {
            coinComponents[i].Init(currentIndex, (float)(i+1)/(float)count, Hit);
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
        context.answeringDeltaTime = 0f;
        directionStatus = DirectionEnum.MEASURING;
        context.AddGamePoint();
        callbackHitCoin();

        handController.PlayHaptics(coinComponent.controller);


    }


    void HitStartMeasure(MeasureStartComponent measureComponent)
    {
        handController.PlayHaptics(measureComponent.controller);


        if (DEFINE_APP.HAND_TARGET[currentIndex - 1] == OVRInput.Controller.LTouch || DEFINE_APP.HAND_TARGET[currentIndex - 1] == OVRInput.Controller.RTouch)
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
            }
            answerController.SetActiveCurrentEgg(true);

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
        if (context != null)
        {
            preIsAnswering = context.isAnswering;
        }

    }


}
