using UnityEngine;
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

    public static int[] RIGHT_HAND_TARGET = new int[] { 1, 3, 6 };
    public static int[] LEFT_HAND_TARGET = new int[] { 2, 5, 8 };
    public static int[] BOTH_HAND_TARGET = new int[] { 4, 7 };

    public static class BODY_SCALE
    {
        // WorldPosition ※フロントのみ
        public static Vector3 PLAYER_BASE_POS;
        public static Vector3 PLAYER_BASE_ROT;
        // LocalPosition ※BasePositionから
        // DEFINEで設定 ※フロントのみ
        public static Vector3 BACK_POS = new Vector3(0f,0.5f,0f);
        // CenterEyeの位置　※サーバー通信
        public static Vector3 HEAD_POS;
        // HEADとHandの位置から決定。BACK_POSから　※フロントのみ
        public static Vector3 SHOULDER_POS_R = new Vector3(HAND_POS_R.x, HAND_POS_R.y, HEAD_POS.z) - BACK_POS;
        // HEADとHandの位置から決定。BACK_POSから　※フロントのみ
        public static Vector3 SHOULDER_POS_L = new Vector3(HAND_POS_L.x, HAND_POS_L.y, HEAD_POS.z) - BACK_POS;
        // 各コントローラーの位置　BasePositionから　※サーバー通信
        public static Vector3 HAND_POS_R;
        public static Vector3 HAND_POS_L;

        public static Dictionary<int, int> TARGET_INDEX_DIC = new Dictionary<int, int>();

        public static Dictionary<int, List<Vector3>> GOAL_DIC = new Dictionary<int, List<Vector3>>();

        public static int[] DIAGNOSIS_ROT_ANCHOR = new int[]
        {
            0,10,20,30,40,50,60,70,80,90,100,110,120,130,140,150,160,170,180
        };

        public static Dictionary<int, float> SHOULDER_ROT_Z = new Dictionary<int, float>()
        {
            {1,90f},
            {2,-90f},
            {3,45f},
            {4,0f},
            {5,-45f},
            {6,135f},
            {7,180f},
            {8,-135f}
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

	
	