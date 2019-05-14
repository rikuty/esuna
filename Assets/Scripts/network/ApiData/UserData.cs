using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable] 
public class UserData 
{
	public string user_id;
	public string facility_id;
	public string user_name;
	public int age;
	public int height;
	public int gender; //性別（1 : 男, 0 : 女）
	public float sitting_height;
	public float left_hand_x;
	public float left_hand_y;
	public float left_hand_z;
	public float right_hand_x;
	public float right_hand_y;
	public float right_hand_z;
	public int max_pt;
	public int experience;
	public int rank;
	public Dictionary<int, MeasureData> measure;
}