using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetUserData : MonoBehaviour {
    // POSTする対象のURL
    string url = "http://dev.rikuty.net/api/SetUserData.php";
    // タイムアウト時間
    float timeoutsec = 10f;

    // Use this for initialization
    void Start () {
        // サーバへPOSTするデータを設定 
        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add("user_id", "1");
        dic.Add("last_name", "山田");
        dic.Add("first_name", "太郎");
        dic.Add("age", "20");

        StartCoroutine(HttpPost(url, dic));
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator HttpPost(string url, Dictionary<string, string> post)
    {
        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, string> post_arg in post)
        {
        	Debug.Log(post_arg.Key+", "+post_arg.Value);
        	form.AddField(post_arg.Key, post_arg.Value);
        }
        WWW www = new WWW(url, form);

        yield return StartCoroutine(CheckTimeOut(www, timeoutsec));

        if (www.error != null)
        {
            Debug.Log("HttpPost NG: " + www.error);
        }
        else if (www.isDone)
        {
            Debug.Log("HttpPost OK: " + www.text);
        }
    }

    // HTTPリクエストのタイムアウト処理
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