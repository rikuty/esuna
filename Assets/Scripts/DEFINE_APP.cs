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
        public enum ANCHOR {
            UpperLeft = 0,
            UpperCenter = 1,
            UpperRight = 2,
            MiddleLeft = 3,
            MiddleCenter = 4,
            MiddleRight = 5,
            LowerLeft = 6,
            LowerCenter = 7,
            LowerRight = 8
        }

        public static Dictionary<ANCHOR, Vector2> ANCHOR_VECTOR2 = new Dictionary<ANCHOR, Vector2>(){
            {ANCHOR.UpperLeft,      new Vector2(0.0f,1.0f)},
            {ANCHOR.UpperCenter,    new Vector2(0.5f,1.0f)},
            {ANCHOR.UpperRight,     new Vector2(1.0f,1.0f)},

            {ANCHOR.MiddleLeft,     new Vector2(0.0f,0.5f)},
            {ANCHOR.MiddleCenter,   new Vector2(0.5f,0.5f)},
            {ANCHOR.MiddleRight,    new Vector2(1.0f,0.5f)},

            {ANCHOR.LowerLeft,      new Vector2(0.0f,0.0f)},
            {ANCHOR.LowerCenter,    new Vector2(0.5f,0.0f)},
            {ANCHOR.LowerRight,     new Vector2(1.0f,0.0f)},

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
    public static OVRInput.Controller[] HAND_TARGET = new OVRInput.Controller[] { OVRInput.Controller.RTouch, OVRInput.Controller.LTouch, OVRInput.Controller.RTouch, OVRInput.Controller.Touch, OVRInput.Controller.LTouch, OVRInput.Controller.RTouch, OVRInput.Controller.Touch, OVRInput.Controller.LTouch };
    public static Dictionary<OVRInput.Controller, Vector3> SHOULDER_POS_DIC = new Dictionary<OVRInput.Controller, Vector3>()
    {
        { OVRInput.Controller.RTouch, BODY_SCALE.SHOULDER_POS_R},
        { OVRInput.Controller.LTouch, BODY_SCALE.SHOULDER_POS_L},
        { OVRInput.Controller.Touch, BODY_SCALE.SHOULDER_POS_C},
    };
    public static Dictionary<OVRInput.Controller, Vector3> HAND_POS_DIC = new Dictionary<OVRInput.Controller, Vector3>()
    {
        { OVRInput.Controller.RTouch, BODY_SCALE.HAND_LOCALPOS_R},
        { OVRInput.Controller.LTouch, BODY_SCALE.HAND_LOCALPOS_L},
        { OVRInput.Controller.Touch, BODY_SCALE.HAND_LOCALPOS_C},
    };

    public static class BODY_SCALE
    {
        // WorldPosition ※フロントのみ
        public static Vector3 PLAYER_BASE_POS;
        public static Vector3 PLAYER_BASE_ROT;
        // LocalPosition ※BasePositionから
        // DEFINEで設定 ※フロントのみ ※BasePositionから
        public static Vector3 BACK_POS = new Vector3(0f, 0.5f, 0f);
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
                return new Vector3((HAND_POS_L.x + HAND_POS_R.x) / 2f, (HAND_POS_L.y + HAND_POS_R.y) / 2f, HEAD_POS.z) - BACK_POS;
            }
        }

        // 各コントローラーの位置　BasePositionから　※サーバー通信
        public static Vector3 HAND_POS_R = new Vector3(0.1f, 1.1f, 0.5f);
        public static Vector3 HAND_POS_L = new Vector3(-0.1f, 1.1f, 0.5f);
        public static Vector3 HAND_POS_C
        {
            get
            {
                return (HAND_POS_L + HAND_POS_R) / 2f;
            }
        }

        // 各コントローラーの位置　ShouldePositionから　※サーバーしない
        public static Vector3 HAND_LOCALPOS_R
        {
            get
            {
                return HAND_POS_R - SHOULDER_POS_R - BACK_POS;
            }
        }
        public static Vector3 HAND_LOCALPOS_L
        {
            get
            {
                return HAND_POS_L - SHOULDER_POS_L - BACK_POS;
            }
        }
        public static Vector3 HAND_LOCALPOS_C
        {
            get
            {
                return (HAND_LOCALPOS_L + HAND_LOCALPOS_R) / 2f;
            }
        }


        public static int[] DIAGNOSIS_DIRECTS = new int[]{1,2,4,7};

        public static int DIAGNOSIS_COUNT = 10;

        public static Dictionary<int, int> DIAGNOSIS_COUNT_DIC = new Dictionary<int, int>()
        {
            {1, 9},
            {2, 9},
            {3, 12},
            {4, 12},
            {5, 12},
            {6, 9},
            {7, 9},
            {8, 9}
        };


        public static void SetDefine()
        {
            _GOAL_DIC = new Dictionary<int, Dictionary<string, Vector3>>()
            {
                    {1, new Dictionary<string, Vector3>{ { BACK_ROT, new Vector3(0f, -45f, 0f) }, { SHOULDER_ROT, new Vector3(0f, -45f, 0f) } } },
                    {2, new Dictionary<string, Vector3>{ { BACK_ROT, new Vector3(0f, 45f, 0f) }, { SHOULDER_ROT, new Vector3(0f, 45f, 0f) } } },
                    {4, new Dictionary<string, Vector3>{ { BACK_ROT, new Vector3(-30f, 0f, 0f) }, { SHOULDER_ROT, new Vector3(-105f, 0f, 0f) } } },
                    {7, new Dictionary<string, Vector3>{ { BACK_ROT, new Vector3(90f, 0f, 0f) }, { SHOULDER_ROT, new Vector3(30f, 0f, 0f) } } }
            };

            _DIAGNOSIS_ROT_MAX = new Dictionary<int, Dictionary<string, Vector3>>
            {
                {1, new Dictionary<string, Vector3>{ { BACK_ROT, new Vector3(0f, -45f, 0f) }, { SHOULDER_ROT, new Vector3(0f, -45f, 0f) } } },
                {2, new Dictionary<string, Vector3>{ { BACK_ROT, new Vector3(0f, 45f, 0f) }, { SHOULDER_ROT, new Vector3(0f, 45f, 0f) } } },
                {4, new Dictionary<string, Vector3>{ { BACK_ROT, new Vector3(-30f, 0f, 0f) }, { SHOULDER_ROT, new Vector3(-105f, 0f, 0f) } } },
                {7, new Dictionary<string, Vector3>{ { BACK_ROT, new Vector3(90f, 0f, 0f) }, { SHOULDER_ROT, new Vector3(30f, 0f, 0f) } } }
            };

            _GOAL_DIC.Add(3, new Dictionary<string, Vector3> { { BACK_ROT, GOAL_DIC[1][BACK_ROT] + GOAL_DIC[4][BACK_ROT] }, { SHOULDER_ROT, GOAL_DIC[1][SHOULDER_ROT] + GOAL_DIC[4][SHOULDER_ROT] } });
            _GOAL_DIC.Add(5, new Dictionary<string, Vector3> { { BACK_ROT, GOAL_DIC[2][BACK_ROT] + GOAL_DIC[4][BACK_ROT] }, { SHOULDER_ROT, GOAL_DIC[2][SHOULDER_ROT] + GOAL_DIC[4][SHOULDER_ROT] } });
            _GOAL_DIC.Add(6, new Dictionary<string, Vector3> { { BACK_ROT, GOAL_DIC[1][BACK_ROT] + GOAL_DIC[7][BACK_ROT] }, { SHOULDER_ROT, GOAL_DIC[1][SHOULDER_ROT] + GOAL_DIC[7][SHOULDER_ROT] } });
            _GOAL_DIC.Add(8, new Dictionary<string, Vector3> { { BACK_ROT, GOAL_DIC[2][BACK_ROT] + GOAL_DIC[7][BACK_ROT] }, { SHOULDER_ROT, GOAL_DIC[2][SHOULDER_ROT] + GOAL_DIC[7][SHOULDER_ROT] } });

            _DIAGNOSIS_ROT_MAX.Add(3, new Dictionary<string, Vector3>{ { BACK_ROT, DIAGNOSIS_ROT_MAX[1][BACK_ROT]+ DIAGNOSIS_ROT_MAX[4][BACK_ROT] }, { SHOULDER_ROT, DIAGNOSIS_ROT_MAX[1][SHOULDER_ROT]+DIAGNOSIS_ROT_MAX[4][SHOULDER_ROT] } });
            _DIAGNOSIS_ROT_MAX.Add(5, new Dictionary<string, Vector3> { { BACK_ROT, DIAGNOSIS_ROT_MAX[2][BACK_ROT] + DIAGNOSIS_ROT_MAX[4][BACK_ROT] }, { SHOULDER_ROT, DIAGNOSIS_ROT_MAX[1][SHOULDER_ROT] + DIAGNOSIS_ROT_MAX[4][SHOULDER_ROT] } });
            _DIAGNOSIS_ROT_MAX.Add(6, new Dictionary<string, Vector3>{ { BACK_ROT, DIAGNOSIS_ROT_MAX[1][BACK_ROT]+ DIAGNOSIS_ROT_MAX[7][BACK_ROT] }, { SHOULDER_ROT, DIAGNOSIS_ROT_MAX[1][SHOULDER_ROT]+DIAGNOSIS_ROT_MAX[7][SHOULDER_ROT] } });
            _DIAGNOSIS_ROT_MAX.Add(8, new Dictionary<string, Vector3>{ { BACK_ROT, DIAGNOSIS_ROT_MAX[2][BACK_ROT]+ DIAGNOSIS_ROT_MAX[7][BACK_ROT] }, { SHOULDER_ROT, DIAGNOSIS_ROT_MAX[2][SHOULDER_ROT]+DIAGNOSIS_ROT_MAX[7][SHOULDER_ROT] } });
        }


        /// <summary>
        /// 8方向の最大角度を保存。
        /// </summary>
        private static Dictionary<int, Dictionary<string, Vector3>> _GOAL_DIC;
        public static Dictionary<int, Dictionary<string, Vector3>> GOAL_DIC {
            get
            {
                if (_GOAL_DIC == null){
                    SetDefine();
                }
                return _GOAL_DIC;
            }
        }

        public static string BACK_ROT = "BACK_ROT";
        public static string SHOULDER_ROT = "SHOULDER_ROT";

        private static Dictionary<int, Dictionary<string, Vector3>> _DIAGNOSIS_ROT_MAX;
        public static Dictionary<int, Dictionary<string, Vector3>> DIAGNOSIS_ROT_MAX
        {
            get
            {
                if (_DIAGNOSIS_ROT_MAX == null)
                {
                    SetDefine();
                }
                return _DIAGNOSIS_ROT_MAX;
            }
        }


        public static Dictionary<int, float> SHOULDER_ROT_Z = new Dictionary<int, float>()
        {
            {1,-90f},
            {2,90f},
            {3,-135f},
            {4,180f},
            {5,135f},
            {6,-45f},
            {7,0f},
            {8,45f}
        };


        //public static Dictionary<int, int> ARM_ROT_SIGN = new Dictionary<int, int>()
        //{
        //    {1,-1},
        //    {2,1},
        //    {3,-1},
        //    {4,-1},
        //    {5,-1},
        //    {6,1},
        //    {7,1},
        //    {8,1}
        //};
    }

    /// <summary>
    /// 測定開始してから次のボールに触れないために次に移動する時間。
    /// </summary>
    public static float DIAGNOSIS_WAIT_TIME = 3f;
}

	
	