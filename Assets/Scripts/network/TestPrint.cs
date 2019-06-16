using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

public class RefreshTokenData 
{
	public string access_token;
}

public class TestPrint : UtilComponent {

	enum PRINT_STATUS{
        FIRST_STEP = 0, //初期状態
        GET_TOKEN = 1,
        PRINT_SEARCH = 2,
        PRINTING = 3,
		WAIT = 99,
    }
	string resText;

	PRINT_STATUS printStatus = PRINT_STATUS.GET_TOKEN;

	private RefreshTokenData refreshTokenData = null;

	// Use this for initialization
    void Start () {
    }

	void Update () {
		switch (printStatus)
        {

            case PRINT_STATUS.FIRST_STEP:
				printStatus = PRINT_STATUS.WAIT;
                //GetAccessToken();
                break;
            case PRINT_STATUS.GET_TOKEN:
				printStatus = PRINT_STATUS.WAIT;
                GetAccessToken();
                break;
            case PRINT_STATUS.PRINT_SEARCH:
				printStatus = PRINT_STATUS.WAIT;
                PrintSearch();
                break;
            case PRINT_STATUS.PRINTING:
				printStatus = PRINT_STATUS.WAIT;
                //GetAccessToken();
                break;
            case PRINT_STATUS.WAIT:
                return;
        }
	}

	void GetAccessToken(){
        // サーバへPOSTするデータを設定 
        string url = "https://www.googleapis.com/oauth2/v4/token";

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add("refresh_token", "1/Nj1U7D8iwEjlWw9maeRmTlopKrQnlClxrzv1lip-sd4");
        dic.Add("client_id", "761889929248-hec2iu2qn5ae35qefm1ji0htu0lmhjmd.apps.googleusercontent.com");
        dic.Add("client_secret", "F3Zva_8sFjFUiuHtjWnD-8Cc");
        dic.Add("grant_type", "refresh_token");
		
        StartCoroutine(GetAccessTokenHttpPost(url, dic));
	}

	void PrintSearch(){
        // サーバへPOSTするデータを設定 
        string url = "https://www.google.com/cloudprint/search";
		
        StartCoroutine(PrintSearchHttpPost(url, null));
	}

	IEnumerator GetAccessTokenHttpPost(string url, Dictionary<string, string> post)
    {
        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, string> post_arg in post)
        {
        	//Debug.Log(post_arg.Key+", "+post_arg.Value);
        	form.AddField(post_arg.Key, post_arg.Value);
        }
        WWW www = new WWW(url, form);

        yield return StartCoroutine(CheckTimeOut(www, 10f));

        if (www.error != null)
        {
            Debug.Log("HttpPost NG: " + www.error);
        }
        else if (www.isDone)
        {
            Debug.Log("HttpPost OK: " + www.text);
			refreshTokenData = JsonConvert.DeserializeObject<RefreshTokenData>(www.text);
			Debug.Log("access token : "+refreshTokenData.access_token);
			printStatus = PRINT_STATUS.PRINT_SEARCH;
        }
    }

	IEnumerator PrintSearchHttpPost(string url, Dictionary<string, string> post)
    {
        WWWForm form = new WWWForm();
		/* 
        foreach (KeyValuePair<string, string> post_arg in post)
        {
        	//Debug.Log(post_arg.Key+", "+post_arg.Value);
        	form.AddField(post_arg.Key, post_arg.Value);
        }
		*/

		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("Authorization", "OAuth " + refreshTokenData.access_token);

        WWW www = new WWW(url, null, headers);

        yield return StartCoroutine(CheckTimeOut(www, 10f));

        if (www.error != null)
        {
            Debug.Log("HttpPost NG: " + www.error);
        }
        else if (www.isDone)
        {
            Debug.Log("HttpPost OK: " + www.text);
			//refreshTokenData = JsonConvert.DeserializeObject<RefreshTokenData>(www.text);
			//Debug.Log("access token : "+refreshTokenData.access_token);
			//printStatus = PRINT_STATUS.PRINT_SEARCH;
        }
    }

	IEnumerator CheckTimeOut(WWW www, float timeout)
    {
        float requestTime = Time.time;

        while (!www.isDone)
        {
            if (Time.time - requestTime < timeout)
                yield return null;
            else
            {
                Debug.Log("TimeOut"); 
                break;
            }
        }
        yield return null;
    }
}
