using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdrestController : MonoBehaviour {

	[SerializeField] private List<LittleBirdAnimationController> birdList;
	// Use this for initialization

	private int currentBirdIndex = 0;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) { 
			LittleBirdAnimationController currentBird = birdList[currentBirdIndex];
			currentBirdIndex++;

			currentBird.gameObject.SetActive(true);
		}
	}
}
