using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdTransfer : MonoBehaviour
{

    [SerializeField] private Animator transferAnimator;
    [SerializeField] private Animator birdAnimator;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayStart()
    {
        transferAnimator.SetTrigger("StartTrigger");
    }

    void PlayFlying()
    {
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
    }

    void FlyingCallback()
    {
        Debug.Log("flying callback.");
    }
}
