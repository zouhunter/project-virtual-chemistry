using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem;

public class ExperimentPanel : UIPanelTemp {
    private void Start()
    {
        SceneMain.Current.RegisterEvent(AppConfig.EventKey.ClickEmpty, Close);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneMain.Current.RemoveEvent(AppConfig.EventKey.ClickEmpty, Close);
    }
    void Close()
    {
        Destroy(gameObject);
    }
}
