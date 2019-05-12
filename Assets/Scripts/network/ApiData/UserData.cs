using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable] 
public class UserData 
{
	public string user_id;
	public string user_name;
	public int age;
	public int height;
	public int rank;
	public Dictionary<int, MeasureData> measure;
}