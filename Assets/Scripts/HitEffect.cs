using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour {

    [SerializeField] private Transform trBird;
    [SerializeField] private Animator birdAnimator;

    // Use this for initialization
    void Start () {
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
        //Debug.Log("callback.");
        Destroy(this.gameObject);
    }

    public void SetLook(Transform target){
        trBird.LookAt(target);
        float vectorY = trBird.localRotation.eulerAngles.y;
        //Debug.Log("vectorY : "+ vectorY);
        trBird.localRotation = Quaternion.Euler(30.0f, vectorY, 0.0f);
    }
}
