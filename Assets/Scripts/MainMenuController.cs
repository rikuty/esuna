using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Gamestrap;

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


public class MainMenuController : UtilComponent
{

    private enum MENU_STATUS_ENUM
    {
        PREPARE,
        WAIT,
        DIAGNOSIS,
        GAME
    }

    private MENU_STATUS_ENUM currentStatus = MENU_STATUS_ENUM.PREPARE;

    private string userID;
    private string userName;
    private DateTime dateTime;
    public Text txtID;
    public Text txtName;
    public Text txtTitle;
    public Text txtDetail;

    //public HandController handController;
    //public PanelButtonComponent panelButtonComponent;
    public BackPanelComponent backPanelComponent;

    public MeasureController measureController;

    //public BirdTransfer birdTransfer;
    public GameObject objWarpEffect;

    private GameData gameData;

    void Start()
    {

        // ユーザーデーター取得　※消しちゃダメ
        StartCoroutine(ConnectAPI("http://18.179.32.33/sample/GetUserData.php", GetUserData));

        this.gameData = GameData.Instance;

        this.dateTime = DateTime.Now;
        this.userID = this.dateTime.ToString("yyMMddHHmm");
        SetLabel(this.txtID, this.userID);
        SetLabel(this.txtName, this.userName);
        SetLabel(this.txtTitle, "目の前にある青い箱を押してください");
        SetLabel(this.txtDetail, "");

        //handController.Init(LoadMain);
        backPanelComponent.Init(FinishPushButton, LoadMain);
        //birdTransfer.Init(LoadMain);

        //measureController.Init(() => { birdTransfer.PlayStart(); });
        measureController.Init(() => { SetActive(objWarpEffect, true); });

        currentStatus = MENU_STATUS_ENUM.WAIT;
    }

    // JSON変換　※消しちゃダメ
    private void GetUserData(string val)
    {
        Debug.Log(val);

        UserData userData = JsonConvert.DeserializeObject<UserData>(val);
        //Debug.Log("userId : "+userData.user_id);
        //Debug.Log("name : " + userData.user_name);
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) { birdTransfer.PlayStart(); }
        if (Input.GetKeyDown(KeyCode.Space)) { SetActive(objWarpEffect, true); }
    }


    private void FinishPushButton()
    {
        measureController.StartDiagnosis();
    }

    public void LoadMain()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        SetActive(objWarpEffect, true);
        Camera.main.GetComponent<SceenFade>().LoadSceenWithFade("Game");
    }

    private void FinishDiagnosis()
    {
        currentStatus = MENU_STATUS_ENUM.WAIT;
    }
}