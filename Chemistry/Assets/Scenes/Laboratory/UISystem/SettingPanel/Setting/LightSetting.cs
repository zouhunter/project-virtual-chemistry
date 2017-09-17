using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class LightSetting : SettingTemp {
    [SerializeField]
    private Light _light;

    protected override void LoadSettingOnOpen(ExpSetting expSetting)
    {
        if (_light == null) _light = GetComponent<Light>();
        OnLightChange(expSetting.LightStrength);
    }

    protected override void RegistEventOnSettingChange(ExpSetting expSetting)
    {
        expSetting.cg_LightStrength += OnLightChange;
    }

    protected override void RemoveEventOnSettingChange(ExpSetting expSetting)
    {
        expSetting.cg_LightStrength -= OnLightChange;
    }

    private void OnLightChange(float intensity)
    {
        _light.intensity = intensity;
    }

}
