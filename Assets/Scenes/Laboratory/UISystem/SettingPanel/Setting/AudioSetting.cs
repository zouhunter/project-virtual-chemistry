using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class AudioSetting : SettingTemp
{
    private AudioManager _audioManager;
    protected override void OnEnable()
    {
        _audioManager = GetComponent<AudioManager>();
        base.OnEnable();
    }

    protected override void LoadSettingOnOpen(ExpSetting expSetting)
    {
        if(_audioManager) OnButtonAudioStateChange(expSetting.ButtonAudio);
    }

    protected override void RegistEventOnSettingChange(ExpSetting expSetting)
    {
       if(_audioManager)  expSetting.cg_ButtonAudio += OnButtonAudioStateChange;
    }

    protected override void RemoveEventOnSettingChange(ExpSetting expSetting)
    {
        if(_audioManager)  expSetting.cg_ButtonAudio -= OnButtonAudioStateChange;
    }

    private void OnButtonAudioStateChange(bool isOn)
    {
        _audioManager.IsOn = isOn;
    }

}
