using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using BundleUISystem;

public enum OperateType
{
    Nil,
    Config,//元素使用builditem
    Domon,//元素不可操作
    Operate//使用Select类来操作
}

/// <summary>
/// 管理本场景3维和ui基本事件,启动框架等
/// </summary>
public class Laboratory : SceneMain<Laboratory> {

    public static OperateType operateType = OperateType.Operate;
    public static event UnityAction<OperateType> onOperateTypeChanged;
    public static void ChangedOperateType(OperateType type)
    {
        operateType = type;
        if (onOperateTypeChanged != null) onOperateTypeChanged(type);
    }

    public BuildingCtrl buildCtrl = new BuildingCtrl();
    public RecordCtrl recordCtrl = new RecordCtrl();
    public HitController hitCtrl = new global::HitController();
    public Transform installParent;

    protected override void Awake()
    {
        program.StartGame();
        Facade.SendNotification<bool>(typeof(SettingDataLoadSave).ToString(),true);
        base.Awake();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
        {
            InvokeEvents(AppConfig.EventKey.ClickEmpty);
        }
    }
}

