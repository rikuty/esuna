using UnityEngine;
using System.Collections;

public class SecretarybirdUserController : MonoBehaviour {

    SecretarybirdCharacter secretarybirdCharacter;
    public float upDownInputSpeed = 3f;
	float forwardMultiplier=.25f;
	float forwardSpeed=.25f;

    void Start()
    {
        secretarybirdCharacter = GetComponent<SecretarybirdCharacter>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            secretarybirdCharacter.Attack();
        }

        if (Input.GetButtonDown("Jump"))
        {
            secretarybirdCharacter.Soar();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            secretarybirdCharacter.Hit();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            secretarybirdCharacter.Eat();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            secretarybirdCharacter.Death();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            secretarybirdCharacter.Rebirth();
        }

		if (Input.GetKeyDown(KeyCode.Z))
		{
			forwardSpeed=.25f;
		}

		if (Input.GetKeyDown(KeyCode.X))
		{
			forwardSpeed=.5f;
		}

		if (Input.GetKeyDown(KeyCode.C))
		{
			forwardSpeed=1f;
		}


		if (Input.GetKeyDown(KeyCode.B))
		{
			secretarybirdCharacter.SitDown();
		}
		
		if (Input.GetKeyDown(KeyCode.Y))
		{
			secretarybirdCharacter.StandUp();
		}



        if (Input.GetKey(KeyCode.N))
        {
            secretarybirdCharacter.upDown = Mathf.Clamp(secretarybirdCharacter.upDown - Time.deltaTime * upDownInputSpeed, -1f, 1f);
        }

        if (Input.GetKey(KeyCode.U))
        {
            secretarybirdCharacter.upDown = Mathf.Clamp(secretarybirdCharacter.upDown + Time.deltaTime * upDownInputSpeed, -1f, 1f);
        }

    }

    private void FixedUpdate()
    {
		forwardMultiplier = Mathf.Lerp (forwardMultiplier, forwardSpeed, Time.deltaTime);

        secretarybirdCharacter.turn = Input.GetAxis("Horizontal");
		float v = Input.GetAxis ("Vertical");
        secretarybirdCharacter.forward=v*forwardMultiplier;
		if(v<.1f){
			forwardSpeed=.25f;
			forwardMultiplier=.25f;
		}
    }
}
