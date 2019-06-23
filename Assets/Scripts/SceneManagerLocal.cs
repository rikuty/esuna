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
	private static SceneManagerLocal instance = null;

	public static SceneManagerLocal Instance {
		get {
			if (instance == null) {
				GameObject sceneManagerObj = GameObject.Find("SceneManage");
				Debug.Assert(sceneManagerObj != null, "SceneManageオブジェクトが見つかりません。");
				instance = sceneManagerObj.GetComponent<SceneManagerLocal>();
				DontDestroyOnLoad(sceneManagerObj);
			}
			return instance;
		}
	}


	private bool isTransition = false;


	private void Awake()
	{
		this.SetAsyncLoad("Title");
		this.Transition();
	}

	private IEnumerator AsyncLoad(string sceneName, string[] args = null)
	{
		Scene scene = SceneManager.GetSceneByName(sceneName);

		AsyncOperation asyncOperation = null;
		if (!scene.isLoaded) {
			asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			asyncOperation.allowSceneActivation = false;
		}

		while (!this.isTransition) {
			yield return null;
		}

		// TODO: ローダーを表示させたい場合はここでアクティブ化

		if (asyncOperation != null) {
			while (!asyncOperation.isDone) {
				Debug.LogError("[SceneManagerLocal] ロード中〜");
				yield return null;
			}
			asyncOperation.allowSceneActivation = true;
		}

		this.isTransition = false;

		scene = SceneManager.GetSceneByName(sceneName);
		GameObject[] sceneObjects = scene.GetRootGameObjects();

		foreach (GameObject obj in sceneObjects) {
			SceneBase sceneBase = obj.GetComponent<SceneBase>();
			if (sceneBase != null) {
				sceneBase.Init(args ?? new string[0]);
				break;
			}
		}
	}


	public void SetAsyncLoad(string sceneName, string[] args = null)
	{
		StartCoroutine(this.AsyncLoad(sceneName, args));
	}

	public void Transition()
	{
		this.isTransition = true;
	}






	/*
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
            if (objsGameScene.Length == 0) return;

            gameController = objsGameScene[0].GetComponent<GameController>();
        }

        if (objsTitleScene.Length == 0)
        {
            Scene sceneTitle = SceneManager.GetSceneByName("Title");
            objsTitleScene = sceneTitle.GetRootGameObjects();
            if (objsTitleScene.Length == 0) return;
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
            mainMenuController.FinishTraining();
        }
    }
    */
}