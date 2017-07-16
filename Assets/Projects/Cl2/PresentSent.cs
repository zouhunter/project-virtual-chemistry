using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class PresentSent : MonoBehaviour {
    public void SendTipInformation(string info)
    {
        EventFacade.Instance.SendNotification(AppConfig.EventKey.TIP, info);
    }

    public void SendWaringInformation(string info)
    {
        PresentationData data = PresentationData.Allocate("提示", info, "");
        EventFacade.Instance.SendNotification(AppConfig.EventKey.OPEN_PRESENTATION, data);
    }
}
