using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResultFormatArea : MonoBehaviour {

    public RenderTexture RenderTextureRef;
    [SerializeField] private WMG_Radar_Graph graph1;
    [SerializeField] private WMG_Axis_Graph graph2;
    [SerializeField] private WMG_Axis_Graph graph3;
    [SerializeField] private WMG_Axis_Graph graph5;

    [SerializeField] private Transform graph4P1;
    [SerializeField] private Transform graph4P2;

    #region Data
	[SerializeField] private List<float> graph2_1ValueList;
    [SerializeField] private WMG_Series graph2_1;
	[SerializeField] private List<float> graph2_2ValueList;
    [SerializeField] private WMG_Series graph2_2;
	[SerializeField] private List<float> graph2_3ValueList;
    [SerializeField] private WMG_Series graph2_3;

	[SerializeField] private List<float> graph1ValueList;
    [SerializeField] private WMG_Series graph3_1;
	[SerializeField] private List<float> graph3ValueList;
    [SerializeField] private WMG_Series graph5_1;
	[SerializeField] private List<float> graph5ValueList;
        
    #endregion

    // Use this for initialization
    void Start () {
        //Debug.Log("path : "+Application.dataPath);
        graph1.SetGraph1ValueList(graph1ValueList);

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

        for(int i=0; i<graph3ValueList.Count; i++){
            float x = (float)(i+1);
            graph3_1.pointValues.Add(new Vector2(x, graph3ValueList[i]));
        }
        graph3.xAxis.AxisNumTicks = graph3ValueList.Count;

        for(int i=0; i<graph5ValueList.Count; i++){
            float x = (float)(i+1);
            graph5_1.pointValues.Add(new Vector2(x, graph5ValueList[i]));
        }
        graph5.xAxis.AxisNumTicks = graph5ValueList.Count;
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
        //Debug.Log(Application.dataPath);
        File.WriteAllBytes(Application.dataPath + "/Resources/ResultSheet.png", bytes);

    }
}
