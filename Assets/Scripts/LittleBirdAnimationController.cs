using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LittleBirdAnimationController : MonoBehaviour {

	[SerializeField] private Animator littleBirdAnimator;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void SetAction(){

		// Z座標の初期化
		this.transform.localPosition = new Vector3(this.transform.localPosition.x, 0, 0);

		int actionNumber = UnityEngine.Random.Range(1, 6);
		littleBirdAnimator.SetInteger("idleStatus", actionNumber);
	}
}
