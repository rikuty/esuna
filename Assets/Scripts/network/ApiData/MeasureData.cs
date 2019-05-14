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
	public float max_rom_measure_1;
	public float max_rom_measure_2;
	public float max_rom_measure_3;
	public float max_rom_measure_4;
	public float max_rom_measure_5;
	public float max_rom_measure_6;
	public float max_rom_measure_7;
	public float max_rom_measure_8;
	public float max_rom_exercise_1;
	public float max_rom_exercise_2;
	public float max_rom_exercise_3;
	public float max_rom_exercise_4;
	public float max_rom_exercise_5;
	public float max_rom_exercise_6;
	public float max_rom_exercise_7;
	public float max_rom_exercise_8;
	public float average_max_rom;
	public float average_time_1;
	public float average_time_2;
	public float average_time_3;
	public float average_time_4;
	public float average_time_5;
	public float average_time_6;
	public float average_time_7;
	public float average_time_8;
	public float appraisal_value_1;
	public float appraisal_value_2;
	public float appraisal_value_3;
	public float appraisal_value_4;
	public float appraisal_value_5;
	public float appraisal_value_6;
	public float appraisal_value_7;
	public float appraisal_value_8;
	public int pre_rest_pain;
	public int pre_move_pain;
	public int pre_move_fear;
	public int post_rest_pain;
	public int post_move_pain;
	public int post_move_fear;
	public int point;
	public int rom_value;
	public int point_value;

	public List<float> AppraisalValues()
	{
		return new List<float>(){
		appraisal_value_1,
		appraisal_value_2,
		appraisal_value_3,
		appraisal_value_4,
		appraisal_value_5,
		appraisal_value_6,
		appraisal_value_7,
		appraisal_value_8
		};
	}

	public List<string> GraphLabelVaues()
	{
		return new List<string>(){
			max_rom_exercise_1.ToString()+"°/"+average_time_1.ToString()+"s",
			max_rom_exercise_2.ToString()+"°/"+average_time_2.ToString()+"s",
			max_rom_exercise_3.ToString()+"°/"+average_time_3.ToString()+"s",
			max_rom_exercise_4.ToString()+"°/"+average_time_4.ToString()+"s",
			max_rom_exercise_5.ToString()+"°/"+average_time_5.ToString()+"s",
			max_rom_exercise_6.ToString()+"°/"+average_time_6.ToString()+"s",
			max_rom_exercise_7.ToString()+"°/"+average_time_7.ToString()+"s",
			max_rom_exercise_8.ToString()+"°/"+average_time_8.ToString()+"s"
		};
	}
}