using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

using System.IO;

using System.Drawing;
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
    ///// <summary>
    ///// Bullet親のTransform
    ///// </summary>
    //public Transform objBulletRoot;
    //public GameObject objBulletOriginal;



    public GameObject objUI;
    public Text txtTitle;
    public Text txtDetail;


    public Collider rightHandCollider;
    public Collider leftHandCollider;

    public HandController handController;


    public MeasureComponent[] measureComponents;
    public MeasureStartComponent measureStartRightComponent;
    public MeasureStartComponent measureStartLeftComponent;
    public OvrAvatar ovrAvatar;

    public NRSComponent[] nrsComponents;
    public GameObject objNRS;

    public RenderTexture RenderTextureRef;

    [SerializeField] private AudioSource audioSourceVoice;
    [SerializeField] private List<AudioClip> diganosisVoiceList;
    [SerializeField] private List<AudioClip> directVoiceList;
    [SerializeField] private List<AudioClip> nrsVoiceList;
    [SerializeField] private AudioClip backVoice;

    enum DIAGNOSIS_STATUS_ENUM
    {
        START = -1, //初期状態
        PREPARING = 0,
        BASE = 1,//初期位置設定
        SHOULDER_ARM = 2, //肩の高さ&腕の長さ設定
        DIRECT = 4, //８方向
        NRS_PRE = 5, //NRS前
        FINISH = 3, //測定終了
        END = 6, //  測定終了待ち
        NRS_POST = 7 //NRS後
    }

    DIAGNOSIS_STATUS_ENUM currentStatus = DIAGNOSIS_STATUS_ENUM.START;

    /// <summary>
    /// 回旋・屈曲・伸展の現在測定している方向 DEFINEのDIAGNOSIS_DIRECTのIndex
    /// </summary>
    int currentDiagnosisDirectsIndex = 0;
    int currentIndex { get { return DEFINE_APP.BODY_SCALE.DIAGNOSIS_DIRECTS[currentDiagnosisDirectsIndex]; } }

    int currentNRSIndex = 1;

    Dictionary<DIAGNOSIS_STATUS_ENUM, string> statusTextTitle = new Dictionary<DIAGNOSIS_STATUS_ENUM, string>
   {
       { DIAGNOSIS_STATUS_ENUM.START, "準備中" },
       { DIAGNOSIS_STATUS_ENUM.PREPARING, "測定を開始" },
       {DIAGNOSIS_STATUS_ENUM.BASE,"体の向き＆座高測定" },
       { DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM, "肩の高さ＆腕長さ測定" },
       { DIAGNOSIS_STATUS_ENUM.DIRECT, "" },
       { DIAGNOSIS_STATUS_ENUM.NRS_PRE, "" },
       { DIAGNOSIS_STATUS_ENUM.FINISH, "測定終了" },
       { DIAGNOSIS_STATUS_ENUM.END, "測定終了" }
   };

    Dictionary<DIAGNOSIS_STATUS_ENUM, string> statusTextDetail = new Dictionary<DIAGNOSIS_STATUS_ENUM, string>
   {
       { DIAGNOSIS_STATUS_ENUM.START, "リラックスしてお待ちください。" },
       { DIAGNOSIS_STATUS_ENUM.PREPARING, "測定を開始します。\nコントローラーのボタンをどれか押してください。" },
       { DIAGNOSIS_STATUS_ENUM.BASE, "椅子の背もたれにもたれない状態になってください。\n背筋を伸ばし、前を真っすぐ見てください。\nカウントを開始します。" },
       { DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM, "背中を伸ばした状態のまま、両腕を前方に真っすぐ伸ばしてください。\nカウントを開始します。" },
       { DIAGNOSIS_STATUS_ENUM.DIRECT, "" },
       { DIAGNOSIS_STATUS_ENUM.NRS_PRE, "" },
       { DIAGNOSIS_STATUS_ENUM.FINISH, "測定が終了しました。トレーニング場所へ移動します。" },
       { DIAGNOSIS_STATUS_ENUM.END, "測定が終了しました。トレーニング場所へ移動します。" }
   };


    Dictionary<int, string> directTitle = new Dictionary<int, string>
   {
       { 1, "右回旋" },
       { 2, "左回旋" },
       { 3, ""},
       { 4, "伸展" },
       { 5, ""},
       { 6, "" },
       { 7, "屈曲" },
       { 8, "" },
   };


    Dictionary<int, string> directDetail = new Dictionary<int, string>
   {
       { 1, "右腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください。"},
       { 2, "左腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください。" },
       { 3, "" },
       { 4, "両腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください。" },
       { 5, "" },
       { 6, "" },
       { 7, "両腕を真っすぐ前に伸ばし、手のシルエットの位置に手を置き、腰と腕をひねりながら順番に球に触れてください。" },
       { 8, "" }
   };

    Dictionary<int, string> NRSTitle = new Dictionary<int, string>
   {
       { 1, "安静時痛" },
       { 2, "運動時痛" },
       { 3, "運動恐怖"},
   };


    Dictionary<int, string> NRSDetail = new Dictionary<int, string>
   {
       { 1, "現在、安静にしているときの痛みを０～１０で表してください。\n該当するボールに触れてください。"},
       { 2, "現在、運動したときの痛みを０～１０で表してください。\n該当するボールに触れてください。" },
       { 3, "体を動かそうとしたとき、どれくらいその動作が怖いと思うかを０～１０で表してください。\n該当するボールに触れてください。" },
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



    public void Init(Action callbackFinish)
    {
        SetActive(objUI, true);

        this.callbackFinish = callbackFinish;
        currentStatus = DIAGNOSIS_STATUS_ENUM.START;
        isWaitingStartDiagnosis = true;

		//SetActive(backTr, false);
	}


	public void StartDiagnosis()
    {
        ShowUI(true);
        isWaitingStartDiagnosis = true;
        StartCoroutine(CoroutineWaitNextStep());
        currentStatus = DIAGNOSIS_STATUS_ENUM.PREPARING;
        audioSourceVoice.clip = diganosisVoiceList[(int)currentStatus];
        audioSourceVoice.Play();

        // ここからはTriggerを使う。
        //rightHandTr.GetComponent<MeshCollider>().isTrigger = true;
        //leftHandCollider.GetComponent<MeshCollider>().isTrigger = true;

    }



    void SavePng()
    {
        //Debug.Log("SavePng");
        Texture2D tex = new Texture2D(RenderTextureRef.width, RenderTextureRef.height, TextureFormat.RGB24, false);
        RenderTexture.active = RenderTextureRef;
        tex.ReadPixels(new Rect(0, 0, RenderTextureRef.width, RenderTextureRef.height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        //Write to a file in the project folder
        //Debug.Log(Application.dataPath);
        //File.WriteAllBytes(Application.dataPath + "/Resources/ResultSheet.png", bytes);
        File.WriteAllBytes(Application.persistentDataPath + "/ResultSheet.png", bytes);

    }


    // Update is called once per frame
    void Update () {
        if (isWaitingStartDiagnosis) return;


        switch (currentStatus)
        {
            case DIAGNOSIS_STATUS_ENUM.PREPARING:
                UpdatePreparing();
                break;
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


    void UpdatePreparing()
    {
        if (OVRInput.GetDown(OVRInput.Button.Any))
        {
            currentStatus = DIAGNOSIS_STATUS_ENUM.BASE;
            ShowUI(true);
            audioSourceVoice.clip = diganosisVoiceList[(int)currentStatus];
            audioSourceVoice.Play();
            StartCoroutine(FinishVoicePreparing(audioSourceVoice.clip.length));

        }

    }


    private IEnumerator FinishVoicePreparing(float clipLength)
    {
        if (currentStatus == DIAGNOSIS_STATUS_ENUM.BASE)
        {
            yield return new WaitForSeconds(clipLength);

            audioSourceVoice.clip = backVoice;
            audioSourceVoice.Play();
            NextBase();
        }
    }


    void UpdateBase()
    {
        if (CheckThumbstickDown())
        {
            NextBase();
        }
    }


    void NextBase()
    {
        if (currentStatus == DIAGNOSIS_STATUS_ENUM.BASE)
        {
            isWaitingStartDiagnosis = true;
            currentStatus = DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM;

            Cache.user.BodyScaleData.playerBasePos = new Vector3(centerEyeTr.position.x, playerBaseTr.position.y, centerEyeTr.position.z);
            playerBaseTr.position = Cache.user.BodyScaleData.playerBasePos;
			Cache.user.BodyScaleData.playerBaseRot = new Vector3(playerBaseTr.rotation.eulerAngles.x, centerEyeTr.rotation.eulerAngles.y, playerBaseTr.rotation.eulerAngles.z);
            playerBaseTr.rotation = Quaternion.Euler(Cache.user.BodyScaleData.playerBaseRot);
			Cache.user.UserData.HeadPos = centerEyeTr.position - Cache.user.BodyScaleData.playerBasePos;
            //backTr.localPosition = DEFINE_APP.BODY_SCALE.BACK_POS;

            ShowUI(false);

            audioSourceVoice.clip = diganosisVoiceList[(int)currentStatus];
            audioSourceVoice.Play();
            StartCoroutine(FinishVoiceBase(audioSourceVoice.clip.length));

            //System.Drawing.Printing.PrintDocument pd =
            //    new System.Drawing.Printing.PrintDocument();

            ////PrintPageイベントハンドラの追加
            //pd.PrintPage +=
            //    new System.Drawing.Printing.PrintPageEventHandler(pd_PrintPage);
            ////印刷を開始する
            //pd.Print();

            StartCoroutine(CoroutineWaitNextStep());
        }
    }


        private IEnumerator FinishVoiceBase(float clipLength)
    {
        if (currentStatus == DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM) {
            yield return new WaitForSeconds(clipLength);

            audioSourceVoice.clip = backVoice;
            audioSourceVoice.Play();

            yield return new WaitForSeconds(audioSourceVoice.clip.length);

            NextShoulderArm();
        }
    }

    private void pd_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e)
    {
        //画像を読み込む
        System.Drawing.Image img = System.Drawing.Image.FromFile(Application.persistentDataPath + "/test.jpg");
        //画像を描画する
        e.Graphics.DrawImage(img, e.MarginBounds);
        //次のページがないことを通知する
        e.HasMorePages = false;
        //後始末をする
        img.Dispose();
    }

    void UpdateShoulderArm()
    {
        if (CheckThumbstickDown())
        {
            NextShoulderArm();
        }
    }


    void NextShoulderArm()
    {
        if (currentStatus == DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM)
        {
            isWaitingStartDiagnosis = true;
            currentStatus = DIAGNOSIS_STATUS_ENUM.DIRECT;

            Vector3 averagePos = new Vector3(((rightHandTr.position.x + leftHandTr.position.x) / 2f), ((rightHandTr.position.y + leftHandTr.position.y) / 2f), ((rightHandTr.position.z + leftHandTr.position.z) / 2f));
			Cache.user.UserData.HandPosR = playerBaseTr.InverseTransformPoint(rightHandTr.position);
			Cache.user.UserData.HandPosL = playerBaseTr.InverseTransformPoint(leftHandTr.position);

            //shoulderTr.localPosition = DEFINE_APP.BODY_SCALE.SHOULDER_POS_C;
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
        measureStartLeftComponent.SetActiveBullet(false);

        GameObject objRight = Instantiate(ovrAvatar.trackedComponents["hand_right"].gameObject, measureStartRightComponent.trRootObj.transform);
        objRight.transform.localPosition = Vector3.zero;
        objRight.transform.localRotation = Quaternion.identity;
        measureStartRightComponent.SetActiveBullet(false);

        //SetActive(objRight, false);

        PreparingDirection();
    }



    void PreparingDirection()
    {
        //for (int i = 0; i < directRotateTrs.Length; i++)
        //{
        //    directRotateTrs[i].localRotation = Quaternion.Euler(new Vector3(directRotateTrs[i].localRotation.x, directRotateTrs[i].localRotation.y, DEFINE_APP.BODY_SCALE.SHOULDER_ROT_Z[currentRotateNumber]));
        //}

        //shoulderTr.localPosition = DEFINE_APP.SHOULDER_POS_DIC[DEFINE_APP.HAND_TARGET[currentIndex - 1]];

        audioSourceVoice.clip = directVoiceList[currentDiagnosisDirectsIndex];
        audioSourceVoice.Play();

        StartCoroutine(CoroutineInstantiateBullets(PreparedDirection));

    }


    IEnumerator CoroutineInstantiateBullets(Action callback = null)
    {
        for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex]; i++)
        {
            measureComponents[i].Init(currentIndex, (float)(i+1)/(float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex], Hit);
            measureComponents[i].SetActiveBullet(true);
            measureComponents[i].ColliderEnabled(false);

            yield return new WaitForSeconds(0.1f);
        }
        PreparedDirection();

        measureStartLeftComponent.Init(currentIndex, OVRInput.Controller.LTouch, HitStartMeasure);
        measureStartRightComponent.Init(currentIndex, OVRInput.Controller.RTouch, HitStartMeasure);

    }


    void PreparedDirection()
    {
        directionStatus = DirectionEnum.STANDBY;
    }


    void Hit(MeasureComponent measureComponent)
    {

        hitDeltaTime = 0f;
        directionStatus = DirectionEnum.MEASURING;

		/*
        Vector3 backRot = measureComponent.trBackRoot.localRotation.eulerAngles;
        Vector3 shoulderRot = measureComponent.trSholderRoot.localRotation.eulerAngles;

        float resultXBack = (backRot.x >= 180f) ? backRot.x -360f : backRot.x;

        float resultXShoulder = (shoulderRot.x >= 180f) ? shoulderRot.x -360f : shoulderRot.x;

        float resultYBack = (backRot.y >= 180f) ? backRot.y -360f : backRot.y;

        float resultYShoulder = (shoulderRot.y >= 180f) ? shoulderRot.y-360f : shoulderRot.y;

        float resultZBack = (backRot.z >= 180f) ? backRot.z-360f : backRot.z;

        float resultZShoulder = (shoulderRot.z >= 180f) ? shoulderRot.z-360f : shoulderRot.z;

		DEFINE_APP.BODY_SCALE.GOAL_DIC[currentIndex][DEFINE_APP.BODY_SCALE.BACK_ROT] = new Vector3(resultXBack, resultYBack, resultZBack);
        DEFINE_APP.BODY_SCALE.GOAL_DIC[currentIndex][DEFINE_APP.BODY_SCALE.SHOULDER_ROT] = new Vector3(resultXShoulder, resultYShoulder, resultZShoulder);
		*/

		OVRInput.Controller result = DEFINE_APP.HAND_TARGET[currentIndex - 1];

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

		float angleSum = 0f;
		float angle;
        Vector3 axis;

        measureComponent.trBackRoot.localRotation.ToAngleAxis(out angle, out axis);
		angleSum += angle;
		measureComponent.trSholderRoot.localRotation.ToAngleAxis(out angle, out axis);
		angleSum += angle;

		Cache.user.BodyScaleData.goalDic[this.currentIndex] = angle;
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
            if (directionStatus == DirectionEnum.PREPARNG)
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
            for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex]; i++)
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
        for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex]; i++)
        {
            measureComponents[i].SetActiveBullet(isActive);
        }
    }


     void UpdateDirection()
    {
        if(directionStatus == DirectionEnum.MEASURING)
        {
            hitDeltaTime += Time.deltaTime;
        }

        // ボタン押下、最大角度確定処理
        if (directionStatus == DirectionEnum.MEASURING && (CheckThumbstickDown() || hitDeltaTime > 2f))
        {

            // 全部の方向が終わった時
            if(currentDiagnosisDirectsIndex == DEFINE_APP.BODY_SCALE.DIAGNOSIS_DIRECTS.Length-1)
            {
                //isWaiting = true;

                ShowUI(false);
                currentStatus = DIAGNOSIS_STATUS_ENUM.NRS_PRE;
                currentNRSIndex = 0;
                InitNRSComponents();

                for (int i = 0; i < directRotateTrs.Length; i++)
                {
                    SetActive(directRotateTrs[i], false);
                }

				//斜め方向を決定
				Cache.user.BodyScaleData.SetDiagonal();
				//DEFINE_APP.BODY_SCALE.SetDefineDiagonal();

                return;
            }

            currentDiagnosisDirectsIndex++;
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
            for (int i = 0; i < DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[currentIndex]; i++)
            {
                measureComponents[i].ColliderEnabled(true);
            }

        }

         // 次の方向に合わせて手のコライダーを調整
        if (directionStatus == DirectionEnum.STANDBY)
        {
            directionStatus = DirectionEnum.PREPARNG;

            OVRInput.Controller result = DEFINE_APP.HAND_TARGET[currentIndex - 1];

            if (result == OVRInput.Controller.RTouch)
            {
                rightHandCollider.enabled = true;
                leftHandCollider.enabled = false;
                controller = OVRInput.Controller.RTouch;
                measureStartRightComponent.SetActiveBullet(true);
            }
            else if (result == OVRInput.Controller.LTouch)
            {
                leftHandCollider.enabled = true;
                rightHandCollider.enabled = false;
                controller = OVRInput.Controller.LTouch;
                measureStartLeftComponent.SetActiveBullet(true);
            }
            else if (result == OVRInput.Controller.Touch)
            {
                rightHandCollider.enabled = true;
                leftHandCollider.enabled = true;
                controller = OVRInput.Controller.All;
                measureStartLeftComponent.SetActiveBullet(true);
                measureStartRightComponent.SetActiveBullet(true);
            }
        }
    }


    void InitNRSComponents()
    {
        audioSourceVoice.clip = nrsVoiceList[currentNRSIndex];
        audioSourceVoice.Play();

        currentNRSIndex++;
        SetActive(objNRS, true);
        ShowUI(true);

        for (int i = 0; i < nrsComponents.Length; i++)
        {
            nrsComponents[i].Init(i, HitNRSComponent);
            nrsComponents[i].SetActiveBullet(true);
            nrsComponents[i].ColliderEnabled(true);
        }
    }


    void HitNRSComponent(NRSComponent nrsComponent)
    {
		//DEFINE_APP.NRS_PRE[currentNRSIndex] = nrsComponent.num;
		Cache.user.MeasureData.SetPreNrs(currentNRSIndex, nrsComponent.num);
        for (int i = 0; i < nrsComponents.Length; i++)
        {
            nrsComponents[i].ColliderEnabled(false);
        }

        if(currentNRSIndex >= 3)
        {
            StartCoroutine("CoroutineFinishNRSPre");
        }
        else
        {
            StartCoroutine("CoroutineNextNRS");
        }

        OVRInput.Controller result = DEFINE_APP.HAND_TARGET[currentIndex - 1];

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
    }

    IEnumerator CoroutineNextNRS()
    {
        yield return new WaitForSeconds(1f);
        InitNRSComponents();

    }


    IEnumerator CoroutineFinishNRSPre()
    {
        yield return new WaitForSeconds(1f);
        SetActive(objNRS, false);
        currentStatus = DIAGNOSIS_STATUS_ENUM.FINISH;
        audioSourceVoice.clip = diganosisVoiceList[(int)currentStatus];
        audioSourceVoice.Play();
    }



    void UpdateFinish()
    {
        this.currentStatus = DIAGNOSIS_STATUS_ENUM.END;
		Cache.user.MeasureData.SetMaxRomMeasure(Cache.user.BodyScaleData.goalDic);
        StartCoroutine(CoroutineWaitNextStep());
        callbackFinish();

    }



    IEnumerator CoroutineWaitNextStep(Action callback = null)
    {
        yield return new WaitForSeconds(1.0f);
        isWaitingStartDiagnosis = false;
	    ShowUI(true);
        
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

        switch (currentStatus)
        {
            case DIAGNOSIS_STATUS_ENUM.DIRECT:
                SetLabel(txtTitle, directTitle[currentIndex]);
                SetLabel(txtDetail, directDetail[currentIndex]);
                break;
            case DIAGNOSIS_STATUS_ENUM.NRS_PRE:
                SetLabel(txtTitle, NRSTitle[currentNRSIndex]);
                SetLabel(txtDetail, NRSDetail[currentNRSIndex]);
                break;
            default:
                break;
        }
    }

}
