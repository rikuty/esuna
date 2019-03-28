using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BackPanelComponent : UtilComponent
{
    [SerializeField] GameObject pbjParent;
    [SerializeField] ButtonPanelComponent button1;
    [SerializeField] ButtonPanelComponent button2;

    Action callback1;
    Action callback2;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(Action callback1, Action callback2)
    {
        this.callback1 = callback1;
        this.callback2 = callback2;
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("BackPanelComponent OnCollisionEnter");
        //Debug.Log("collision.gameObject.name : "+ collision.gameObject.name);
        if (collision.gameObject.name == button1.gameObject.name)
        {
            if (this.callback1 != null)
            {
                this.callback1();
            }
            // TODO 消す（何かしらアニメーション入れたい）
            SetActive(pbjParent, false);
        }

        if (collision.gameObject.name == button2.gameObject.name)
        {
            if (this.callback2 != null)
            {
                this.callback2();
            }
            // TODO 消す（何かしらアニメーション入れたい）
            SetActive(pbjParent, false);
        }
    }
}
