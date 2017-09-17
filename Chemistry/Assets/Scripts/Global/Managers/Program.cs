using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// 整个程序的入口，并且做程序全局管理
/// </summary>
public partial class program : GameManager<program>
{
    public static bool isQuit;
    public static bool isOn = false;

    void Awake()
    {
        Application.targetFrameRate = 90;
    }
 
    protected override void LunchFrameWork()
    {
        AudioManager.Instance.ResetDefult(true, 0.3f);
        Facade.RegisterCommand<SceneLoadCommond, SceneData>();
        Facade.RegisterCommand<SettingDataLoadSave, bool>();
    }
}