﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Gamestrap;


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
    public PanelButtonComponent panelButtonComponent;

    public MeasureController measureController;

    private GameData gameData;

    public void Start()
    {

        this.gameData = GameData.Instance;

        this.dateTime = DateTime.Now;
        this.userID = this.dateTime.ToString("yyMMddHHmm");
        SetLabel(this.txtID, this.userID);
        SetLabel(this.txtName, this.userName);
        SetLabel(this.txtTitle, "");
        SetLabel(this.txtDetail, "目の前にある青い箱を押してください");

        //handController.Init(LoadMain);
        panelButtonComponent.Init(FinishPushButton);

        measureController.Init(LoadMain);

        currentStatus = MENU_STATUS_ENUM.WAIT;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { Camera.main.GetComponent<SceenFade>().LoadSceenWithFade("Main"); }
    }


    private void FinishPushButton()
    {
        measureController.StartDiagnosis();
    }

    public void LoadMain()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        Camera.main.GetComponent<SceenFade>().LoadSceenWithFade("Main");
    }


    private void FinishDiagnosis()
    {
        currentStatus = MENU_STATUS_ENUM.WAIT;
    }
}