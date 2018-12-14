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


	public enum STATUS_ENUM : int{
		START,
		COUNT,
		PLAY,
        FINISH,
        SHOW_RESLUT
	}
    private STATUS_ENUM currentStatus = STATUS_ENUM.START;

    [SerializeField] AnswerController answerController;

    [SerializeField] private GameObject avatar;


    [SerializeField] private CountDownComponent cdComponent;

    [SerializeField] private GameObject objStart;
    [SerializeField] private GameObject objCountDown;
    [SerializeField] private GameObject objPlay;


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

    //[SerializeField] private ResultModalPresenter resultModalPresenter;


    //[SerializeField] private StartObject startObject;

	// Use this for initialization
	private void Start () {
        //this.answerController.Init(this.context, CallbackCut);
        //this.sordCotroller.Init(this.context);

        //startObject = ResourceLoader.Instance.Create<StartObject>("Prefabs/CubeStart", trStart);
        //this.startObject.Init(this.context, CallBackStartCut);
        //startObject.cutEvent += CallBackStartCut;


        answerController.Init(CallbackFromAnswerControllers);
        //this.context.Init();

        //this.gazeButtonInput.Init(this.context);

        SetActive(this.objCountDown, false);
        SetActive(this.objPlay, false);

        //resultModalPresenter = ResourceLoader.Instance.Create<ResultModalPresenter>("Prefabs/ResultModal", trResult, false);

        if (this.audioSource != null)
        {
            this.audioSource.Play();
        }
	}

    
    private void CallbackFromAnswerControllers(DEFINE_APP.ANSWER_TYPE_ENUM answerType)
    {
        if(answerType == DEFINE_APP.ANSWER_TYPE_ENUM.START)
        {
            this.currentStatus = STATUS_ENUM.COUNT;
            StartCoroutine(this.SetCountDown());
        }
        else
        {

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

        switch(this.currentStatus){
            case STATUS_ENUM.START:
                this.UpdateStart();
    			break;
    		case STATUS_ENUM.COUNT:
    			//this.UpdateCount();
    			break;
    		case STATUS_ENUM.PLAY:
    			this.UpdatePlay();
    			break;
            case STATUS_ENUM.FINISH:
                this.UpdateFinish();
                break;
            case STATUS_ENUM.SHOW_RESLUT:
                this.UpdateShowResult();
                break;
		}
	}
	

	private void UpdateStart(){
	}


	private void FinishCountDown(){
		//this.cdCountDown.Init(3f, ()=>
        //{
        this.currentStatus = STATUS_ENUM.PLAY;
        //this.context.StartPlay();
        //this.answerController.Init(this.context);
        //this.answerController.SetAnswers();

        //CallSwitchInvoke();
        //Invoke("CallSwitchInvoke", 0.5f);

        this.answerController.InstantiateNewCube();

        SetActive(this.objStart, false);
        SetActive(this.objCountDown, false);
        SetActive(this.objPlay, true);


	}


    private void UpdatePlay(){
        this.context.AddPlayTime(Time.deltaTime);

        SetLabel(leftTime, context.leftPlayTime.ToString("F0"));

        if(!this.context.isPlay){
            this.currentStatus = STATUS_ENUM.FINISH;
            //this.context.Finish();
            return;
        }
        //SetLabel(this.curretSpeed, this.context.answerTime.ToString("F2"));
	}

    private void UpdateFinish()
    {
        //ResultModalModel model = 
        //    new ResultModalModel(this.context.averageTime,
        //                         this.context);

        //resultModalPresenter.Show(model);
        this.currentStatus = STATUS_ENUM.SHOW_RESLUT;
        //SetActive(this.objStart, true);
        //SetActive(this.objPlay, false);
        ClickedFinish();

    }

    private void UpdateShowResult(){
        
    }


    private void ClickedFinish()
    {
        //Gamestrap.GSAppExampleControl.Instance.LoadScene(Gamestrap.ESceneNames.scene_title);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
