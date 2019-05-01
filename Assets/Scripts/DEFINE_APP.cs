﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class DEFINE_APP {//ApplictionDefine

    public enum STATUS_ENUM : int
    {
        PREPARE,
        START,
        TUTORIAL,
        COUNT,
        PLAY,
        FINISH,
        SHOW_RESLUT
    }

    public static class UGUI {
		public enum ANCHOR{ 
			UpperLeft	= 0,
			UpperCenter = 1,
			UpperRight	= 2,
			MiddleLeft	= 3,
			MiddleCenter= 4,
			MiddleRight	= 5,
			LowerLeft	= 6,
			LowerCenter	= 7,
			LowerRight	= 8
		}

		public static Dictionary<ANCHOR, Vector2> ANCHOR_VECTOR2 = new Dictionary<ANCHOR, Vector2>(){
			{ANCHOR.UpperLeft,		new Vector2(0.0f,1.0f)},
			{ANCHOR.UpperCenter,	new Vector2(0.5f,1.0f)},
			{ANCHOR.UpperRight,		new Vector2(1.0f,1.0f)},

			{ANCHOR.MiddleLeft,		new Vector2(0.0f,0.5f)},
			{ANCHOR.MiddleCenter,	new Vector2(0.5f,0.5f)},
			{ANCHOR.MiddleRight,	new Vector2(1.0f,0.5f)},

			{ANCHOR.LowerLeft,		new Vector2(0.0f,0.0f)},
			{ANCHOR.LowerCenter,	new Vector2(0.5f,0.0f)},
			{ANCHOR.LowerRight,		new Vector2(1.0f,0.0f)},

		};

	}

    public enum ANSWER_TYPE_ENUM
    {
        TUTORIAL,
        PLAY,
        RESULT
    }


    /// <summary>
    /// R=右手、L=左手、C=両手
    /// </summary>
    public static string[] HAND_TARGET = new string[] { "R","L","R","C","L","R","C","L"};
    public static Dictionary<string, Vector3> SHOULDER_POS_DIC = new Dictionary<string, Vector3>()
    {
        { "R", BODY_SCALE.SHOULDER_POS_R},
        { "L", BODY_SCALE.SHOULDER_POS_L},
        { "C", BODY_SCALE.SHOULDER_POS_C},
    };

    public static class BODY_SCALE
    {
        // WorldPosition ※フロントのみ
        public static Vector3 PLAYER_BASE_POS;
        public static Vector3 PLAYER_BASE_ROT;
        // LocalPosition ※BasePositionから
        // DEFINEで設定 ※フロントのみ ※BasePositionから
        public static Vector3 BACK_POS = new Vector3(0f,0.5f,0f);
        // CenterEyeの位置　※サーバー通信 ※BasePositionから
        public static Vector3 HEAD_POS;
        // HEADとHandの位置から決定。BACK_POSから　※フロントのみ
        public static Vector3 SHOULDER_POS_R
        {
            get
            {
                return new Vector3(HAND_POS_R.x, HAND_POS_R.y, HEAD_POS.z) - BACK_POS;
            }
        }
        // HEADとHandの位置から決定。BACK_POSから　※フロントのみ
        public static Vector3 SHOULDER_POS_L
        {
            get
            {
                return new Vector3(HAND_POS_L.x, HAND_POS_L.y, HEAD_POS.z) - BACK_POS;
            }
        }
        // HEADとHandの位置から決定。BACK_POSから　※フロントのみ
        public static Vector3 SHOULDER_POS_C
        {
            get
            {
                return new Vector3((HAND_POS_L.x + HAND_POS_R.x) / 2, (HAND_POS_L.y + HAND_POS_R.y) / 2, HEAD_POS.z) - BACK_POS;
            }
        }

        // 各コントローラーの位置　BasePositionから　※サーバー通信
        public static Vector3 HAND_POS_R;
        public static Vector3 HAND_POS_L;

        /// <summary>
        /// 8方向の最大角度を保存。
        /// </summary>
        public static Dictionary<int, float> GOAL_DIC = new Dictionary<int, float>()
        {
            {1, 45f},
            {2, 45f},
            {3, 45f},
            {4, 45f},
            {5, 45f},
            {6, 45f},
            {7, 45f},
            {8, 45f}
        };

        public static int[] DIAGNOSIS_ROT_ANCHOR = new int[]
        {
            10,20,30,40,50,60,70,80,90,100,110,120,130,140,150
        };

        public static Dictionary<int, float> SHOULDER_ROT_Z = new Dictionary<int, float>()
        {
            {1,-90f},
            {2,90f},
            {3,-45f},
            {4,0f},
            {5,45f},
            {6,-135f},
            {7,-180f},
            {8,135f}
        };


        public static Dictionary<int, int> ARM_ROT_SIGN = new Dictionary<int, int>()
        {
            {1,-1},
            {2,1},
            {3,-1},
            {4,-1},
            {5,-1},
            {6,1},
            {7,1},
            {8,1}
        };
    }
}

	
	