using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class FillImageMediator : Mediator<float> {

    public string formatString = "进度  {0}  %";
    public int total;
    public Text textShow;
    public Image slider;
    public override string Acceptor
    {
        get
        {
            return AppConfig.EventKey.FillImage;
        }
    }

    public override void HandleNotification(float notification)
    {
        slider.fillAmount = (float)notification;
        if (textShow != null) textShow.text = string.Format(formatString, (slider.fillAmount * total).ToString("00"));
    }
}
