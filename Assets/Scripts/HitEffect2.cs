using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class HitEffect2 : MonoBehaviour {

    // Debug 
    [SerializeField] private Transform lookTarget;

    [SerializeField] private Transform trBird;
    [SerializeField] private Animator birdAnimator;

    int rotateYValues = 0;
    double currentYValues;
    bool rotateFlg = false;
    bool moveUpFlg = false;

    double movePosX = 0;
    double movePosZ = 0;

    float moveUpValue = 4.0f;
    float moveUpFrame = 180.0f;

    // Use this for initialization
    void Start () {
        trBird.LookAt(lookTarget);
        currentYValues = trBird.localEulerAngles.y;
    }
	
	// Update is called once per frame
	void Update () {
        if(rotateFlg && rotateYValues < 180){
            rotateYValues += 6;
            this.transform.localRotation = Quaternion.Euler(trBird.localRotation.x, rotateYValues, trBird.localRotation.z);
            //Debug.Log("rotateYValues : "+rotateYValues);

            if(rotateYValues == 180){
                rotateFlg = false;
            }
        }

        if(moveUpFlg){
            double targetAngles = 0;

            if(180 <= currentYValues && currentYValues < 270){
                targetAngles = currentYValues * (Math.PI / 180);
            } else if (90 <= currentYValues && currentYValues < 180) {
                targetAngles = (180 - currentYValues) * (Math.PI / 180);
            } else if (0 <= currentYValues && currentYValues < 90) {
                targetAngles = (currentYValues - 180) * (Math.PI / 180);
            } else if (270 <= currentYValues && currentYValues < 360) {
                targetAngles = (360 - currentYValues) * (Math.PI / 180);
            }
            
            movePosX = Math.Cos(targetAngles) / moveUpFrame * moveUpValue;
            movePosZ = Math.Sin(targetAngles) / moveUpFrame * moveUpValue;

            float unitPosX = this.transform.localPosition.x + (float)movePosX;
            float unitPosY = this.transform.localPosition.y + moveUpValue / moveUpFrame;
            float unitPosZ = this.transform.localPosition.z + (float)movePosZ;

            this.transform.localPosition = new Vector3(unitPosX, unitPosY, unitPosZ);

        }
    }

    void SetFlyingStatus(int isFly) {
        bool isFlying = (isFly == 1) ? true : false;
        birdAnimator.SetBool("flying", isFlying);
        rotateFlg = true;
    }

    void SetMoveUpStatus(int isUp){
        moveUpFlg = (isUp == 1) ? true : false;
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
        trBird.rotation = Quaternion.identity;
    }
}
