using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;




public class Context
{
    public readonly float PLAY_TIME = 180.0f;

    public float currentPlayTime { get; private set; }

    public int gamePoint { get; private set; }

    public float leftPlayTime
    {
        get
        {
            return PLAY_TIME - currentPlayTime;
        }
    }

    public bool isPlay
    {
        get
        {
            return currentPlayTime < PLAY_TIME;
        }
    }

    /// <summary>
    /// 手を合わせて回答を始めたらtrueになる
    /// </summary>
    public bool isAnswering = false;

    public DEFINE_APP.STATUS_ENUM currentStatus;


    public void AddPlayTime(float deltaTime)
    {
        this.currentPlayTime += deltaTime;
    }


    public void AddGamePoint()
    {
        this.gamePoint += 10;
    }


}
