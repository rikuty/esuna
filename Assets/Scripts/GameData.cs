using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameData : MonoBehaviour
{

    /// <summary>
    /// Gaze待機時間
    /// </summary>
    public readonly float WAIT_TIME = 0.5f;

    /// <summary>
    /// Gaze判定時間
    /// </summary>
    public readonly float COUNT_TIME = 0.7f;

    /// <summary>
    /// アクション制限時間
    /// </summary>
    public readonly float ACT_LIMIT_TIME = 5f;

    /// <summary>
    /// 制限時間
    /// </summary>
    public float limitTime
    {
        get
        {
            return WAIT_TIME + COUNT_TIME + ACT_LIMIT_TIME;
        }
    }


    public static GameData Instance { get; private set; }

    public string   id              { get; private set; }
    public DateTime dateTime        { get; private set; }
    public float    x               { get; private set; }
    public float    y               { get; private set; }
    public Context  context         { get; private set; }



    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }


    public void Init(string id, DateTime dateTime, float x, float y)
    {
        this.id = id;
        this.dateTime = dateTime;
        this.x = x;
        this.y = y;
    }


    public void SetContext(Context context){
        this.context = context;
    }
}