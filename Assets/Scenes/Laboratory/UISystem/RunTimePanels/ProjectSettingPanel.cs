using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using UnityEngine.Events;


//公共对象

public partial class ProjectSettingPanel : MonoBehaviour, IRunTimeToggle
{
    //private ProjectSetting setting;
    //private ProjectSettingObject settObj { get { return Develop.Main.settingData; } }
    public Button closeBtn;
    private Toggle tog;
    public Toggle toggle
    {
        set
        {
            tog = value;
        }
    }

    public event UnityAction OnDelete;

    void Start()
    {
        closeBtn.onClick.AddListener(()=> {
            tog.isOn = false;
            Destroy(gameObject);
        });
    }

    void OnDestroy()
    {
        OnDelete();
    }
}
