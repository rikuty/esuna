using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;




public class Context
{
    public readonly float PLAY_TIME = 40.0f;

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


    public void AddPlayTime(float deltaTime)
    {
        this.currentPlayTime += deltaTime;
    }


    public void AddGamePoint()
    {
        this.gamePoint += 10;
    }


}
