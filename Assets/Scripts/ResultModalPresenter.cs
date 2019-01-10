using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultModalPresenter : UtilComponent
{
    
    [SerializeField] private Text gamePoint;

    public void Show(ResultModalModel model) {
        SetLabel(this.gamePoint, model.context.gamePoint.ToString());
        //model.Start.Init("Start", model.Context);
        this.gameObject.SetActive(true);
    }

}
