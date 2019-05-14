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
    [SerializeField] private List<Text> txtLabels; 

    [Header("Graph Propaty")]
    [SerializeField] private WMG_Radar_Graph graph1;
    [SerializeField] private WMG_Axis_Graph graph2;
    [SerializeField] private WMG_Axis_Graph graph3;
    [SerializeField] private Transform graph4P1; // 今回
    [SerializeField] private Transform graph4P2; // 前回
    [SerializeField] private WMG_Axis_Graph graph5;

    [Header("Series Propaty")]
    [SerializeField] private WMG_Series graph2_1;
    [SerializeField] private WMG_Series graph2_2;
    [SerializeField] private WMG_Series graph2_3;
    [SerializeField] private WMG_Series graph3_1;
    [SerializeField] private WMG_Series graph5_1;
        
    private UserData userData;

    void Awake () {
        StartCoroutine(ConnectAPI("http://dev.rikuty.net/api/GetFormatData.php", GetUserData));
    }

    // Use this for initialization
    void Start () {
    }

    private void GetUserData(string val) {
        //Debug.Log(val);
        if(val.Length == 1){
            Debug.Log("アクティブなユーザーが設定されていません。");
        } else {
            userData = JsonConvert.DeserializeObject<UserData>(val);

            // first data
            MeasureData firstData = userData.measure[1];

            SetLabel(this.txtDate, DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
            SetLabel(this.txtUserId, userData.user_id);

            SetLabel(this.txtUserName, userData.user_name);
            SetLabel(this.txtUserAge, userData.age);
            SetLabel(this.txtUserHeight, userData.height+"cm");
            
            SetLabel(this.txtPoint, firstData.point);
            SetLabel(this.txtRank, userData.rank);

            SettingGraph();
        }
    }

    private void SettingGraph(){
        
        // first data
        MeasureData firstData = userData.measure[1];

        graph1.SetGraph1ValueList(firstData.AppraisalValues());

        List<string> graphLabelVaues = firstData.GraphLabelVaues();
        for(int i = 0; i < txtLabels.Count; i++){
            txtLabels[i].text = graphLabelVaues[i];
        }

        for(int i=userData.measure.Count; i>=1; i--){
            float x = (float)(i);
            graph2_3.pointValues.Add(new Vector2(x, userData.measure[i].post_rest_pain));
            graph2_2.pointValues.Add(new Vector2(x, userData.measure[i].post_move_pain));
            graph2_1.pointValues.Add(new Vector2(x, userData.measure[i].post_move_fear));
            graph3_1.pointValues.Add(new Vector2(x, userData.measure[i].average_max_rom));
            graph5_1.pointValues.Add(new Vector2(x, userData.measure[i].point));
            
            DateTime measureDate = DateTime.ParseExact(userData.measure[i].measure_date, "yyyy-MM-dd HH:mm:ss", null);
            graph2.xAxis.axisLabels[userData.measure.Count - i] = measureDate.ToString("MM/dd");
            graph3.xAxis.axisLabels[userData.measure.Count - i] = measureDate.ToString("MM/dd");
            graph5.xAxis.axisLabels[userData.measure.Count - i] = measureDate.ToString("MM/dd");
        }
        graph2.xAxis.AxisNumTicks = userData.measure.Count;
        graph3.xAxis.AxisNumTicks = userData.measure.Count;
        graph5.xAxis.AxisNumTicks = userData.measure.Count;

        graph4P1.localPosition = new Vector3(userData.measure[1].rom_value * 5.3f, userData.measure[1].point_value * 3.85f, 0.0f);
        graph4P2.localPosition = new Vector3(userData.measure[2].rom_value * 5.3f, userData.measure[2].point_value * 3.85f, 0.0f);

        // Update Graph
        graph1.GraphChanged();
        graph2.GraphChanged();
        graph3.GraphChanged();
        graph5.GraphChanged();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) { SavePng(); }
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