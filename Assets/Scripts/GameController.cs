﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

public class GameController : UtilComponent {


	//public CountDownComponent cdCountDown;

    private Context _context;
    public Context context{
        get{
            if(this._context == null){
                this._context = new Context();
            }
            return this._context;
        }
    }

    [SerializeField] PanelButtonComponent panelButtonComponent;
    [SerializeField] AnswerController answerController;


    [SerializeField] HandController handController;

    [SerializeField] BirdTransfer birdTransfer;

    [SerializeField] private CountdownComponet cdComponent;
    [SerializeField] private FinishComponent finishComponent;

    [SerializeField] private GameObject[] objStart;
    [SerializeField] private GameObject[] objTutorial;
    [SerializeField] private GameObject[] objCountDown;
    [SerializeField] private GameObject[] objPlay;
    [SerializeField] private GameObject[] objResult;

    [SerializeField] private GameObject[] objNestAndEgg;
    [SerializeField] private GameObject objGuide;


    [SerializeField] private AudioSource audioSourceForest;

    [SerializeField] private AudioSource audioSourceGame;

    //[SerializeField] private AudioClip audioClip;
    //[SerializeField] private AnswerObjectController answerController;
    [SerializeField] private Text leftTime;


    //[SerializeField] private Text numMinus;
    //[SerializeField] private Text correctCount;
    //[SerializeField] private Text time;

    //[SerializeField] private SordCotroller sordCotroller;


    //[SerializeField] private Text x;
    //[SerializeField] private Text y;
    //[SerializeField] private Text z;

    [SerializeField] private ResultModalPresenter resultModalPresenter;
    


    public void Init()
    {
        context.currentStatus = DEFINE_APP.STATUS_ENUM.PREPARE;

        audioSourceGame.Stop();
        audioSourceForest.Play();


        answerController.Init(CallbackFromAnswerControllers, context, handController);
        handController.Init(CallbackFromHandRelease, CallbackFromHandGrabbing, context);


        this.answerController.InstantiateNewEgg(DEFINE_APP.ANSWER_TYPE_ENUM.TUTORIAL);
        context.currentStatus = DEFINE_APP.STATUS_ENUM.TUTORIAL;
        answerController.SetGravity(false);
        SetActive(this.objStart, false);
        SetActive(this.objTutorial, true);
        SetActive(this.objNestAndEgg, true);

        if (this.audioSourceGame != null)
        {
            this.audioSourceGame.Play();
        }
    }

    private void CallbackFromHandRelease()
    {
        answerController.SetGravity(true);
        answerController.SetActiveNest(false);
        answerController.ResetCoinController();
        //answerController.SetTransformTarget();
    }


    private void CallbackFromHandGrabbing()
    {
        //answerController.SetActiveNest(true);
    }

    private void CallbackFromAnswerControllers(DEFINE_APP.ANSWER_TYPE_ENUM answerType)
    {
        switch(answerType)
        {
            case DEFINE_APP.ANSWER_TYPE_ENUM.TUTORIAL:
                this.context.currentStatus = DEFINE_APP.STATUS_ENUM.COUNT;
                StartCoroutine(this.SetCountDown());
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
                this.context.AddGamePoint();
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
                BackToTitle();
                break;

        }
    }


    private IEnumerator SetCountDown(){
        //Debug.Log("CountDown");
        audioSourceForest.Stop();

        yield return new WaitForSeconds(2);

        this.cdComponent.Init(this.FinishCountDown);
        SetActive(this.cdComponent, true);
        
        SetActive(this.objStart, false);
        SetActive(this.objCountDown, true);
        SetActive(this.objNestAndEgg, false);
        SetActive(this.objPlay, false);
        SetActive(this.objResult, false);
    }

    // Update is called once per frame
    private void Update () {
        OVRInput.Controller activeController = OVRInput.GetActiveController();
        Quaternion rot = OVRInput.GetLocalControllerRotation(activeController);
        //SetLabel(this.x, rot.eulerAngles.x.ToString());
        //SetLabel(this.y, rot.eulerAngles.y.ToString());
        //SetLabel(this.z, rot.eulerAngles.z.ToString());
        //Debug.Log("x:" + this.x.text);
        //Debug.Log("y:" + this.y.text);
        //Debug.Log("z:" + this.z.text);
        //if (Input.GetKey(KeyCode.S))
        //{
        //    this.CallBackStartCut(0);
        //    //this.context.SetLongSord(false);
        //}

        switch(this.context.currentStatus){
            case DEFINE_APP.STATUS_ENUM.TUTORIAL:
                this.UpdateStart();
    			break;
    		case DEFINE_APP.STATUS_ENUM.COUNT:
    			//this.UpdateCount();
    			break;
    		case DEFINE_APP.STATUS_ENUM.PLAY:
    			this.UpdatePlay();
    			break;
            case DEFINE_APP.STATUS_ENUM.FINISH:
                this.UpdateFinish();
                break;
            case DEFINE_APP.STATUS_ENUM.SHOW_RESLUT:
                this.UpdateShowResult();
                break;
		}
	}
	

	private void UpdateStart(){
	}


	private void FinishCountDown(){
		//this.cdCountDown.Init(3f, ()=>
        //{
        this.context.currentStatus = DEFINE_APP.STATUS_ENUM.PLAY;
        //this.context.StartPlay();
        //this.answerController.Init(this.context);
        //this.answerController.SetAnswers();

        //CallSwitchInvoke();
        //Invoke("CallSwitchInvoke", 0.5f);

        this.answerController.InstantiateNewEgg(DEFINE_APP.ANSWER_TYPE_ENUM.PLAY);

        SetActive(this.objStart, false);
        SetActive(this.objCountDown, false);
        SetActive(this.objNestAndEgg, true);
        SetActive(this.objPlay, true);

        SetActive(audioSourceGame.gameObject, true);
        audioSourceGame.Play();



    }


    private void UpdatePlay(){
        this.context.AddPlayTime(Time.deltaTime);

        SetLabel(leftTime, context.leftPlayTime.ToString("F0"));

        if(!this.context.isPlay){

            //this.context.currentStatus = DEFINE_APP.STATUS_ENUM.FINISH;
            //this.context.Finish();
            SetActive(this.objPlay, false);
            SetActive(this.objNestAndEgg, false);
            SetActive(this.objGuide, false);
            this.context.isAnswering = false;

            this.ShowFinish();
            audioSourceGame.Stop();

            return;
        }
        //SetLabel(this.curretSpeed, this.context.answerTime.ToString("F2"));
	}

    private void ShowFinish() {
        this.finishComponent.Init(this.ShowFinishCallback);
        SetActive(this.finishComponent.gameObject, true);
    }

    private void ShowFinishCallback() {
        SetActive(this.finishComponent.gameObject, false);
        this.context.currentStatus = DEFINE_APP.STATUS_ENUM.FINISH;
    }

    private void UpdateFinish()
    {
        ResultModalModel model = 
            new ResultModalModel(this.context);

        resultModalPresenter.Show(model);
        this.context.currentStatus = DEFINE_APP.STATUS_ENUM.SHOW_RESLUT;
        SetActive(this.objResult, true);
        SetActive(this.objPlay, false);
        //this.answerController.InstantiateNewEgg(DEFINE_APP.ANSWER_TYPE_ENUM.RESULT);
        SetActive(this.objNestAndEgg, false);
        SetActive(this.objGuide, false);

        this.context.isAnswering = false;


        this.context.currentStatus = DEFINE_APP.STATUS_ENUM.SHOW_RESLUT;

        StartCoroutine(ResultCoroutine());
    }

    private void UpdateShowResult(){
        
    }

    IEnumerator ResultCoroutine()
    {
        yield return new WaitForSeconds(7.0f);

        BackToTitle();
    }


    public void BackToTitle()
    {
        GetSceneManagerLocal().MoveToTitleScene();
    }



    // TODO 実データで繋ぎなおし
    private void ApiSetResultData(){
        // サーバへPOSTするデータを設定 
        string url = "http://dev.rikuty.net/api/SetResultData.php";

        Dictionary<string, string> dic = new Dictionary<string, string>();
        
        dic.Add("user_id", "1");
        dic.Add("max_rom_exercise_1", "44");
        dic.Add("max_rom_exercise_2", "44");
        dic.Add("max_rom_exercise_3", "44");
        dic.Add("max_rom_exercise_4", "44");
        dic.Add("max_rom_exercise_5", "44");
        dic.Add("max_rom_exercise_6", "44");
        dic.Add("max_rom_exercise_7", "44");
        dic.Add("max_rom_exercise_8", "44");
        dic.Add("average_max_rom", "40");
        dic.Add("average_time_1", "10");
        dic.Add("average_time_2", "10");
        dic.Add("average_time_3", "10");
        dic.Add("average_time_4", "10");
        dic.Add("average_time_5", "10");
        dic.Add("average_time_6", "10");
        dic.Add("average_time_7", "10");
        dic.Add("average_time_8", "10");
        dic.Add("appraisal_value_1", "3");
        dic.Add("appraisal_value_2", "3");
        dic.Add("appraisal_value_3", "3");
        dic.Add("appraisal_value_4", "3");
        dic.Add("appraisal_value_5", "3");
        dic.Add("appraisal_value_6", "3");
        dic.Add("appraisal_value_7", "3");
        dic.Add("appraisal_value_8", "3");
        dic.Add("post_rest_pain", "5");
        dic.Add("post_move_pain", "5");
        dic.Add("post_move_fear", "5");
        dic.Add("point", "6666");
        dic.Add("rom_value", "33");
        dic.Add("point_value", "33");

        StartCoroutine(HttpPost(url, dic));
    }
}
