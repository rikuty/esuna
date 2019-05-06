using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenDialogSample : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Return))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			GameObject.Find("OkCancelDialog").GetComponent<OKCANCELDIALOG.OkCancelDialog>().ShowDialog("スペースキーが押されました。");
		}
	}
}
