using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestResultApi : UtilComponent {
    // Use this for initialization
    void Start () {
        // サーバへPOSTするデータを設定 
        string url = "http://dev.rikuty.net/api/SetResultData.php";

        Dictionary<string, string> dic = new Dictionary<string, string>();
        
        dic.Add("user_id", "1");
        dic.Add("max_rom_exercise_1", "44");
        dic.Add("max_rom_exercise_2", "44");
        dic.Add("max_rom_exercise_3", "44");
        dic.Add("max_rom_exercise_4", "44");
        dic.Add("max_rom_exercise_5", "44");
        dic.Add("max_rom_exercise_6", "44");
        dic.Add("max_rom_exercise_7", "44");
        dic.Add("max_rom_exercise_8", "44");
        dic.Add("average_max_rom", "40");
        dic.Add("average_time_1", "10");
        dic.Add("average_time_2", "10");
        dic.Add("average_time_3", "10");
        dic.Add("average_time_4", "10");
        dic.Add("average_time_5", "10");
        dic.Add("average_time_6", "10");
        dic.Add("average_time_7", "10");
        dic.Add("average_time_8", "10");
        dic.Add("appraisal_value_1", "3");
        dic.Add("appraisal_value_2", "3");
        dic.Add("appraisal_value_3", "3");
        dic.Add("appraisal_value_4", "3");
        dic.Add("appraisal_value_5", "3");
        dic.Add("appraisal_value_6", "3");
        dic.Add("appraisal_value_7", "3");
        dic.Add("appraisal_value_8", "3");
        dic.Add("post_rest_pain", "5");
        dic.Add("post_move_pain", "5");
        dic.Add("post_move_fear", "5");
        dic.Add("point", "6666");
        dic.Add("rom_value", "33");
        dic.Add("point_value", "33");
		
        // string filePath = Application.dataPath + "/Resources/";
        // string fileName = "ResultSheet.png";
        // StartCoroutine(HttpPost(url, dic, filePath, fileName));

        StartCoroutine(HttpPost(url, dic));
    }
}