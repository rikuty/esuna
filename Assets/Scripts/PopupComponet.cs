using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupComponet : MonoBehaviour {

    [SerializeField] private Animator animator;

	// Use this for initialization
	void Start () {
        animator.SetTrigger("PopupTrigger");
	}
	
    private void PopupCallback(){
        Debug.Log("callback.");
    }
}
