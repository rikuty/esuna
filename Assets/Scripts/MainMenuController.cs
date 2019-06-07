﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using Gamestrap;

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class MainMenuController : UtilComponent
{

    public GameObject objDirectionalLight;

    private enum MENU_STATUS_ENUM
    {
        PREPARE,
        WAIT,
        DIAGNOSIS,
        GAME
    }

    private MENU_STATUS_ENUM currentStatus = MENU_STATUS_ENUM.PREPARE;

    public GameObject objUI;

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

    public GameObject objResult;

    
	//　読み込み率を表示するスライダー
	[SerializeField] private Slider slider;
	//　非同期動作で使用するAsyncOperation
	private AsyncOperation async;

    public ResultFormatArea resultFormatArea;


    private void Start()
    {
        if(SceneManager.GetSceneAt(0).name == "Title")
        {
            SetActive(objDirectionalLight, true);
            Init();
        }
    }


    public void Init()
    {

        // ユーザーデーター取得　※消しちゃダメ
        //StartCoroutine(ConnectAPI("http://18.179.32.33/sample/GetUserData.php", GetUserData));
        StartCoroutine(ConnectAPI("http://dev.rikuty.net/api/GetUserData.php", GetUserData));

        this.gameData = GameData.Instance;

        //this.dateTime = DateTime.Now;
        //this.userID = this.dateTime.ToString("yyMMddHHmm");
        //SetLabel(this.txtID, this.userID);
        //SetLabel(this.txtName, this.userName);
        SetLabel(this.txtTitle, "");
        SetLabel(this.txtDetail, "");

        //handController.Init(LoadMain);
        backPanelComponent.Init(FinishPushButton, LoadMain);
        //birdTransfer.Init(LoadMain);

        //measureController.Init(() => { birdTransfer.PlayStart(); });
        measureController.Init(LoadMain);

        currentStatus = MENU_STATUS_ENUM.WAIT;

        StartCoroutine("WaitStartCoroutine");

        SetActive(objUI, true);
        SetActive(objResult, false);

    }


    IEnumerator WaitStartCoroutine()
    {
        yield return new WaitForSeconds(3.0f);
        measureController.StartDiagnosis();
    }


    // JSON変換　※消しちゃダメ
    private void GetUserData(string val)
    {
        //Debug.Log(val);

        if(val.Length == 1){
            Debug.Log("アクティブなユーザーが設定されていません。");
        } else {
            UserData userData = JsonConvert.DeserializeObject<UserData>(val);
            //Debug.Log("userId : "+userData.user_id);
            //Debug.Log("name : " + userData.user_name);
            SetLabel(this.txtID, userData.user_id);
            SetLabel(this.txtName, userData.user_name);
        }
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) { birdTransfer.PlayStart(); }
        if (Input.GetKeyDown(KeyCode.Space)) { SetActive(objWarpEffect, true); }
    }

    private void FinishPushButton()
    {
        //measureController.StartDiagnosis();
        //ApiSetUserData();
        Camera.main.GetComponent<SceenFade>().LoadSceenWithFade("Game");

        //　ロード画面UIをアクティブにする
		//this.slider.gameObject.SetActive(true);
		//　コルーチンを開始
		//StartCoroutine("LoadData");
    }

    public void LoadMain()
    {
        StartCoroutine("CoroutineLoad");
    }

    IEnumerator CoroutineLoad()
    {

        SetActive(objWarpEffect, true);

        yield return new WaitForSeconds(8.0f);

        SetActive(objWarpEffect, false);
        GetSceneManagerLocal().MoveToGameScene();



        //　ロード画面UIをアクティブにする
        //this.slider.gameObject.SetActive(true);
        //　コルーチンを開始
        //StartCoroutine("LoadData");
    }

    private void FinishDiagnosis()
    {
        currentStatus = MENU_STATUS_ENUM.WAIT;
    }

    // TODO 実データで繋ぎなおし
    private void ApiSetUserData(){
        // サーバへPOSTするデータを設定 
        string url = "http://dev.rikuty.net/api/SetUserData.php";

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add("user_id", "1");
        dic.Add("max_rom_measure_1", "44");
        dic.Add("max_rom_measure_2", "44");
        dic.Add("max_rom_measure_3", "44");
        dic.Add("max_rom_measure_4", "44");
        dic.Add("max_rom_measure_5", "44");
        dic.Add("max_rom_measure_6", "44");
        dic.Add("max_rom_measure_7", "44");
        dic.Add("max_rom_measure_8", "44");
        dic.Add("pre_rest_pain", "5");
        dic.Add("pre_move_pain", "5");
        dic.Add("pre_move_fear", "5");

        StartCoroutine(HttpPost(url, dic));
    }


    public void ShowResult()
    {
        SetActive(objUI, false);
        SetActive(objResult, true);

        resultFormatArea.Init();
    }

 //   IEnumerator LoadData() {
	//	// シーンの読み込みをする
	//	async = SceneManager.LoadSceneAsync("Game");
 
	//	//　読み込みが終わるまで進捗状況をスライダーの値に反映させる
	//	while(!async.isDone) {
	//		var progressVal = Mathf.Clamp01(async.progress / 0.9f);
	//		slider.value = progressVal;
	//		yield return null;
	//	}
	//}
}