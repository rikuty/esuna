using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using Gamestrap;

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class SceneManagerLocal : UtilComponent
{
    bool isInit = false;

    GameObject[] objsGameScene;
    GameObject[] objsTitleScene;

    GameController gameController;
    MainMenuController mainMenuController;


    private void Awake()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
        objsGameScene = new GameObject[0];
        objsTitleScene = new GameObject[0];
    }


    private void Update()
    {
        if (objsGameScene == null || objsTitleScene == null) return;

        if (!isInit && objsGameScene.Length > 0 && objsTitleScene.Length > 0)
        {
            isInit = true;
            Scene sceneGame = SceneManager.GetSceneByName("Game");
            objsGameScene = sceneGame.GetRootGameObjects();
            MoveToTitleScene();

        }

        if (objsGameScene.Length == 0)
        {
            Scene sceneGame = SceneManager.GetSceneByName("Game");
            objsGameScene = sceneGame.GetRootGameObjects();
            gameController = objsGameScene[0].GetComponent<GameController>();
        }

        if (objsTitleScene.Length == 0)
        {
            Scene sceneTitle = SceneManager.GetSceneByName("Title");
            objsTitleScene = sceneTitle.GetRootGameObjects();
            mainMenuController = objsTitleScene[0].GetComponent<MainMenuController>();

        }
    }

    public void  MoveToGameScene() {

        if (objsGameScene.Length > 0)
        {
            SetActive(objsTitleScene, false);
            SetActive(objsGameScene, true);
            gameController.Init();
        }
    }

    public void MoveToTitleScene()
    {
        if (objsTitleScene.Length > 0)
        {
            SetActive(objsTitleScene, true);
            SetActive(objsGameScene, false);
            mainMenuController.Init();
        }
    }


    public void MoveToTitleSceneResult()
    {
        if (objsTitleScene.Length > 0)
        {
            SetActive(objsTitleScene, true);
            SetActive(objsGameScene, false);
            mainMenuController.ShowResult();
        }
    }
}