using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CountdownComponet : MonoBehaviour {

    [SerializeField] private Animator animator;

    Action callback;

	// Use this for initialization
	void Start () {
        animator.SetTrigger("CountDownTrigger");

    }

    public void Init(Action callback )
    {
        this.callback = callback;

    }

    private void CountDownCallback(){
        this.callback();
    }
}
