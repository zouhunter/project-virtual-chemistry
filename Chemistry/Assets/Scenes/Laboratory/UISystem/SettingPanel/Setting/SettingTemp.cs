using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public abstract class SettingTemp : MonoBehaviour
{
    private ExpSetting _expSetting;
    protected virtual void OnEnable()
    {
        Facade.RetrieveProxy<ExpSetting>(AppConfig.EventKey.SettngData, OnLoadSetting);
    }

    protected virtual void OnDisable()
    {
        if(_expSetting != null)
        RemoveEventOnSettingChange(_expSetting);
    }

    private void OnLoadSetting(ExpSetting arg0)
    {
        if (arg0 != null)
        {
            _expSetting = arg0;
            LoadSettingOnOpen(arg0);
            RegistEventOnSettingChange(arg0);
        }
    }

    protected abstract void LoadSettingOnOpen(ExpSetting expSetting);

    protected abstract void RegistEventOnSettingChange(ExpSetting expSetting);

    protected abstract void RemoveEventOnSettingChange(ExpSetting expSetting);
}
