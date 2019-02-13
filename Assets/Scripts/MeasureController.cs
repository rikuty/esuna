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
    ///　肩の高さと測定器具の測定方向への設置用のTransform
    /// </summary>
    public Transform shoulderTr;
    /// <summary>
    /// 手の長さ調整用Transform
    /// </summary>
    public Transform handTr;
    /// <summary>
    /// 測定用円柱の角度調整用Transform
    /// </summary>
    public Transform armBaseTr;
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


    public GameObject objUI;
    public Text txtTitle;
    public Text txtDetail;
    public Text txtRotateTitle;
    public Text txtRotateDetail;

    public MeasureComponent measureComponent;

    public Collider measureCollider;


    enum DIAGNOSIS_STATUS_ENUM
    {
        PREPARE, //初期状態
        BASE, //初期位置設定
        SHOULDER_ARM, //肩の高さ&腕の長さ設定
        DIRECT, //８方向
        FINISH, //測定終了
    }

    DIAGNOSIS_STATUS_ENUM currentStatus = DIAGNOSIS_STATUS_ENUM.PREPARE;

    /// <summary>
    /// 回旋・屈曲・伸展の現在測定している方向
    /// </summary>
    int currentRotateNumber = 1;
   
    Dictionary<DIAGNOSIS_STATUS_ENUM, string> statusTextTitle = new Dictionary<DIAGNOSIS_STATUS_ENUM, string>
    {
        { DIAGNOSIS_STATUS_ENUM.PREPARE, "測定準備中" },
        { DIAGNOSIS_STATUS_ENUM.BASE, "初期位置設定" },
        { DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM, "肩の高さ&腕長さ測定" },
        { DIAGNOSIS_STATUS_ENUM.DIRECT, "８方向測定" },
        { DIAGNOSIS_STATUS_ENUM.FINISH, "測定終了" }
    };

    Dictionary<DIAGNOSIS_STATUS_ENUM, string> statusTextDetail = new Dictionary<DIAGNOSIS_STATUS_ENUM, string>
    {
        { DIAGNOSIS_STATUS_ENUM.PREPARE, "測定の準備をしています。少々お待ちください。" },
        { DIAGNOSIS_STATUS_ENUM.BASE, "椅子の背もたれにもたれない状態で背筋を伸ばし、前を真っすぐ見て、ボタンをどれか押してください。" },
        { DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM, "背筋を伸ばした状態のまま、両腕を前方に真っすぐ伸ばし、ボタンをどれか押してください。" },
        { DIAGNOSIS_STATUS_ENUM.DIRECT, "" },
        { DIAGNOSIS_STATUS_ENUM.FINISH, "測定が終了しました。ゲームに進んでください。" }
    };


    Dictionary<int, string> rotateNumberTitle = new Dictionary<int, string>
    {
        { 1, "左回旋" },
        { 2, "右回旋" },
        { 3, "左回旋&伸展" },
        { 4, "伸展" },
        { 5, "右回旋&伸展" },
        { 6, "左回旋&屈曲" },
        { 7, "屈曲" },
        { 8, "右回旋&屈曲" },
    };


    Dictionary<int, string> rotateNumberDetail = new Dictionary<int, string>
    {
        { 1, "右腕を真っすぐ前に伸ばし、前方の板を左方向にできるだけ押してください"},
        { 2, "左腕を真っすぐ前に伸ばし、前方の板を右方向にできるだけ押してください" },
        { 3, "右腕を真っすぐ前に伸ばし、前方の板を左上方向にできるだけ押してください" },
        { 4, "両腕を真っすぐ前に伸ばし、前方の板を上方向に押してください" },
        { 5, "左腕を真っすぐ前に伸ばし、前方の板を右上方向に押してください" },
        { 6, "右腕を真っすぐ前に伸ばし、前方の板を左下に押してください" },
        { 7, "両腕を真っすぐ前に伸ばし、前方の板を下方向に押してください" },
        { 8, "左腕を真っすぐ前に伸ばし、前方の板を右下に押してください" }
    };


    Action callbackFinish;

    bool isWaiting = false;

    int currentDiagnosisRotAnchorIndex;

    float maxAngle;
    Vector3 maxAngleVector;


	// Use this for initialization
	void Start () {
        SetActive(objUI, true);
	}


    public void Init(Action callbackFinish)
    {
        this.callbackFinish = callbackFinish;
        currentStatus = DIAGNOSIS_STATUS_ENUM.DIRECT;
        isWaiting = true;
        currentDiagnosisRotAnchorIndex = 0;
        maxAngle = 0f;
        measureComponent.Init(CallbackFromComponent);

        measureCollider.enabled = true;

        StartCoroutine(CoroutineWaitNextStep());
    }




    // Update is called once per frame
    void Update () {
        if (isWaiting) return;


        switch (currentStatus)
        {
            case DIAGNOSIS_STATUS_ENUM.BASE:
                UpdateBase();
                break;
            case DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM:
                UpdateShoulderArm();
                break;
            case DIAGNOSIS_STATUS_ENUM.DIRECT:
                UpdateDirction();
                break;
        }
	}

    
    void UpdateBase()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.Any))
        {
            isWaiting = true;
            currentStatus = DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM;

            DEFINE_APP.BODY_SCALE.PLAYER_BASE_POS = new Vector3(centerEyeTr.position.x, playerBaseTr.position.y, centerEyeTr.position.z);
            playerBaseTr.position = DEFINE_APP.BODY_SCALE.PLAYER_BASE_POS;
            DEFINE_APP.BODY_SCALE.PLAYER_BASE_ROT = new Vector3(playerBaseTr.rotation.eulerAngles.x, centerEyeTr.rotation.eulerAngles.y, playerBaseTr.rotation.eulerAngles.z);
            playerBaseTr.rotation = Quaternion.Euler(DEFINE_APP.BODY_SCALE.PLAYER_BASE_ROT);

            ShowUI(false);

            StartCoroutine(CoroutineWaitNextStep());
        }
    }


    void UpdateShoulderArm()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.Any))
        {
            isWaiting = true;
            currentStatus = DIAGNOSIS_STATUS_ENUM.DIRECT;

            Vector3 averagePos = new Vector3(((rightHandTr.position.x + leftHandTr.position.x) / 2f), ((rightHandTr.position.y + leftHandTr.position.y) / 2f), ((rightHandTr.position.z + leftHandTr.position.z) / 2f));
            DEFINE_APP.BODY_SCALE.SHOULDER_POS = new Vector3(shoulderTr.position.x, averagePos.y, shoulderTr.position.z);
            shoulderTr.position = DEFINE_APP.BODY_SCALE.SHOULDER_POS;
            //DEFINE_APP.BODY_SCALE.ARM_POS = new Vector3(handTr.position.x, handTr.position.y, averagePos.z);
            //handTr.position = DEFINE_APP.BODY_SCALE.ARM_POS;

            ShowUI(false);

            StartCoroutine(CoroutineWaitNextStep());

            measureCollider.enabled = true;
        }
    }


    void UpdateDirction()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.Any))
        {

            isWaiting = true;
            currentRotateNumber++;

            DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber].Add((int)maxAngle, maxAngleVector);

            StartCoroutine(CoroutineWaitNextStep());
        }
    }


    void CallbackFromComponent(Collider collider)
    {
        if (currentStatus != DIAGNOSIS_STATUS_ENUM.DIRECT) return;

        Vector3 diff = collider.transform.position - shoulderTr.position;

        Vector3 axis = Vector3.Cross(shoulderTr.forward, diff);

        float angle = Vector3.Angle(shoulderTr.forward, diff) * (axis.x < 0 ? 1 : -1);
        SetLabel(txtRotateTitle, angle.ToString());
        
        if(angle > DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_ANCHOR[currentDiagnosisRotAnchorIndex])
        {
            if (!DEFINE_APP.BODY_SCALE.GOAL_DIC.ContainsKey(currentRotateNumber))
            {
                DEFINE_APP.BODY_SCALE.GOAL_DIC.Add(currentRotateNumber, new Dictionary<int, Vector3>());
            }
            Vector3 vector = new Vector3(diff.x, diff.y, diff.z);
            DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber].Add(currentDiagnosisRotAnchorIndex, vector);
            SetLabel(txtRotateDetail, "("
                + angle.ToString() + ":  "
                + DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber][currentDiagnosisRotAnchorIndex].x.ToString() + ","
                + DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber][currentDiagnosisRotAnchorIndex].y.ToString() + ","
                + DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber][currentDiagnosisRotAnchorIndex].z.ToString()
                + ")");

            currentDiagnosisRotAnchorIndex++;
        }
        if(angle > maxAngle)
        {
            maxAngle = angle;
            maxAngleVector = diff;
        }
    }



    IEnumerator CoroutineWaitNextStep()
    {
        yield return new WaitForSeconds(3.0f);
        isWaiting = false;
        ShowUI(true);
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
