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
    /// 測定用円柱の角度調整用Transform
    /// </summary>
    public Transform armBaseTr;

    public GameObject objUI;
    public Text txtTitle;
    public Text txtDetail;
    public Text txtRotateTitle;
    public Text txtRotateDetail;


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


	// Use this for initialization
	void Start () {
        SetActive(objUI, true);
	}


    public void Init(Action callbackFinish)
    {
        this.callbackFinish = callbackFinish;
        currentStatus = DIAGNOSIS_STATUS_ENUM.BASE;
        StartCoroutine(CoroutineWaitNextStep());
        isWaiting = true;

    }




    // Update is called once per frame
    void Update () {
        if (isWaiting) return;

        if (OVRInput.GetDown(OVRInput.Button.Any))
        {
            ShowUI(false);
        }

        switch (currentStatus)
        {
            case DIAGNOSIS_STATUS_ENUM.BASE:
                UpdateBase();
                break;
        }
	}

    
    void UpdateBase()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.Any))
        {
            isWaiting = true;
            currentStatus = DIAGNOSIS_STATUS_ENUM.SHOULDER_ARM;
            StartCoroutine(CoroutineWaitNextStep());
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
