using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class SelectPanel : MonoBehaviour
{
    public Toggle toggle;
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

