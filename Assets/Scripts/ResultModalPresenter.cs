using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultModalPresenter : UtilComponent
{
    
    [SerializeField] private Text averageTime;

    public void Show(ResultModalModel model) {
        SetLabel(this.averageTime, model.averageTime.ToString("F2"));
        //model.Start.Init("Start", model.Context);
        this.gameObject.SetActive(true);
    }


    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Reset()
    {
        // Gamestrap.GSAppExampleControl.Instance.LoadScene(Gamestrap.ESceneNames.scene_title);
    }

}
