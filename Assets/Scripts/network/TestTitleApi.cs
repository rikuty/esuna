using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestTitleApi : UtilComponent {
    // Use this for initialization
    void Start () {
        // サーバへPOSTするデータを設定 
        string url = "http://dev.rikuty.net/api/SetUserData.php";

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add("user_id", "1");
        dic.Add("max_rom_measure_1", "44");
        dic.Add("max_rom_measure_2", "44");
        dic.Add("max_rom_measure_3", "44");
        dic.Add("max_rom_measure_4", "44");
        dic.Add("max_rom_measure_5", "44");
        dic.Add("max_rom_measure_6", "44");
        dic.Add("max_rom_measure_7", "44");
        dic.Add("max_rom_measure_8", "44");
        dic.Add("pre_rest_pain", "5");
        dic.Add("pre_move_pain", "5");
        dic.Add("pre_move_fear", "5");
		
        StartCoroutine(HttpPost(url, dic));
    }
}