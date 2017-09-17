//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using UnityEngine.Events;
//using UnityEngine;
//using WorldActionSystem;

//public class AnimInstallSpeedSetting : SettingTemp
//{
//    private AnimObj[] _objs;
//    private AnimObj[] Objs
//    {
//        get
//        {
//            if (_objs == null)
//            {
//                _objs = gameObject.GetComponentsInChildren<AnimObj>();
//            }
//            return _objs;
//        }
//    }

//    protected override void LoadSettingOnOpen(ExpSetting expSetting)
//    {
//        OnSpeedChanged(expSetting.PlayerSpeed);
//    }

//    protected override void RegistEventOnSettingChange(ExpSetting expSetting)
//    {
//        expSetting.cg_PlayerSpeed += OnSpeedChanged;
//    }

//    protected override void RemoveEventOnSettingChange(ExpSetting expSetting)
//    {
//        expSetting.cg_PlayerSpeed -= OnSpeedChanged;
//    }

//    private void OnSpeedChanged(float speed)
//    {
//        if (Objs != null)
//        {
//            foreach (var item in Objs)
//            {
//                item.speed = speed * 0.5f;
//            }
//        }
//    }
//}
