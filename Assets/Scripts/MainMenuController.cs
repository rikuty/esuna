using UnityEngine;
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

    public HandController handController;

    private GameData gameData;

    public void Start()
    {

        this.gameData = GameData.Instance;

        this.dateTime = DateTime.Now;
        this.userID = this.dateTime.ToString("yyMMddHHmm");
        SetLabel(this.txtID, this.userID);
        SetLabel(this.txtName, this.userName);

        handController.Init(LoadMain);

    }

    public void LoadMain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}