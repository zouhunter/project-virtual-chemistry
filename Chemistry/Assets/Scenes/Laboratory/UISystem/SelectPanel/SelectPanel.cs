using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using BundleUISystem;

public class SelectPanel : UIPanelTemp
{
    public Transform panel;
    public DoTweenPanel deTween;
    void Start()
    {
        deTween.InitTween(panel);
        Laboratory.onOperateTypeChanged += OnTypeChanged;
    }
    void OnTypeChanged(OperateType type)
    {
        panel.gameObject.SetActive(type == OperateType.Operate);
    }
}

