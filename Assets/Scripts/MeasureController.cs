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

    public Collider rightHandCollider;
    public Collider leftHandCollider;


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

    // 測定中。colliderが当たってからボタンが押されるまで。
    bool isDiagnosising = false;

    bool isSetHandCollider = false;
    OVRInput.Controller controller;


	// Use this for initialization
	void Start () {
        SetActive(objUI, true);
	}


    public void Init(Action callbackFinish)
    {
        this.callbackFinish = callbackFinish;
        currentStatus = DIAGNOSIS_STATUS_ENUM.PREPARE;
        isWaiting = true;
        currentDiagnosisRotAnchorIndex = 0;
        maxAngle = 0f;
        measureComponent.Init(CallbackFromComponent);

        measureCollider.enabled = false;
        SetActive(shoulderTr, false);

    }


    public void StartDiagnosis()
    {
        ShowUI(true);
        isWaiting = true;
        StartCoroutine(CoroutineWaitNextStep());
        currentStatus = DIAGNOSIS_STATUS_ENUM.BASE;


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
            case DIAGNOSIS_STATUS_ENUM.FINISH:
                UpdateFinish();
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
            DEFINE_APP.BODY_SCALE.HAND_POS = new Vector3(handTr.position.x, handTr.position.y, averagePos.z);
            //handTr.position = DEFINE_APP.BODY_SCALE.ARM_POS;

            ShowUI(false);

            StartCoroutine(CoroutineWaitNextStep(FinishShoulderArm));


        }
    }


    void FinishShoulderArm()
    {
        measureCollider.enabled = true;
        SetActive(shoulderTr, true);
        shoulderTr.localRotation = Quaternion.Euler(new Vector3(shoulderTr.localRotation.x, shoulderTr.localRotation.y, DEFINE_APP.BODY_SCALE.SHOULDER_ROT_Z[currentRotateNumber]));
    }

    void UpdateDirction()
    {

        if (measureCollider.enabled && OVRInput.GetDown(OVRInput.RawButton.Any) && DEFINE_APP.BODY_SCALE.GOAL_DIC.Count == currentRotateNumber)
        {


            DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber].Add(maxAngleVector);

            if(currentRotateNumber == 8)
            {
                //isWaiting = true;
                currentStatus = DIAGNOSIS_STATUS_ENUM.FINISH;
                ShowUI(false);
                SetActive(shoulderTr, false);
                return;
            }

            currentRotateNumber++;
            shoulderTr.localRotation = Quaternion.Euler(new Vector3(shoulderTr.localRotation.x, shoulderTr.localRotation.y, DEFINE_APP.BODY_SCALE.SHOULDER_ROT_Z[currentRotateNumber]));
            armBaseTr.localRotation = Quaternion.identity;
            currentDiagnosisRotAnchorIndex = 0;
            measureCollider.enabled = false;
            maxAngle = 0f;
            isSetHandCollider = false;
            rightHandCollider.enabled = false;
            leftHandCollider.enabled = false;
            ShowUI(true);

        }
        if (!measureCollider.enabled)
        {
            Vector3 diffRight = rightHandTr.position - shoulderTr.position;
            float angleRight = GetAngle(diffRight);
            Vector3 diffLeft = leftHandTr.position - shoulderTr.position;
            float angleLeft = GetAngle(diffLeft);
            measureCollider.enabled = 
                (angleRight < 0 && controller == OVRInput.Controller.RTouch) 
                || (angleLeft < 0 && controller == OVRInput.Controller.LTouch)
                || (angleLeft < 0 && angleRight < 0 && controller == OVRInput.Controller.All);
        }

        if (!isSetHandCollider)
        {
            isSetHandCollider = true;

            bool resultRight = Array.IndexOf(DEFINE_APP.RIGHT_HAND_TARGET, currentRotateNumber) >= 0;
            bool resultLeft = Array.IndexOf(DEFINE_APP.LEFT_HAND_TARGET, currentRotateNumber) >= 0;
            bool resultBoth = Array.IndexOf(DEFINE_APP.BOTH_HAND_TARGET, currentRotateNumber) >= 0;
            if (resultRight)
            {
                rightHandCollider.enabled = true;
                leftHandCollider.enabled = false;
                controller = OVRInput.Controller.RTouch;
            }
            else if (resultLeft)
            {
                leftHandCollider.enabled = true;
                rightHandCollider.enabled = false;
                controller = OVRInput.Controller.LTouch;
            }
            else if (resultBoth)
            {
                rightHandCollider.enabled = true;
                leftHandCollider.enabled = true;
                controller = OVRInput.Controller.All;

            }
        }
    }


    void CallbackFromComponent(Collider collider)
    {
        if (currentStatus != DIAGNOSIS_STATUS_ENUM.DIRECT) return;

        Vector3 diff = collider.transform.position - shoulderTr.position;


        float angle = GetAngle(diff);
        //SetLabel(txtRotateTitle, angle.ToString());
        
        if(angle > DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_ANCHOR[currentDiagnosisRotAnchorIndex])
        {
            if (!DEFINE_APP.BODY_SCALE.GOAL_DIC.ContainsKey(currentRotateNumber))
            {
                DEFINE_APP.BODY_SCALE.GOAL_DIC.Add(currentRotateNumber, new List<Vector3>());
            }
            Vector3 vector = new Vector3(diff.x, diff.y, diff.z);
            DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber].Add(vector);
            //SetLabel(txtRotateDetail, "("
            //    + angle.ToString() + ":  "
            //    + DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber][currentDiagnosisRotAnchorIndex].x.ToString() + ","
            //    + DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber][currentDiagnosisRotAnchorIndex].y.ToString() + ","
            //    + DEFINE_APP.BODY_SCALE.GOAL_DIC[currentRotateNumber][currentDiagnosisRotAnchorIndex].z.ToString()
            //    + ")");

            currentDiagnosisRotAnchorIndex++;
        }
        if(angle > maxAngle)
        {
            maxAngle = angle;
            maxAngleVector = diff;
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
        isWaiting = false;
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
