using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using UnityEngine.Events;

using BundleUISystem;

//公共对象

public partial class ProjectSettingPanel :UIPanelTemp
{
    //private ProjectSetting setting;
    //private ProjectSettingObject settObj { get { return Develop.Main.settingData; } }
    public Button closeBtn;

    void Start()
    {
        closeBtn.onClick.AddListener(()=> {
            //tog.isOn = false;
            Destroy(gameObject);
        });
    }

}
