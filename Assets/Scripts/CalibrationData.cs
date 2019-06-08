using System.Collections.Generic;
using UnityEngine;

public class CalibrationData
{
	public Dictionary<int, float> maxRomMeasure;

	public CalibrationData()
	{
		this.maxRomMeasure = new Dictionary<int, float>();
		foreach (int index in DEFINE_APP.BODY_SCALE.ROT_AXIS.Keys) {
			this.maxRomMeasure.Add(index, 0f);
		}
	}
}