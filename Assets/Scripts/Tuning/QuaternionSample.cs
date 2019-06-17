#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class QuaternionSample : MonoBehaviour
{
	[SerializeField] private int direction;
	[SerializeField] private int bodyPart;
	[SerializeField] private Vector3 axis;
	[SerializeField] private float angle;
	[SerializeField] private float rotationSpeed = 1f;

	[Header("回転軸表示用")]
	[SerializeField] private Transform objAxis;
	[SerializeField] private Transform objLook;


	private List<int> directionList {
		get {
			return new List<int>(DEFINE_APP.BODY_SCALE.ROT_AXIS.Keys);
		}
	}

	private List<string> bodyPartList {
		get {
			return new List<string> {
				DEFINE_APP.BODY_SCALE.BACK_ROT,
				DEFINE_APP.BODY_SCALE.SHOULDER_ROT
			};
		}
	}


	private void Set()
	{
		int directionINdex = this.directionList[this.direction];
		string bodyPartName = this.bodyPartList[this.bodyPart];

		if (DEFINE_APP.BODY_SCALE.ROT_AXIS.ContainsKey(directionINdex) &&
			DEFINE_APP.BODY_SCALE.ROT_AXIS[directionINdex].ContainsKey(bodyPartName)
		) {
			this.axis = DEFINE_APP.BODY_SCALE.ROT_AXIS[directionINdex][bodyPartName];
		}

		if (this.objLook == null || this.objAxis == null) {
			return;
		}
		this.objLook.transform.localPosition = this.axis;
		this.objAxis.LookAt(this.objLook, Vector3.up);
	}


	private void Awake()
	{
		this.Set();
	}

	private void Update()
	{
		this.transform.localRotation = Quaternion.AngleAxis(this.angle += this.rotationSpeed, this.axis);
	}



	[CustomEditor(typeof(QuaternionSample))]
	private class QuaternionSampleEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			QuaternionSample self = this.target as QuaternionSample;

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Direction", GUILayout.Width(EditorGUIUtility.labelWidth));
			List<string> directionList = self.directionList.ConvertAll(x => x.ToString());
			self.direction = EditorGUILayout.Popup(self.direction, directionList.ToArray());
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Body Part", GUILayout.Width(EditorGUIUtility.labelWidth));
			self.bodyPart = EditorGUILayout.Popup(self.bodyPart, self.bodyPartList.ToArray());
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical();

			EditorGUILayout.PropertyField(this.serializedObject.FindProperty("axis"));
			EditorGUILayout.PropertyField(this.serializedObject.FindProperty("angle"));
			EditorGUILayout.PropertyField(this.serializedObject.FindProperty("rotationSpeed"));
			EditorGUILayout.PropertyField(this.serializedObject.FindProperty("objAxis"));
			EditorGUILayout.PropertyField(this.serializedObject.FindProperty("objLook"));
			EditorGUILayout.EndVertical();

			self.Set();

			this.serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif