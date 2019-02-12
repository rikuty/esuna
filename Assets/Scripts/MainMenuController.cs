﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Gamestrap;


public class MainMenuController : UtilComponent
{

    private string userID;
    private string userName;
    private DateTime dateTime;
    public Text txtID;
    public Text txtName;

    //public HandController handController;
    public PanelButtonComponent panelButtonComponent;

    private GameData gameData;

    public void Start()
    {

        this.gameData = GameData.Instance;

        this.dateTime = DateTime.Now;
        this.userID = this.dateTime.ToString("yyMMddHHmm");
        SetLabel(this.txtID, this.userID);
        SetLabel(this.txtName, this.userName);

        //handController.Init(LoadMain);
        panelButtonComponent.Init(LoadMain);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { Camera.main.GetComponent<SceenFade>().LoadSceenWithFade("Main"); }
    }

    public void LoadMain()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        Camera.main.GetComponent<SceenFade>().LoadSceenWithFade("Main");
    }
}