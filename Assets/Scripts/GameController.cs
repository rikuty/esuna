using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

public class GameController : UtilComponent {

    public GameObject objDirectionalLight;

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

    [SerializeField] private AudioSource audioSourceVoice;
    [SerializeField] private List<AudioClip> tutorialVoiceList;

    private void Awake()
    {
        Cache.Initialize();
    }
    
    private void Start()
    {
        if (SceneManager.GetSceneAt(0).name == "Game")
        {
            SetActive(objDirectionalLight, true);
            Init();
        }
    }


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

        audioSourceVoice.clip = tutorialVoiceList[0];
        audioSourceVoice.Play();
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

        audioSourceVoice.clip = tutorialVoiceList[1];
        audioSourceVoice.Play();
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

        GetSceneManagerLocal().MoveToTitleSceneResult();

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

        Cache.user.MeasureData.SetMaxRomExercise(Cache.user.BodyScaleData.goalCurrentDic);

        StartCoroutine(ResultCoroutine());
    }


    IEnumerator ResultCoroutine()
    {
        yield return new WaitForSeconds(3.0f);

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

        dic.Add("user_id", Cache.user.UserData.user_id);
        dic.Add("max_rom_exercise_1", Cache.user.MeasureData.max_rom_exercise_1.ToString());
        dic.Add("max_rom_exercise_2", Cache.user.MeasureData.max_rom_exercise_2.ToString());
        dic.Add("max_rom_exercise_3", Cache.user.MeasureData.max_rom_exercise_3.ToString());
        dic.Add("max_rom_exercise_4", Cache.user.MeasureData.max_rom_exercise_4.ToString());
        dic.Add("max_rom_exercise_5", Cache.user.MeasureData.max_rom_exercise_5.ToString());
        dic.Add("max_rom_exercise_6", Cache.user.MeasureData.max_rom_exercise_6.ToString());
        dic.Add("max_rom_exercise_7", Cache.user.MeasureData.max_rom_exercise_7.ToString());
        dic.Add("max_rom_exercise_8", Cache.user.MeasureData.max_rom_exercise_8.ToString());
        dic.Add("average_max_rom", Cache.user.MeasureData.average_max_rom.ToString());
        dic.Add("average_time_1", Cache.user.MeasureData.average_time_1.ToString());
        dic.Add("average_time_2", Cache.user.MeasureData.average_time_2.ToString());
        dic.Add("average_time_3", Cache.user.MeasureData.average_time_3.ToString());
        dic.Add("average_time_4", Cache.user.MeasureData.average_time_4.ToString());
        dic.Add("average_time_5", Cache.user.MeasureData.average_time_5.ToString());
        dic.Add("average_time_6", Cache.user.MeasureData.average_time_6.ToString());
        dic.Add("average_time_7", Cache.user.MeasureData.average_time_7.ToString());
        dic.Add("average_time_8", Cache.user.MeasureData.average_time_8.ToString());
        dic.Add("appraisal_value_1", Cache.user.MeasureData.appraisal_value_1.ToString());
        dic.Add("appraisal_value_2", Cache.user.MeasureData.appraisal_value_2.ToString());
        dic.Add("appraisal_value_3", Cache.user.MeasureData.appraisal_value_3.ToString());
        dic.Add("appraisal_value_4", Cache.user.MeasureData.appraisal_value_4.ToString());
        dic.Add("appraisal_value_5", Cache.user.MeasureData.appraisal_value_5.ToString());
        dic.Add("appraisal_value_6", Cache.user.MeasureData.appraisal_value_6.ToString());
        dic.Add("appraisal_value_7", Cache.user.MeasureData.appraisal_value_7.ToString());
        dic.Add("appraisal_value_8", Cache.user.MeasureData.appraisal_value_8.ToString());
        dic.Add("post_rest_pain", Cache.user.MeasureData.post_rest_pain.ToString());
        dic.Add("post_move_pain", Cache.user.MeasureData.post_move_pain.ToString());
        dic.Add("post_move_fear", Cache.user.MeasureData.post_move_fear.ToString());
        dic.Add("point", Cache.user.MeasureData.point.ToString());
        dic.Add("rom_value", Cache.user.MeasureData.rom_value.ToString());
        dic.Add("point_value", Cache.user.MeasureData.point_value.ToString());

        StartCoroutine(HttpPost(url, dic));
    }
}
