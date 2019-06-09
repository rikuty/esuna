using UnityEngine;

public static class UtilMain
{
	public static Quaternion ToQuaternion(int direction, float angle, string bodyPart)
	{
//		angle *= DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX_RATIO[direction][bodyPart];
		return Quaternion.AngleAxis(angle, DEFINE_APP.BODY_SCALE.ROT_AXIS[direction][bodyPart]);
	}
}