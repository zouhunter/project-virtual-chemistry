using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class FillImageMediator : MonoBehaviour {

    public string formatString = "进度  {0}  %";
    public int total;
    public Text textShow;
    private Image slider;
    void Start()
    {
        slider = GetComponent<Image>();
        SceneMain.Current.RegisterEvent<float>("LoadProgress", HandleNotification);
    }
    void OnDestroy()
    {
        SceneMain.Current.RemoveEvent<float>("LoadProgress", HandleNotification);
    }
    public void HandleNotification(object progress)
    {
        slider.fillAmount = (float)progress;
        if (textShow != null) textShow.text = string.Format(formatString, (slider.fillAmount * total).ToString("00"));
    }

}
