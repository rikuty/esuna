using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MeasureController : UtilComponent {

    /// <summary>
    /// 座っている位置のTransform
    /// </summary>
    public Transform playerBaseTr;
    /// <summary>
    /// 腰の位置のTransform
    /// </summary>
    public Transform backTr;
    /// <summary>
    ///　肩の高さのTransform
    /// </summary>
    public Transform shoulderTr;
    /// <summary>
    ///　測定器具の測定方向への設置用のTransform。測定スタート地点の手などもあるので複数。
    /// </summary>
    public Transform[] directRotateTrs;
    /// <summary>
    /// 眼の位置のTransform
    /// </summary>
    public Transform centerEyeTr;
    /// <summary>
    ///　右腕の位置のTransform
    /// </summary>
    public Transform rightHandTr;
    /// <summary>
    /// 左腕の位置のTransform
    /// </summary>
    public Transform leftHandTr;
    /// <summary>
    /// Bullet親のTransform
    /// </summary>
    //public Transform objBulletRoot;


    public GameObject objUI;
    public Text txtTitle;
    public Text txtDetail;
    public Text txtRotateTitle;
    public Text txtRotateDetail;

    //public GameObject objBulletOriginal;

    public Collider rightHandCollider;
    public Collider leftHandCollider;

    public MeasureComponent[] measureComponents;
    public MeasureComponent measureStartRightComponent;
    public MeasureComponent measureStartLeftComponent;
    public OvrAvatar ovrAvatar;


    enum DIAGNOSIS_STATUS_ENUM
    {
        PREPARE, //初期状態
        BASE, //初期位置設定
        SHOULDER_ARM, //肩の高さ&腕の長さ設定
        DIRECT, //８方向
        FINISH, //測定終了
        END //  測定終了待ち
    }

    DIAGNOSIS_STATUS_ENUM currentStatus = DIAGNOSIS_STATUS_ENUM.PREPARE;

    /// <summary>
    /// 回旋・屈曲・伸展の現在測定している方向
    /// </summary>
    int currentRotateNumber = 1;

    Dictionary<DIAGNOSIS_STATUS_ENUM, string> statusTextTitle = new Dictionary<DIAGNOSIS_STATUS_ENUM, string>
   {
       { DIAGNOSIS_STATUS_ENUM.PREPARE, "準備中です" },
       {DIAGNOSIS_STATUS_ENUM.BASE,"身体の情報を取り込みます" },
       { DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM, "肩の高さ&腕長さ測定" },
       { DIAGNOSIS_STATUS_ENUM.DIRECT, "さあ身体を動かしてみましょう！" },
       { DIAGNOSIS_STATUS_ENUM.FINISH, "情報を取り込みました！" }
   };

    Dictionary<DIAGNOSIS_STATUS_ENUM, string> statusTextDetail = new Dictionary<DIAGNOSIS_STATUS_ENUM, string>
   {
       { DIAGNOSIS_STATUS_ENUM.PREPARE, "リラックスしてお待ちください。" },
       { DIAGNOSIS_STATUS_ENUM.BASE, "椅子の背もたれにもたれない状態で背中を伸ばし、前を真っすぐ見て、ボタンをどれか押してください。" },
       { DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM, "背中を伸ばした状態のまま、両腕を前方に真っすぐ伸ばし、ボタンをどれか押してください。" },
       { DIAGNOSIS_STATUS_ENUM.DIRECT, "" },
       { DIAGNOSIS_STATUS_ENUM.FINISH, "測定が終了しました。ボタンをどれか押してください。" }
   };


    Dictionary<int, string> rotateNumberTitle = new Dictionary<int, string>
   {
       { 1, "体を左側に回す" },
       { 2, "体を右側に回す" },
       { 3, "体を左側に回して後ろに反る"},
       { 4, "体を後ろに反る" },
       { 5, "体を右に回して後ろに反る"},
       { 6, "体を左に回して前に倒す" },
       { 7, "体を前に倒す" },
       { 8, "体を右に回して前に倒す" },
   };


    Dictionary<int, string> rotateNumberDetail = new Dictionary<int, string>
   {
       { 1, "右腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください"},
       { 2, "左腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください" },
       { 3, "右腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください" },
       { 4, "両腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください" },
       { 5, "左腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください" },
       { 6, "右腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください" },
       { 7, "両腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください" },
       { 8, "左腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください" }
   };


    Action callbackFinish;

    bool isWaitingStartDiagnosis = false;


    // 手がスタート地点に入り、OKとなったか
    enum DirectionEnum
    {
        NONE, // 両手もしくは片手がスタンバイ位置に来ていない
        STANDBY, // Rotateなどのセットアップ
        PREPARNG, // 手のコライダーセット後、両手がスタンバイ位置に来るまで
        LEFT,　// 両手がスタンバイ位置にくる必要があるときに片方が来ていない
        RIGHT,　// 両手がスタンバイ位置にくる必要があるときに片方が来ていない
        PREPARED,　// 両手もしくは片手がスタンバイ位置に来ている
        WAITING, // 測定中。PREPAREDの1フレーム後になる。
        MEASURING // コライダーが当たってからボタンが押されるか数秒立つまで
    }
    DirectionEnum directionStatus = DirectionEnum.NONE;

    // 測定方向最大角度　※方向が変わるごとにリセット
    float maxAngle;

    //　次のBulletに当たるまでの時間をカウント　※一定時間経過すると測定終了
    float hitDeltaTime = 0f;

    OVRInput.Controller controller;


	// Use this for initialization
	void Start () {
        SetActive(objUI, true);
	}


    public void Init(Action callbackFinish)
    {
        this.callbackFinish = callbackFinish;
        currentStatus = DIAGNOSIS_STATUS_ENUM.PREPARE;
        isWaitingStartDiagnosis = true;

        //SetActive(backTr, false);

    }


    public void StartDiagnosis()
    {
        ShowUI(true);
        isWaitingStartDiagnosis = true;
        StartCoroutine(CoroutineWaitNextStep());
        currentStatus = DIAGNOSIS_STATUS_ENUM.BASE;

        // ここからはTriggerを使う。
        rightHandTr.GetComponent<MeshCollider>().isTrigger = true;
        leftHandCollider.GetComponent<MeshCollider>().isTrigger = true;

    }




    // Update is called once per frame
    void Update () {
        if (isWaitingStartDiagnosis) return;


        switch (currentStatus)
        {
            case DIAGNOSIS_STATUS_ENUM.BASE:
                UpdateBase();
                break;
            case DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM:
                UpdateShoulderArm();
                break;
            case DIAGNOSIS_STATUS_ENUM.DIRECT:
                UpdateDirection();
                break;
            case DIAGNOSIS_STATUS_ENUM.FINISH:
                UpdateFinish();
                break;
        }
	}

    
    void UpdateBase()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.Any))
        {
            isWaitingStartDiagnosis = true;
            currentStatus = DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM;

            DEFINE_APP.BODY_SCALE.PLAYER_BASE_POS = new Vector3(centerEyeTr.position.x, playerBaseTr.position.y, centerEyeTr.position.z);
            playerBaseTr.position = DEFINE_APP.BODY_SCALE.PLAYER_BASE_POS;
            DEFINE_APP.BODY_SCALE.PLAYER_BASE_ROT = new Vector3(playerBaseTr.rotation.eulerAngles.x, centerEyeTr.rotation.eulerAngles.y, playerBaseTr.rotation.eulerAngles.z);
            playerBaseTr.rotation = Quaternion.Euler(DEFINE_APP.BODY_SCALE.PLAYER_BASE_ROT);
            DEFINE_APP.BODY_SCALE.HEAD_POS = centerEyeTr.position - DEFINE_APP.BODY_SCALE.PLAYER_BASE_POS;
            backTr.localPosition = DEFINE_APP.BODY_SCALE.BACK_POS;

            ShowUI(false);

            StartCoroutine(CoroutineWaitNextStep());
        }
    }


    void UpdateShoulderArm()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.Any))
        {
            isWaitingStartDiagnosis = true;
            currentStatus = DIAGNOSIS_STATUS_ENUM.DIRECT;

            Vector3 averagePos = new Vector3(((rightHandTr.position.x + leftHandTr.position.x) / 2f), ((rightHandTr.position.y + leftHandTr.position.y) / 2f), ((rightHandTr.position.z + leftHandTr.position.z) / 2f));
            DEFINE_APP.BODY_SCALE.HAND_POS_R = playerBaseTr.InverseTransformPoint(rightHandTr.position);
            DEFINE_APP.BODY_SCALE.HAND_POS_L = playerBaseTr.InverseTransformPoint(leftHandTr.position);

            shoulderTr.localPosition = DEFINE_APP.BODY_SCALE.SHOULDER_POS_C;
            //handTr.position = DEFINE_APP.BODY_SCALE.ARM_POS;
        

            ShowUI(false);

            StartCoroutine(CoroutineWaitNextStep(InitDirection));


        }
    }


    void InitDirection()
    {
        GameObject objLeft = Instantiate(ovrAvatar.trackedComponents["hand_left"].gameObject, measureStartLeftComponent.trRootObj.transform);
        objLeft.transform.localPosition = Vector3.zero;
        objLeft.transform.localRotation = Quaternion.identity;
        //SetActive(objLeft, false);
        GameObject objRight = Instantiate(ovrAvatar.trackedComponents["hand_right"].gameObject, measureStartRightComponent.trRootObj.transform);
        objRight.transform.localPosition = Vector3.zero;
        objRight.transform.localRotation = Quaternion.identity;
        //SetActive(objRight, false);

        PreparingDirection();
    }



    void PreparingDirection()
    {
        for (int i = 0; i < directRotateTrs.Length; i++)
        {
            directRotateTrs[i].localRotation = Quaternion.Euler(new Vector3(directRotateTrs[i].localRotation.x, directRotateTrs[i].localRotation.y, DEFINE_APP.BODY_SCALE.SHOULDER_ROT_Z[currentRotateNumber]));
        }

        StartCoroutine(CoroutineInstantiateBullets(PreparedDirection));

    }


    IEnumerator CoroutineInstantiateBullets(Action callback = null)
    {
        for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_ANCHOR.Length; i++)
        {
            yield return new WaitForSeconds(0.1f);

            measureComponents[i].Init(Hit, DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_ANCHOR[i], (DEFINE_APP.BODY_SCALE.HAND_POS_R.z + DEFINE_APP.BODY_SCALE.HAND_POS_L.z)/2, DEFINE_APP.HAND_TARGET[currentRotateNumber - 1]);
            measureComponents[i].SetActiveBullet(true);
            measureComponents[i].ColliderEnabled(false);
        }
        PreparedDirection();

        measureStartLeftComponent.Init(HitStartMeasure, 0, DEFINE_APP.BODY_SCALE.HAND_POS_L.z, "L", Bullet.CollisionEnum.STAY, 0.3f);

        measureStartRightComponent.Init(HitStartMeasure, 0, DEFINE_APP.BODY_SCALE.HAND_POS_R.z, "R", Bullet.CollisionEnum.STAY, 0.3f);

    }


    void PreparedDirection()
    {
        directionStatus = DirectionEnum.STANDBY;
    }


    void Hit(MeasureComponent measureComponent)
    {

        hitDeltaTime = 0f;
        directionStatus = DirectionEnum.MEASURING;
        DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber - 1] = (float)measureComponent.rot;

    }


    void HitStartMeasure(MeasureComponent measureComponent)
    {
        if(DEFINE_APP.HAND_TARGET[currentRotateNumber - 1] == "L" || DEFINE_APP.HAND_TARGET[currentRotateNumber - 1] == "R")
        {
            if(DEFINE_APP.HAND_TARGET[currentRotateNumber - 1] == measureComponent.strhand)
            {
                directionStatus = DirectionEnum.PREPARED;
            }
        }
        else
        {
            if (directionStatus == DirectionEnum.PREPARNG)
            {
                if (measureComponent.strhand == "R")
                {
                    directionStatus = DirectionEnum.RIGHT;
                }else if(measureComponent.strhand == "L")
                {
                    directionStatus = DirectionEnum.LEFT;
                }
            }
            if ((directionStatus == DirectionEnum.LEFT && measureComponent.strhand == "R")
                || (directionStatus == DirectionEnum.RIGHT && measureComponent.strhand == "L"))
            {
                directionStatus = DirectionEnum.PREPARED;
            }
        }

        if(directionStatus == DirectionEnum.PREPARED)
        {
            for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_ANCHOR.Length; i++)
            {
                measureComponents[i].ColliderEnabled(true);
            }
        }
    }

    /// <summary>
    /// 球体表示用
    /// </summary>
    /// <param name="isActive"></param>
    void SetActiveBullets(bool isActive)
    {
        for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_ANCHOR.Length; i++)
        {
            measureComponents[i].SetActiveBullet(isActive);
        }
    }


     void UpdateDirection()
    {
        // ボタン押下、最大角度確定処理
        if (directionStatus == DirectionEnum.MEASURING && CheckHandTriggerButtonDown())
        {


            DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber] = maxAngle;

            if(currentRotateNumber == 8)
            {
                //isWaiting = true;
                currentStatus = DIAGNOSIS_STATUS_ENUM.FINISH;
                ShowUI(false);
                //SetActive(backTr, false);
                return;
            }

            currentRotateNumber++;
            directionStatus = DirectionEnum.NONE;
            rightHandCollider.enabled = false;
            leftHandCollider.enabled = false;
            ShowUI(true);
            SetActiveBullets(false);

            PreparingDirection();

        }

        // 測定開始部分
        if (directionStatus == DirectionEnum.PREPARED)
        {
            directionStatus = DirectionEnum.WAITING;
            // 測定器具のコライダーセット
            for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_ANCHOR.Length; i++)
            {
                measureComponents[i].ColliderEnabled(true);
            }

        }

         // 次の方向に合わせて手のコライダーを調整
        if (directionStatus == DirectionEnum.STANDBY)
        {
            directionStatus = DirectionEnum.PREPARNG;

            string result = DEFINE_APP.HAND_TARGET[currentRotateNumber-1];

            if (result == "R")
            {
                rightHandCollider.enabled = true;
                leftHandCollider.enabled = false;
                controller = OVRInput.Controller.RTouch;
                measureStartRightComponent.SetActiveBullet(true);
            }
            else if (result == "L")
            {
                leftHandCollider.enabled = true;
                rightHandCollider.enabled = false;
                controller = OVRInput.Controller.LTouch;
                measureStartLeftComponent.SetActiveBullet(true);
            }
            else if (result == "C")
            {
                rightHandCollider.enabled = true;
                leftHandCollider.enabled = true;
                controller = OVRInput.Controller.All;
                measureStartLeftComponent.SetActiveBullet(true);
                measureStartRightComponent.SetActiveBullet(true);
            }
        }
    }


    void CallbackFromComponent(Collider collider)
    {
        if (currentStatus != DIAGNOSIS_STATUS_ENUM.DIRECT) return;

        Vector3 diff = collider.transform.position - shoulderTr.position;


        float angle = GetAngle(diff);
        //SetLabel(txtRotateTitle, angle.ToString());
        
        if(angle > maxAngle)
        {
            maxAngle = angle;
        }
    }


    float GetAngle(Vector3 diff)
    {
        Vector3 axis = Vector3.Cross(shoulderTr.forward, diff);

        float selectedAxis;
        switch (currentRotateNumber)
        {
            case 1:
            case 2:
                selectedAxis = axis.y;
                break;
            default:
                selectedAxis = axis.x;
                break;
        }

        float angle = Vector3.Angle(shoulderTr.forward, diff) * (selectedAxis * DEFINE_APP.BODY_SCALE.ARM_ROT_SIGN[currentRotateNumber] < 0 ? -1 : 1);

        return angle;
    }


    void UpdateFinish()
    {
        this.currentStatus = DIAGNOSIS_STATUS_ENUM.END;
        StartCoroutine(CoroutineWaitNextStep());
        callbackFinish();
    }



    IEnumerator CoroutineWaitNextStep(Action callback = null)
    {
        yield return new WaitForSeconds(3.0f);
        isWaitingStartDiagnosis = false;
        if(currentStatus != DIAGNOSIS_STATUS_ENUM.END)
        {
	        ShowUI(true);
        }
        if(callback != null)
        {
            callback();
        }
    }


    void ShowUI(bool active)
    {
        SetActive(objUI, active);
        SetLabel(txtTitle, statusTextTitle[currentStatus]);
        SetLabel(txtDetail, statusTextDetail[currentStatus]);

        SetActive(txtRotateTitle, currentStatus == DIAGNOSIS_STATUS_ENUM.DIRECT);
        SetActive(txtRotateDetail, currentStatus == DIAGNOSIS_STATUS_ENUM.DIRECT);
        SetLabel(txtRotateTitle, rotateNumberTitle[currentRotateNumber]);
        SetLabel(txtRotateDetail, rotateNumberDetail[currentRotateNumber]);
    }


    bool CheckPushButton()
    {
        bool result = false;
        result |= OVRInput.GetDown(OVRInput.RawButton.A);
        result |= OVRInput.GetDown(OVRInput.RawButton.B);
        result |= OVRInput.GetDown(OVRInput.RawButton.X);
        result |= OVRInput.GetDown(OVRInput.RawButton.Y);
        result |= OVRInput.GetDown(OVRInput.RawButton.B);
        result |= OVRInput.GetDown(OVRInput.RawButton.A);
        return result;
    }
}
