using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using BundleUISystem;
public class SettingPanel : UIPanelTemp {
    [SerializeField]
    private Button closeBtn;

   //[SerializeField] private Toggle guideTog;             //1.程序引导系统开启状态切换
   [SerializeField] private Slider cameraHightSld;       //2.相机的高低调节
   [SerializeField] private Slider playerSpeedSld;       //3.人物移动速度调节
   [SerializeField] private Slider ligthStrigthSld;      //4.灯光亮度调节
   //[SerializeField] private Slider selectFontSize;       //5.提示字体大小
   //[SerializeField] private Button mainColrField;        //6.主题色选择
   //[SerializeField] private Button selectColorField;     //7.选中提示颜色
   //[SerializeField] private Toggle humanVoiceTog;        //8.语音提示开关
   //[SerializeField] private Toggle buttonAudioTog;       //9.按扭声音开关

    [SerializeField] private Button resetBtn;     
    [SerializeField] private Button saveBtn;
    private ExpSetting setting { get; set; }
    private Action onClose;
    //private bool modify = false;
    public override void HandleData(object data)
    {
        base.HandleData(data);
        if(data is Action)
        {
            onClose = (Action)data;
        }
    }

    private void Awake()
    {
        //humanVoiceTog.onValueChanged.AddListener(OnHumanVoiceTogValueChanged);
        //buttonAudioTog.onValueChanged.AddListener(OnButtonAudioTogValueChanged);
        cameraHightSld.onValueChanged.AddListener(OnCameraHightSldValueChanged);
        playerSpeedSld.onValueChanged.AddListener(OnPlayerSpeedSldValueChanged);
        ligthStrigthSld.onValueChanged.AddListener(OnLigthStrigthSldValueChanged);
        //selectFontSize.onValueChanged.AddListener(OnSelectFontSizeValueChanged);
    }

    private void Start()
    {
        if (setting == null)
        {
            Facade.RetrieveProxy<ExpSetting>(AppConfig.EventKey.SettngData, OnLoadSettingData);
        }
        else
        {
            OnLoadSettingData(setting);
        }
        //mainColrField.onClick.AddListener(SelectMainColor);
        //selectColorField.onClick.AddListener(SelectNoticColor);
        resetBtn.onClick.AddListener(ResetDefult);
        saveBtn.onClick.AddListener(SaveSetting);
        closeBtn.onClick.AddListener(BackButtonClick);
    }

    private void OnHumanVoiceTogValueChanged(bool arg)
    {
        if (setting != null) setting.HumanVoice = arg;
    }
    private void OnButtonAudioTogValueChanged(bool arg)
    {
        if (setting != null) setting.ButtonAudio = arg;
    }
    private void OnCameraHightSldValueChanged(float arg)
    {
        if (setting != null) setting.CameraHight = arg;
    }
    private void OnPlayerSpeedSldValueChanged(float arg)
    {
        if (setting != null) setting.PlayerSpeed = arg;
    }
    private void OnLigthStrigthSldValueChanged(float arg)
    {
        if (setting != null) setting.LightStrength = arg;
    }
    private void OnSelectFontSizeValueChanged(float arg)
    {
        if (setting != null) setting.FontSize = arg;
    }

    private void SaveSetting()
    {
        Facade.SendNotification(AppConfig.EventKey.TIP, "设置信息更改已经保存");
        SaveSettingData();
    }

    private void ResetDefult()
    {
        setting.ResetDefult();
        Facade.SendNotification(AppConfig.EventKey.TIP, "设置信息已经重置");
        OnLoadSettingData(setting);
    }

    private void OnDisable()
    {
        //UIGroup.Close<ColorChoisePanel>();
    }
    /// <summary>
    /// 返回
    /// </summary>
    private void BackButtonClick()
    {
        Destroy(gameObject);
        if(onClose!= null) onClose.Invoke();
    }

    ///// <summary>
    ///// 主题颜色选择
    ///// </summary>
    ///// <param name="image"></param>
    //private void SelectMainColor()
    //{
    //    ColorChoisePanel.Data data = new global::ColorChoisePanel.Data();
    //    data.startColor = mainColrField.image.color;
    //    data.onColorChanged = (x) =>
    //    {
    //        x.a = 1;
    //        mainColrField.image.color = x;
    //    };
    //    UIGroup.Open<ColorChoisePanel>(data);
    //}

    ///// <summary>
    ///// 提示色选择
    ///// </summary>
    //private void SelectNoticColor()
    //{
    //    ColorChoisePanel.Data data = new global::ColorChoisePanel.Data();
    //    data.startColor = selectColorField.image.color;
    //    data.onColorChanged = (x) =>
    //    {
    //        x.a = 1;
    //        selectColorField.image.color = x;
    //    };
    //    UIGroup.Open<ColorChoisePanel>(data);
    //}

    void OnLoadSettingData(ExpSetting proxy)
    {
        setting = proxy;
        //guideTog.isOn = setting.guide;
        cameraHightSld.value = setting.cameraHight;
        playerSpeedSld.value = setting.playerSpeed;
        ligthStrigthSld.value = setting.lightStrength;
        //selectFontSize.value = setting.fontSize;
        ////mainColrField.image.color = setting.WindowColor;
        ////selectColorField.image.color = setting.SelectColor; 
        //humanVoiceTog.isOn = setting.humanVoice;
        //buttonAudioTog.isOn = setting.buttonAudio;
    }
    
    void SaveSettingData()
    {
        //setting.Guide = guideTog.isOn;
        setting.CameraHight  = cameraHightSld.value;
        setting.PlayerSpeed  = playerSpeedSld.value;
        setting.LightStrength  = ligthStrigthSld.value;
        //setting.FontSize  = (int)selectFontSize.value;
        ////setting.WindowColor = mainColrField.image.color;
        ////setting.SelectColor = selectColorField.image.color;
        //setting.HumanVoice  = humanVoiceTog.isOn;
        //setting.ButtonAudio  = buttonAudioTog.isOn;
        Facade.SendNotification(typeof(SettingDataLoadSave).ToString(), false);
    }
}
