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
public class PrintStatusData
{
    public List<PrinterData> printers;
}
public class PrinterData
{
    public string id; 
}

public class SubmitResponse
{
    public string success;
    public string message;
}

public class TestPrint : UtilComponent {

    public Texture2D tImage;

	enum PRINT_STATUS{
        FIRST_STEP = 0, //初期状態
        GET_TOKEN = 1,
        PRINT_SEARCH = 2,
        PRINTING = 3,
        FINISH = 9,
		WAIT = 99,
    }
	string resText;

	PRINT_STATUS printStatus = PRINT_STATUS.GET_TOKEN;

	private RefreshTokenData refreshTokenData = null;
    private PrintStatusData printStatusData = null;
    private SubmitResponse submitResponse = null;

    string printerId = null;

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
                PrintProcess();
                break;
            case PRINT_STATUS.WAIT:
                return;
        }
	}

	void GetAccessToken(){
        string url = "https://www.googleapis.com/oauth2/v4/token";

        WWWForm form = new WWWForm();
        form.AddField("refresh_token", "1/Nj1U7D8iwEjlWw9maeRmTlopKrQnlClxrzv1lip-sd4");
        form.AddField("client_id", "761889929248-hec2iu2qn5ae35qefm1ji0htu0lmhjmd.apps.googleusercontent.com");
        form.AddField("client_secret", "F3Zva_8sFjFUiuHtjWnD-8Cc");
        form.AddField("grant_type", "refresh_token");

        StartCoroutine(GetAccessTokenHttpPost(url, form));
	}

	void PrintSearch(){
        string url = "https://www.google.com/cloudprint/search";
		
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("Authorization", "OAuth " + refreshTokenData.access_token);
        
        StartCoroutine(PrintSearchHttpPost(url, headers));
	}

    void PrintProcess(){
        string url = "https://www.google.com/cloudprint/submit";

        WWWForm form = new WWWForm();

        //byte[] imageData = File.ReadAllBytes(Application.dataPath + "/Resources/ResultSheet.png");
        //byte[] txtData = File.ReadAllBytes(Application.dataPath + "/Resources/test.txt");
        //byte[] imageData = tImage.EncodeToPNG();
        //byte[] pdfData = File.ReadAllBytes(Application.dataPath + "/Resources/ResultSheet.pdf");

        form.AddField("printerid", printerId);
        form.AddField("title", "Sample05");
        form.AddField("contentType", "url");
        form.AddField("content", "https://dev.rikuty.net/image.php");
        //form.AddField("contentType", "image/png");
        //form.AddField("content", System.Text.Encoding.Unicode.GetString(imageData));
        //form.AddBinaryData("content", pdfData, "ResultSheet.png", "application/pdf");
        //form.AddBinaryData("content", txtData, "test.txt", "text/plain");
        //form.AddBinaryData("content", imageData);
        form.AddField("ticket", "{'version':'1.0','print':{'vendor_ticket_item':[],'color':{'type':'STANDARD_COLOR'},'copies':{'copies':1}}}");

		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("Authorization", "OAuth " + refreshTokenData.access_token);
		//headers.Add("Content-Type", "image/png");

        StartCoroutine(PrintProcessHttpPost(url, form.data, headers));
    }

	IEnumerator GetAccessTokenHttpPost(string url, WWWForm form)
    {
        WWW www = new WWW(url, form);

        yield return StartCoroutine(CheckTimeOut(www, 10f));

        if (www.error != null) {
            Debug.Log("HttpPost NG: " + www.error);
        } else if (www.isDone) {
            //Debug.Log("HttpPost OK: " + www.text);
			refreshTokenData = JsonConvert.DeserializeObject<RefreshTokenData>(www.text);
			Debug.Log("access token : "+refreshTokenData.access_token);
			printStatus = PRINT_STATUS.PRINT_SEARCH;
        }
    }

	IEnumerator PrintSearchHttpPost(string url, Dictionary<string, string> headers)
    {
        WWW www = new WWW(url, null, headers);

        yield return StartCoroutine(CheckTimeOut(www, 10f));

        if (www.error != null) {
            Debug.Log("HttpPost NG: " + www.error);
        } else if (www.isDone) {
            //Debug.Log("HttpPost OK: " + www.text);
			printStatusData = JsonConvert.DeserializeObject<PrintStatusData>(www.text);
			//Debug.Log("printStatusData[0] : "+printStatusData.printers[0].id);
			//Debug.Log("printStatusData[1] : "+printStatusData.printers[1].id);

            if(printStatusData.printers[0].id == "__google__docs"){
                printerId = printStatusData.printers[1].id;
            } else {
                printerId = printStatusData.printers[0].id;
            }
			Debug.Log("printerId : "+printerId);

			printStatus = PRINT_STATUS.PRINTING;
        }
    }

	IEnumerator PrintProcessHttpPost(string url, byte[] postData, Dictionary<string, string> headers)
    {
        WWW www = new WWW(url, postData, headers);

        yield return StartCoroutine(CheckTimeOut(www, 30f));

        if (www.error != null) {
            Debug.Log("HttpPost NG: " + www.error);
        } else if (www.isDone) {
            //Debug.Log("HttpPost OK: " + www.text);
			submitResponse = JsonConvert.DeserializeObject<SubmitResponse>(www.text);
            Debug.Log("success: " + submitResponse.success);
            Debug.Log("message: " + submitResponse.message);
			printStatus = PRINT_STATUS.FINISH;
        }
    }

	IEnumerator CheckTimeOut(WWW www, float timeout)
    {
        float requestTime = Time.time;

        while (!www.isDone) {
            if (Time.time - requestTime < timeout) {
                yield return null;
            } else {
                Debug.Log("TimeOut"); 
                break;
            }
        }
        yield return null;
    }
}
