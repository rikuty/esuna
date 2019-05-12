using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable] 
public class MeasureData 
{
	public string measure_id;
	public string user_id;
	public string measure_date;
	public float average_max_rom;
	public int post_rest_pain;
	public int post_move_pain;
	public int post_move_fear;
	public int point;
	public int rom_value;
	public int point_value;
}