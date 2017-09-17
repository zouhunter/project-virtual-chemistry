using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
public class PlayerSetting : SettingTemp
{
    public float scale = 1.5f;
    private Transform _transfrom;
    private PersonCharacter _person;
    protected override void LoadSettingOnOpen(ExpSetting expSetting)
    {
        _transfrom = transform;
        _person = GetComponent<PersonCharacter>();
         OnCamerHightChange(expSetting.cameraHight);
         OnSpeedChange(expSetting.playerSpeed);
    }

    protected override void RegistEventOnSettingChange(ExpSetting expSetting)
    {
        expSetting.cg_CameraHight += OnCamerHightChange;
        expSetting.cg_PlayerSpeed += OnSpeedChange;
    }

    protected override void RemoveEventOnSettingChange(ExpSetting expSetting)
    {
        expSetting.cg_CameraHight -= OnCamerHightChange;
        expSetting.cg_PlayerSpeed -= OnSpeedChange;
    }

    private void OnCamerHightChange(float height)
    {
        if(_transfrom) _transfrom.localScale = new Vector3(1, height * scale + 0.1f, 1);
        else { Debug.LogWarning(gameObject.name + "事件注销失败"); }
    }
    private void OnSpeedChange(float playerSpeed)
    {
        if(_person) _person.TurnSpeed = playerSpeed * 100;
        else { Debug.LogWarning( gameObject.name+ "事件注销失败"); }
    }
}
