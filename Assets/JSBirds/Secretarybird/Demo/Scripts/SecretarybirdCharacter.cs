using UnityEngine;
using System.Collections;

public class SecretarybirdCharacter : MonoBehaviour {
    Animator secretarybirdAnimator;
    public float groundCheckDistance = 0.1f;
    public float flyingGroundCheckDistance = .5f;
    public float groundCheckOffset = 0.01f;
    public float maxFlyTurnSpeed = .3f;
    public bool isGrounded;
	public bool leftFootIsGrounded;
	public bool rightFootIsGrounded;
    public float upDown = 0f;
    public bool soaring = false;
    public bool isFlying = false;
	public float forwardSpeed=0f;
	public float forward=0f;
	public float turn=0f;
	public GameObject leftFoot;
	public GameObject rightFoot;	
	public float maxForwardSpeed=3f;
	public float meanForwardSpeed=1.5f;
	public float speedDumpingTime=.1f;
	float timeFromSoar=0f;
	public float soaringTime=3f;

	Rigidbody secretarybirdRigid;

    void Start()
    {
        secretarybirdAnimator = GetComponent<Animator>();
        secretarybirdRigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
		Move ();	
        CheckGroundStatus();
		if (soaring) {
			timeFromSoar+=Time.deltaTime;
			if(timeFromSoar>soaringTime){
				soaring=false;
			}
		}
    }

    public void Attack()
    {
        secretarybirdAnimator.SetTrigger("Attack");
    }

    public void Hit()
    {
        secretarybirdAnimator.SetTrigger("Hit");
    }

    public void Eat()
    {
        secretarybirdAnimator.SetTrigger("Eat");
    }

    public void Death()
    {
        secretarybirdAnimator.SetTrigger("Death");
    }

    public void Rebirth()
    {
        secretarybirdAnimator.SetTrigger("Rebirth");
    }

	public void SitDown()
	{
		secretarybirdAnimator.SetTrigger("SitDown");
	}
	
	public void StandUp()
	{
		secretarybirdAnimator.SetTrigger("StandUp");
	}

    public void Soar()
    {
        if (isGrounded && !soaring && forward>.4f)
        {

			float runCycle=Mathf.Repeat(secretarybirdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1);
			if(runCycle<.25||runCycle>.75){
				secretarybirdAnimator.SetBool("SoarR", true);
			}else{
				secretarybirdAnimator.SetBool("SoarR", false);
			}
            secretarybirdAnimator.SetTrigger("Soar");
            secretarybirdAnimator.SetBool("Landing", false);
            isFlying = true;
			soaring=true;

			timeFromSoar=0f;
            secretarybirdAnimator.applyRootMotion = false;
			secretarybirdRigid.useGravity=false;
			forwardSpeed=forward;
        }
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
        if (isFlying)
        {
            leftFootIsGrounded = Physics.Raycast(leftFoot.transform.position + (Vector3.up * groundCheckOffset), Vector3.down, out hitInfo, flyingGroundCheckDistance);
			rightFootIsGrounded = Physics.Raycast(rightFoot.transform.position + (Vector3.up * groundCheckOffset), Vector3.down, out hitInfo, flyingGroundCheckDistance);

		}
        else
        {
			leftFootIsGrounded = Physics.Raycast(leftFoot.transform.position + (Vector3.up * groundCheckOffset), Vector3.down, out hitInfo, groundCheckDistance);
			rightFootIsGrounded = Physics.Raycast(rightFoot.transform.position + (Vector3.up * groundCheckOffset), Vector3.down, out hitInfo, groundCheckDistance);
		}

		isGrounded = leftFootIsGrounded || rightFootIsGrounded;

        if (isGrounded)
        {
            if (!soaring)
            {
                secretarybirdAnimator.applyRootMotion = true;
				if (isFlying)
				{
					secretarybirdAnimator.SetBool("Landing", true);
					isFlying = false;
					secretarybirdAnimator.applyRootMotion = true;
					secretarybirdRigid.useGravity=true;
				}
            }
        }
        else
        {
            secretarybirdAnimator.applyRootMotion = false;
        }
    }

    public void Move()
    {
        secretarybirdAnimator.SetFloat("Forward", forward);
        secretarybirdAnimator.SetFloat("Turn", turn);
        secretarybirdAnimator.SetFloat("UpDown", upDown);
        upDown = Mathf.Lerp(upDown, 0, Time.deltaTime * 3f);

        if (isFlying)
        {
			if(forward<0f){
				secretarybirdRigid.velocity=transform.up*upDown*2f+transform.forward*forwardSpeed;	
			}else if(forward>0f){
				secretarybirdRigid.velocity=transform.up*(upDown*2f+(forwardSpeed-meanForwardSpeed))+transform.forward*forwardSpeed;				
			}else{
				secretarybirdRigid.velocity=transform.up*(upDown*2f+(forwardSpeed-maxForwardSpeed))+transform.forward*forwardSpeed;
			}			
			transform.RotateAround(transform.position,Vector3.up,Time.deltaTime*turn*100f);
			forwardSpeed=Mathf.Lerp(forwardSpeed,Mathf.Min(meanForwardSpeed,forwardSpeed),Time.deltaTime*speedDumpingTime);
			forwardSpeed=Mathf.Clamp(forwardSpeed+forward*Time.deltaTime,0f,maxForwardSpeed);
        }
    }
}
