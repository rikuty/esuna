using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class GazeButtonInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	[SerializeField] private Image img;

	private float elapsedTime = 0f;

    private Context context;

	private enum COUNT_STATUS : int{
		NOT_SELECT,
		WAIT,
		COUNT
	}
	private COUNT_STATUS currentStatus = COUNT_STATUS.NOT_SELECT;

    // Event delegates triggered on click.
    [SerializeField]
    public UnityEvent m_OnClickGaze;

	private bool isPointerInside;

	// Use this for initialization
	void Start () {
		if(this.img == null){
			this.img = this.GetComponent<Image>();
		}
	}


    public void Init(Context context)
    {
        this.context = context;
    }

    // Update is called once per frame
    protected void Update () {


		if(this.isPointerInside){
			//Debug.Log("CCCCCCC");
			this.elapsedTime += Time.deltaTime;
			if(this.currentStatus == COUNT_STATUS.WAIT && this.elapsedTime > GameData.Instance.WAIT_TIME){
				this.currentStatus = COUNT_STATUS.COUNT;
				this.elapsedTime = 0f;
			}else if(this.currentStatus == COUNT_STATUS.COUNT && this.elapsedTime < GameData.Instance.COUNT_TIME){
				this.img.fillAmount = this.elapsedTime / GameData.Instance.COUNT_TIME;
			}else if(this.currentStatus == COUNT_STATUS.COUNT && this.elapsedTime >= GameData.Instance.COUNT_TIME){
				UISystemProfilerApi.AddMarker("Button.onClick", this);
                this.elapsedTime = 0f;
                this.img.fillAmount = 0f;
                this.currentStatus = COUNT_STATUS.NOT_SELECT;
				m_OnClickGaze.Invoke();
			}
		}else{
			if(this.img == null){
				this.img = this.GetComponent<Image>();
			}
			this.img.fillAmount = 0f;
            this.currentStatus = COUNT_STATUS.NOT_SELECT;
			this.elapsedTime = 0f;
		}


	}

	public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("AAAAAAAAAA");
        isPointerInside = true;
		this.currentStatus = COUNT_STATUS.WAIT;
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		//Debug.Log("BBBBBBBBBB");
		isPointerInside = false;
	}
		
}
