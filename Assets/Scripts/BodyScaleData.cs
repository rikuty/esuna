﻿using System.Collections.Generic;
using UnityEngine;

public class BodyScaleData
{
	public Dictionary<int, float> goalDic { get; private set; }
	public Dictionary<int, Dictionary<string, float>> goalCurrentDic { get; private set; }


	public BodyScaleData()
	{
		this.SetDefaultGoalDic();
		this.SetDiagonal();
	}


	public void SetDiagonal()
	{
		// 上下左右の回旋の測定後、ななめ方向を補間する

		/*
		_GOAL_DIC[3][BACK_ROT] = new Vector3(0f, GOAL_DIC[1][BACK_ROT].y, 0f);
        _GOAL_DIC[3][SHOULDER_ROT] = new Vector3(GOAL_DIC[4][SHOULDER_ROT].x / 2f, GOAL_DIC[1][SHOULDER_ROT].y, 0f);
        _GOAL_DIC[5][BACK_ROT] = new Vector3(0f, GOAL_DIC[2][BACK_ROT].y, 0f);
        _GOAL_DIC[5][SHOULDER_ROT] = new Vector3(GOAL_DIC[4][SHOULDER_ROT].x / 2f, GOAL_DIC[2][SHOULDER_ROT].y, 0f);
        _GOAL_DIC[6][BACK_ROT] = new Vector3(GOAL_DIC[7][BACK_ROT].x, GOAL_DIC[1][BACK_ROT].y, 0f);
        _GOAL_DIC[6][SHOULDER_ROT] = new Vector3(GOAL_DIC[7][SHOULDER_ROT].x, 0f, 0f);
        _GOAL_DIC[8][BACK_ROT] = new Vector3(GOAL_DIC[7][BACK_ROT].x, GOAL_DIC[2][BACK_ROT].y, 0f);
        _GOAL_DIC[8][SHOULDER_ROT] = new Vector3(GOAL_DIC[7][SHOULDER_ROT].x, 0f, 0f);
		*/

		this.goalDic[3] = (this.goalDic[1] + this.goalDic[4]) / 2f;
		this.goalDic[5] = (this.goalDic[2] + this.goalDic[4]) / 2f;
		this.goalDic[6] = (this.goalDic[1] + this.goalDic[7]) / 2f;
		this.goalDic[8] = (this.goalDic[2] + this.goalDic[7]) / 2f;

		this.SetDefaultCurrentDiagonal();
	}


	private void SetDefaultGoalDic()
	{
		this.goalDic = new Dictionary<int, float>();

		foreach (int direction in DEFINE_APP.BODY_SCALE.ROT_AXIS.Keys) {
			this.goalDic.Add(direction, 0f);
		}

		foreach (int direction in DEFINE_APP.BODY_SCALE.DIAGNOSIS_DIRECTS) {
			float sumAngle = 0f;
			sumAngle += DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[direction][DEFINE_APP.BODY_SCALE.BACK_ROT];
			sumAngle += DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[direction][DEFINE_APP.BODY_SCALE.SHOULDER_ROT];
			this.goalDic[direction] = sumAngle;
		}
	}

	private void SetDefaultCurrentDiagonal()
	{
		this.goalCurrentDic = new Dictionary<int, Dictionary<string, float>>();

		foreach (int direction in DEFINE_APP.BODY_SCALE.ROT_AXIS.Keys) {

			/*
			// 測定時の回旋は腰と肩の合計値になっているため、肩と腰それぞれがしめる回転の割合から分解して求める
			float backAngle = this.goalDic[direction] * DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX_RATIO[direction][DEFINE_APP.BODY_SCALE.BACK_ROT] / 2f;
			float shoulderAngle = this.goalDic[direction] * DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX_RATIO[direction][DEFINE_APP.BODY_SCALE.SHOULDER_ROT] / 2f;
			*/
			float backAngle = DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[direction][DEFINE_APP.BODY_SCALE.BACK_ROT] / 2f;
			float shoulderAngle = DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[direction][DEFINE_APP.BODY_SCALE.SHOULDER_ROT] / 2f;

			float backAngleMin = DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[direction][DEFINE_APP.BODY_SCALE.BACK_ROT] / (float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[direction];
			float shoulderAngleMin = DEFINE_APP.BODY_SCALE.DIAGNOSIS_ROT_MAX[direction][DEFINE_APP.BODY_SCALE.SHOULDER_ROT] / (float)DEFINE_APP.BODY_SCALE.DIAGNOSIS_COUNT_DIC[direction];

			float resultBack = Mathf.Abs(backAngle) <= Mathf.Abs(backAngleMin) ? backAngleMin : backAngle;
			float resultShoulder = Mathf.Abs(shoulderAngle) <= Mathf.Abs(shoulderAngleMin) ? shoulderAngleMin : shoulderAngle;

			this.goalCurrentDic.Add(
				direction,
				new Dictionary<string, float> {
					{ DEFINE_APP.BODY_SCALE.BACK_ROT, resultBack },
					{ DEFINE_APP.BODY_SCALE.SHOULDER_ROT, resultShoulder }
				}
			);
		}
	}
}