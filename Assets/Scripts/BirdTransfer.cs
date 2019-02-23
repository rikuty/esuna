using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdTransfer : MonoBehaviour
{

    [SerializeField] private Animator transferAnimator;
    [SerializeField] private Animator birdAnimator;

    [SerializeField] private Camera OVRCamera;
    [SerializeField] private Transform birdParent;
    [SerializeField] private Transform trackingParent;

    Action callback = null;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(Action callback)
    {
        this.callback = callback;
    }

    public void PlayStart()
    {
        transferAnimator.SetTrigger("StartTrigger");
    }

    public void PlayFlying()
    {
        //カメラスイッチ
        OVRCamera.transform.parent = birdParent;
        transferAnimator.SetTrigger("FlyingTrigger");
    }

    void SetHitTrigger()
    {
        birdAnimator.SetTrigger("Hit");
    }

    void SetSitDownTrigger()
    {
        birdAnimator.SetTrigger("SitDown");
    }

    void SetSoarTrigger()
    {
        birdAnimator.SetTrigger("Soar");
    }

    void SetLandingStatus(int isLanding)
    {
        bool isLand = (isLanding == 1) ? true : false;
        birdAnimator.SetBool("Landing", isLand);
    }

    void SetForward(float value)
    {
        birdAnimator.SetFloat("Forward", value);
    }

    void SetTurn(float value)
    {
        birdAnimator.SetFloat("Turn", value);
    }

    void StartCallback()
    {
        Debug.Log("start callback.");
        if (this.callback != null)
        {
            this.callback();
        }
    }

    void FlyingCallback()
    {
        //カメラスイッチ
        OVRCamera.transform.parent = trackingParent;
        Debug.Log("flying callback.");
        if (this.callback != null)
        {
            this.callback();
        }
    }
}
