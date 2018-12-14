using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class CountDownComponent : UtilComponent {

	#region Variable Defines
	///	<summary>初期化済みフラグ</summary>
	private bool isInitialized = false;
	///	<summary>カウントダウン終了済みフラグ</summary>
	private bool isPast = false;

	private bool isPlayStart = false;

	///	<summary>カウントダウン終了済みフラグ</summary>
	private float timeStartTxt = 3.0f;
	private float rate = 10.0f;

	///	<summary>カウントする秒数</summary>
	private float count;

	///	<summary>経過時間</summary>
	private float elapsedTime = 0f;

	///	<summary>残り時間</summary>
	private float leftTime {
		get {
			return count - elapsedTime;
		}
	}

	///	<summary>コールバック</summary>
	private System.Action callbackFinish = null;

	[SerializeField] Text txtCountdown;

	#endregion Variable Defines



	private void Update (){
		//	初期化処理を通していない場合は処理をしない。また、終了している場合も処理をしない
		if (!this.isInitialized) {
			return;
		}
		this.elapsedTime += Time.deltaTime;
		if(!this.isPast){
			this.Calculate ();
		}else if (this.isPast && this.timeStartTxt > -1f * this.leftTime && this.isPlayStart){
			this.StartText();
		}else{

		}
	}

	public void Init(float second, System.Action callbackFinish, bool isPlayStart){
        this.count = second;
		this.callbackFinish = callbackFinish;
		this.isInitialized = true;
		this.isPlayStart = isPlayStart;
		this.callbackFinish = callbackFinish;
        this.isPast = false;
        this.elapsedTime = 0f;

		this.SetCountDownText ();
	}

	private void Calculate(){
		//	現在時刻が終了時刻に到達した場合は処理を終了
		if(leftTime<0){
			this.isPast = true;
			if (this.callbackFinish != null) {
				this.callbackFinish ();
			}
			return;
		}
		this.SetCountDownText ();
	}
		
	private void SetCountDownText(){
		SetLabel (this.txtCountdown, Mathf.Ceil(this.leftTime).ToString());
	}

	private void StartText(){

		//	現在時刻が終了時刻に到達した場合は処理を終了
		SetLabel(this.txtCountdown, "START");
//		float rate = this.rate * ((-1f * this.leftTime) / this.timeStartTxt);
//		Debug.Log("rate"+rate.ToString());
//		this.txtCountdown.gameObject.transform.localScale = Vector3.one * rate;
	}
}
