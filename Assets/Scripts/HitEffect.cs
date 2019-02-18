using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour {

    [SerializeField] private Animator hitAnimator;
    [SerializeField] private Animator birdAnimator;


    // Use this for initialization
    void Start () {
        hitAnimator.SetTrigger("HitTrigger");
    }
	
	// Update is called once per frame
	void Update () {

    }

    void SetFlyingStatus(int isFly) {
        bool isFlying = (isFly == 1) ? true : false;
        birdAnimator.SetBool("flying", isFlying);
    }

    void HitCallback()
    {
        Debug.Log("callback.");
    }
}
