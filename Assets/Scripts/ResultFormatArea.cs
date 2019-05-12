using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

public class ResultFormatArea : UtilComponent {

    public RenderTexture RenderTextureRef;
    [Header("Text Propaty")]
    [SerializeField] private Text txtDate; 
    [SerializeField] private Text txtUserId; 
    [SerializeField] private Text txtUserName; 
    [SerializeField] private Text txtUserAge; 
    [SerializeField] private Text txtUserHeight; 
    [SerializeField] private Text txtPoint;
    [SerializeField] private Text txtRank; 

    [Header("Graph Propaty")]
    [SerializeField] private WMG_Radar_Graph graph1;
    [SerializeField] private WMG_Axis_Graph graph2;
    [SerializeField] private WMG_Axis_Graph graph3;
    [SerializeField] private Transform graph4P1;
    [SerializeField] private Transform graph4P2;
    [SerializeField] private WMG_Axis_Graph graph5;

    [Space(10)]
    [Header("Data Propaty")]
	[SerializeField] private List<float> graph1ValueList;

	[SerializeField] private List<float> graph2_1ValueList;
    [SerializeField] private WMG_Series graph2_1;
	[SerializeField] private List<float> graph2_2ValueList;
    [SerializeField] private WMG_Series graph2_2;
	[SerializeField] private List<float> graph2_3ValueList;
    [SerializeField] private WMG_Series graph2_3;

    [SerializeField] private WMG_Series graph3_1;
	[SerializeField] private List<float> graph3ValueList;

    [SerializeField] private Vector2 graph4NowValue;
    [SerializeField] private Vector2 graph4PreValue;

    [SerializeField] private WMG_Series graph5_1;
	[SerializeField] private List<float> graph5ValueList;
        
    private UserData userData;

    void Awake () {
        StartCoroutine(ConnectAPI("http://dev.rikuty.net/api/GetFormatData.php", GetUserData));
    }

    // Use this for initialization
    void Start () {
        //Debug.Log("path : "+Application.dataPath);
        //StartCoroutine(ConnectAPI("http://dev.rikuty.net/api/GetFormatData.php", GetUserData));
/* 
        // Graph1 Setting
        graph1.SetGraph1ValueList(graph1ValueList);

        // Graph2 Setting
        for(int i=0; i<graph2_1ValueList.Count; i++){
            float x = (float)(i+1);
            graph2_1.pointValues.Add(new Vector2(x, graph2_1ValueList[i]));
        }
        for(int i=0; i<graph2_2ValueList.Count; i++){
            float x = (float)(i+1);
            graph2_2.pointValues.Add(new Vector2(x, graph2_2ValueList[i]));
        }
        for(int i=0; i<graph2_3ValueList.Count; i++){
            float x = (float)(i+1);
            graph2_3.pointValues.Add(new Vector2(x, graph2_3ValueList[i]));
        }
        graph2.xAxis.AxisNumTicks = graph2_1ValueList.Count;

        // Graph3 Setting
        for(int i=0; i<graph3ValueList.Count; i++){
            float x = (float)(i+1);
            graph3_1.pointValues.Add(new Vector2(x, graph3ValueList[i]));
        }
        graph3.xAxis.AxisNumTicks = graph3ValueList.Count;

        // Graph4 Setting
        //graph4P1.localPosition = new Vector3(graph4NowValue.x * 5.3f, graph4NowValue.y * 3.85f, 0.0f);
        //graph4P2.localPosition = new Vector3(graph4PreValue.x * 5.3f, graph4PreValue.y * 3.85f, 0.0f);
        //Debug.Log(userData.measure[1].rom_value+" : "+userData.measure[1].point_value);
        //graph4P1.localPosition = new Vector3(userData.measure[1].rom_value * 5.3f, userData.measure[1].point_value * 3.85f, 0.0f);
        //graph4P2.localPosition = new Vector3(userData.measure[2].rom_value * 5.3f, userData.measure[2].point_value * 3.85f, 0.0f);

        // Graph5 Setting
        for(int i=0; i<graph5ValueList.Count; i++){
            float x = (float)(i+1);
            graph5_1.pointValues.Add(new Vector2(x, graph5ValueList[i]));
        }
        graph5.xAxis.AxisNumTicks = graph5ValueList.Count;    
*/
    }

    private void GetUserData(string val) {
        //Debug.Log(val);
        if(val.Length == 1){
            Debug.Log("アクティブなユーザーが設定されていません。");
        } else {
            userData = JsonConvert.DeserializeObject<UserData>(val);

            // first data
            MeasureData firstData = userData.measure[1];
            //Debug.Log(firstData.point);

            SetLabel(this.txtDate, DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
            SetLabel(this.txtUserId, userData.user_id);

            SetLabel(this.txtUserName, userData.user_name);
            SetLabel(this.txtUserAge, userData.age);
            SetLabel(this.txtUserHeight, userData.height);
            
            SetLabel(this.txtPoint, firstData.point);
            SetLabel(this.txtRank, userData.rank);

            SettingGraph();
        }
    }

    private void SettingGraph(){
        //Debug.Log("SettingGraph");
        // Graph1 Setting
        graph1.SetGraph1ValueList(graph1ValueList);

        // Graph2 Setting
/* 
        for(int i=0; i<graph2_1ValueList.Count; i++){
            float x = (float)(i+1);
            graph2_1.pointValues.Add(new Vector2(x, graph2_1ValueList[i]));
        }
        for(int i=0; i<graph2_2ValueList.Count; i++){
            float x = (float)(i+1);
            graph2_2.pointValues.Add(new Vector2(x, graph2_2ValueList[i]));
        }
        for(int i=0; i<graph2_3ValueList.Count; i++){
            float x = (float)(i+1);
            graph2_3.pointValues.Add(new Vector2(x, graph2_3ValueList[i]));
        }
        graph2.xAxis.AxisNumTicks = graph2_1ValueList.Count;
*/
        // Graph3 Setting
/* 
        for(int i=0; i<graph3ValueList.Count; i++){
            float x = (float)(i+1);
            graph3_1.pointValues.Add(new Vector2(x, graph3ValueList[i]));
        }
        graph3.xAxis.AxisNumTicks = graph3ValueList.Count;
        
        for(int i=1; i<=userData.measure.Count; i++){
            float x = (float)(i);
            graph3_1.pointValues.Add(new Vector2(x, userData.measure[i].average_max_rom));
        }
        graph3.xAxis.AxisNumTicks = userData.measure.Count;
*/
        // Graph4 Setting
        //graph4P1.localPosition = new Vector3(graph4NowValue.x * 5.3f, graph4NowValue.y * 3.85f, 0.0f);
        //graph4P2.localPosition = new Vector3(graph4PreValue.x * 5.3f, graph4PreValue.y * 3.85f, 0.0f);
        graph4P1.localPosition = new Vector3(userData.measure[1].rom_value * 5.3f, userData.measure[1].point_value * 3.85f, 0.0f);
        graph4P2.localPosition = new Vector3(userData.measure[2].rom_value * 5.3f, userData.measure[2].point_value * 3.85f, 0.0f);

        // Graph5 Setting
/* 
        for(int i=0; i<graph5ValueList.Count; i++){
            float x = (float)(i+1);
            graph5_1.pointValues.Add(new Vector2(x, graph5ValueList[i]));
        }
        graph5.xAxis.AxisNumTicks = graph5ValueList.Count;
        
        for(int i=1; i<=userData.measure.Count; i++){
            float x = (float)(i);
            graph5_1.pointValues.Add(new Vector2(x, userData.measure[i].point));
        }
        graph5.xAxis.AxisNumTicks = userData.measure.Count;
*/
        for(int i=userData.measure.Count; i>=1; i--){
            float x = (float)(i);
            graph2_1.pointValues.Add(new Vector2(x, userData.measure[i].post_rest_pain));
            graph2_2.pointValues.Add(new Vector2(x, userData.measure[i].post_move_pain));
            graph2_3.pointValues.Add(new Vector2(x, userData.measure[i].post_move_fear));
            graph3_1.pointValues.Add(new Vector2(x, userData.measure[i].average_max_rom));
            graph5_1.pointValues.Add(new Vector2(x, userData.measure[i].point));
        }
        graph2.xAxis.AxisNumTicks = userData.measure.Count;
        graph3.xAxis.AxisNumTicks = userData.measure.Count;
        graph5.xAxis.AxisNumTicks = userData.measure.Count;
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
        Destroy(tex);

        //Write to a file in the project folder
        //Debug.Log(Application.dataPath);
        File.WriteAllBytes(Application.dataPath + "/Resources/ResultSheet.png", bytes);

    }
}
