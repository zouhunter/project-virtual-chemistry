using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using ChargeSystem;
public class InstallSpeedSetting : SettingTemp {
    private InstallObj[] _objs;
    private InstallObj[] Objs
    {
        get
        {
            if(_objs == null){
                _objs = gameObject.GetComponentsInChildren<InstallObj>();
            }
            return _objs;
        }
    }
    protected override void LoadSettingOnOpen(ExpSetting expSetting)
    {
        OnSpeedChanged(expSetting.PlayerSpeed);
    }

    protected override void RegistEventOnSettingChange(ExpSetting expSetting)
    {
        expSetting.cg_PlayerSpeed += OnSpeedChanged;
    }

    protected override void RemoveEventOnSettingChange(ExpSetting expSetting)
    {
        expSetting.cg_PlayerSpeed -= OnSpeedChanged;
    }

    private void OnSpeedChanged(float speed)
    {
        if (Objs != null)
        {
            foreach (var item in Objs)
            {
                item.animTime = (int)(11 - speed * 10);
            }
        }
    }
}
