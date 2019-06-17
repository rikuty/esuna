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


	public Vector3 HeadPos {
		get {
			return new Vector3(0f, this.sitting_height, 0f);
		}
		set {
			this.sitting_height = value.y;
		}
	}

	public Vector3 HandPosL {
		get {
			return new Vector3(this.left_hand_x, this.left_hand_y, this.left_hand_z);
		}
		set {
			this.left_hand_x = value.x;
			this.left_hand_y = value.y;
			this.left_hand_z = value.z;
		}
	}
	public Vector3 HandPosR {
		get {
			return new Vector3(this.right_hand_x, this.right_hand_y, this.right_hand_z);
		}
		set {
			this.right_hand_x = value.x;
			this.right_hand_y = value.y;
			this.right_hand_z = value.z;
		}
	}


	public UserData()
	{
		// デフォルト値をセット
		this.left_hand_x = DEFINE_APP.DEFAULT_HAND_POS_L.x;
		this.left_hand_y = DEFINE_APP.DEFAULT_HAND_POS_L.y;
		this.left_hand_z = DEFINE_APP.DEFAULT_HAND_POS_L.z;
		this.right_hand_x = DEFINE_APP.DEFAULT_HAND_POS_R.x;
		this.right_hand_y = DEFINE_APP.DEFAULT_HAND_POS_R.y;
		this.right_hand_z = DEFINE_APP.DEFAULT_HAND_POS_R.z;
	}
}