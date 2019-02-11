using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownComponet : MonoBehaviour {

    [SerializeField] private Animator animator;

	// Use this for initialization
	void Start () {
        animator.SetTrigger("CountDownTrigger");
	}
	
    private void CountDownCallback(){
        Debug.Log("callback.");
    }
}
