using System.Collections;
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

    [SerializeField] private GameObject avatar;

    [SerializeField] HandController handController;


    [SerializeField] private CountDownComponent cdComponent;

    [SerializeField] private GameObject objStart;
    [SerializeField] private GameObject objCountDown;
    [SerializeField] private GameObject objPlay;
    [SerializeField] private GameObject objResult;


    [SerializeField] private AudioSource audioSource;
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


    private void Awake()
    {
        context.currentStatus = DEFINE_APP.STATUS_ENUM.PREPARE;
        StartCoroutine("PrepareCoroutine");
    }

    IEnumerator PrepareCoroutine()
    {
        yield return new WaitForSeconds(3.0f);
        context.currentStatus = DEFINE_APP.STATUS_ENUM.START;
        answerController.SetGravity(false);
    }

    // Use this for initialization
    private void Start () {

        panelButtonComponent.Init(()　=> { Debug.Log("OKOK");/*ここにキューブを押した後の処理を記述*/});

        answerController.Init(CallbackFromAnswerControllers, context, handController);
        handController.Init(CallbackFromHandCanGrab, CallbackFromHandGrabbing, context);
        //this.context.Init();
        this.answerController.InstantiateNewEgg(DEFINE_APP.ANSWER_TYPE_ENUM.START);

        //this.gazeButtonInput.Init(this.context);

        SetActive(this.objCountDown, false);
        SetActive(this.objPlay, false);
        SetActive(this.objResult, false);

        //resultModalPresenter = ResourceLoader.Instance.Create<ResultModalPresenter>("Prefabs/ResultModal", trResult, false);

        if (this.audioSource != null)
        {
            this.audioSource.Play();
        }
	}

    private void CallbackFromHandCanGrab()
    {
        answerController.SetGravity(true);
    }

    private void CallbackFromHandGrabbing()
    {
        //answerController.SetActiveNest(true);
    }

    private void CallbackFromAnswerControllers(DEFINE_APP.ANSWER_TYPE_ENUM answerType)
    {
        switch(answerType)
        {
            case DEFINE_APP.ANSWER_TYPE_ENUM.START:
                this.context.currentStatus = DEFINE_APP.STATUS_ENUM.COUNT;
                StartCoroutine(this.SetCountDown());
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
                this.context.AddGamePoint();
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
                Reload();
                break;

        }
    }


    private IEnumerator SetCountDown(){
        //Debug.Log("CountDown");

        yield return new WaitForSeconds(2);

        this.cdComponent.Init(3.0f, this.FinishCountDown, false);
        
        SetActive(this.objStart, false);
        SetActive(this.objCountDown, true);
        SetActive(this.objPlay, false);
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
            case DEFINE_APP.STATUS_ENUM.START:
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
        SetActive(this.objPlay, true);


	}


    private void UpdatePlay(){
        this.context.AddPlayTime(Time.deltaTime);

        SetLabel(leftTime, context.leftPlayTime.ToString("F0"));

        if(!this.context.isPlay){
            this.context.currentStatus = DEFINE_APP.STATUS_ENUM.FINISH;
            //this.context.Finish();
            return;
        }
        //SetLabel(this.curretSpeed, this.context.answerTime.ToString("F2"));
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

        this.context.isAnswering = false;


        this.context.currentStatus = DEFINE_APP.STATUS_ENUM.SHOW_RESLUT;

        ResultCoroutine();
    }

    private void UpdateShowResult(){
        
    }

    IEnumerator ResultCoroutine()
    {
        yield return new WaitForSeconds(10.0f);

        Reload();
    }


    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
