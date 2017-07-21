using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

using BundleUISystem;

public class PresentSent : MonoBehaviour {
    public void SendTipInformation(string info)
    {
        SceneMain.Current.InvokeEvents(AppConfig.EventKey.TIP, info);
    }

    public void SendWaringInformation(string info)
    {
        PresentationData data = PresentationData.Allocate("提示", info, "");
       UIGroup.Open<PresentationPanel>(data);
    }
}
