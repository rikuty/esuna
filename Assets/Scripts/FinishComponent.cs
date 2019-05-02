using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FinishComponent : MonoBehaviour {

    [SerializeField] private Animator animator;

    Action callback;

    void Start()
    {
        animator.SetTrigger("FinishTrigger");

    }

    // Use this for initialization
    public void Init(Action callback )
    {
        this.callback = callback;

    }

    private void FinishCallback(){
        this.callback();
    }
}
