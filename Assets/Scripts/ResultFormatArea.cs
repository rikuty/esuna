using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResultFormatArea : MonoBehaviour {

    public RenderTexture RenderTextureRef;

    // Use this for initialization
    void Start () {
        //Debug.Log("path : "+Application.dataPath);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            SavePng(); 
        }
    }

    void SavePng()
    {
        //Debug.Log("SavePng");
        Texture2D tex = new Texture2D(RenderTextureRef.width, RenderTextureRef.height, TextureFormat.RGB24, false);
        RenderTexture.active = RenderTextureRef;
        tex.ReadPixels(new Rect(0, 0, RenderTextureRef.width, RenderTextureRef.height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);

        //Write to a file in the project folder
        File.WriteAllBytes(Application.dataPath + "/Resources/Images/ResultSheet.png", bytes);

    }
}
