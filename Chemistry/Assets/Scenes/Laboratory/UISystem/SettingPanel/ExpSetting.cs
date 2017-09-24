using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
[Serializable]
public class ExpSetting
{
    public bool guide;
    public float cameraHight;
    public float playerSpeed;
    public float lightStrength;
    public float fontSize;
    public float[] windowColor = new float[4];
    public float[] selectColor = new float[4];
    public bool humanVoice;
    public bool buttonAudio;
    
    public bool Guide
    {
        get
        {
            return guide;
        }
        set
        {
            guide = value;
            if (cg_Guide != null)
            {
                cg_Guide.Invoke(guide);
            }
        }
    }
    public float CameraHight
    {
        get
        {
            return cameraHight;
        }
        set
        {
            cameraHight = value;
            if (cg_CameraHight != null)
            {
                cg_CameraHight.Invoke(cameraHight);
            }
        }
    }
    public float PlayerSpeed
    {
        get
        {
            return playerSpeed;
        }
        set
        {
            if (playerSpeed != value)
            {
                playerSpeed = value;
                if (cg_PlayerSpeed != null) cg_PlayerSpeed.Invoke(playerSpeed);
            }
        }
    }
    public float LightStrength
    {
        get
        {
            return lightStrength;
        }
        set
        {
            if (lightStrength != value)
            {
                lightStrength = value;
                if (cg_LightStrength != null) cg_LightStrength.Invoke(lightStrength);
            }
        }
    }
    public float FontSize
    {
        get
        {
            return fontSize;
        }
        set
        {
            if (fontSize != value)
            {
                fontSize = value;
                if (cg_FontSize != null) cg_FontSize.Invoke(fontSize);
            }
        }
    }
    public Color WindowColor
    {
        get
        {
            return new Color(windowColor[0], windowColor[1], windowColor[2], windowColor[3]);
        }
        set
        {
            if (!(windowColor[0] == value.r && windowColor[1] == value.g && windowColor[2] == value.b && windowColor[3] == value.b))
            {
                windowColor = new float[] { value.r, value.g, value.b, value.a };
                if (cg_WindowColor != null)
                {
                    cg_WindowColor.Invoke(new Color(windowColor[0], windowColor[1], windowColor[2], windowColor[3]));
                }
            }
        }
    }
    public Color SelectColor
    {
        get
        {
            return new Color(selectColor[0], selectColor[1], selectColor[2], selectColor[3]);
        }
        set
        {
            if (!(selectColor[0] == value.r && selectColor[1] == value.g && selectColor[2] == value.b && selectColor[3] == value.b))
            {
                selectColor = new float[] { value.r, value.g, value.b, value.a };
                if (cg_SelectColor != null)
                {
                    cg_SelectColor.Invoke(value);
                }
            }
        }
    }
    public bool HumanVoice
    {
        get
        {
            return humanVoice;
        }
        set
        {
            if (humanVoice != value)
            {
                humanVoice = value;
                if (cg_HumanVoice != null) cg_HumanVoice.Invoke(humanVoice);
            }
        }
    }
    public bool ButtonAudio
    {
        get
        {
            return buttonAudio;
        }
        set
        {
            if (buttonAudio != value)
            {
                buttonAudio = value;
                if (cg_ButtonAudio != null) cg_ButtonAudio.Invoke(buttonAudio);
            }
        }
    }

    public event UnityAction<bool> cg_Guide;
    public event UnityAction<float> cg_CameraHight;
    public event UnityAction<float> cg_PlayerSpeed;
    public event UnityAction<float> cg_LightStrength;
    public event UnityAction<float> cg_FontSize;
    public event UnityAction<Color> cg_WindowColor;
    public event UnityAction<Color> cg_SelectColor;
    public event UnityAction<bool> cg_HumanVoice;
    public event UnityAction<bool> cg_ButtonAudio;

    public void ResetDefult()
    {
        guide = false;
        cameraHight = 0.8f;
        playerSpeed = .5f;
        lightStrength = 0.8f;
        fontSize = 1;
        windowColor = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
        selectColor = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
        humanVoice = true;
        buttonAudio = true;
    }

    public ExpSetting GetSaveAbleCopy()
    {
        ExpSetting setting = new ExpSetting();
        setting.guide = guide;
        setting.cameraHight = cameraHight;
        setting.playerSpeed = playerSpeed;
        setting.lightStrength = lightStrength;
        setting.fontSize = fontSize;
        setting.windowColor = windowColor;
        setting.selectColor = selectColor;
        setting.humanVoice = humanVoice;
        setting.buttonAudio = buttonAudio;
        return setting;
    }
    /*
    1.程序引导系统开启状态切换
    2.相机的高低调节
    3.人物移动速度调节
    4.灯光亮度调节
    5.主题色选择
    6.选中提示颜色
    7.语音提示开关
    8.语音提示男声女声切换
    8.按扭声音开关
    */
}
