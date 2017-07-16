using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RunTimeTrigger {
    public TriggerType type;
    public GameObject prefab;
    public bool isWorld;
    public Transform parent;

    #region 触发方式
    public Button button;//按扭
    public Toggle toggle;//布
    public string message;//事件
    #endregion

    public object Data { get; set; }
    public UnityAction<GameObject> OnCreate;

    public enum TriggerType
    {
        button,
        toggle,
        message,
        action,
    }

   
}


