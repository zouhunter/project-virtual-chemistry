using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class TextMediator : MonoBehaviour
{
    public Text text;
    public bool showAnim;
    public float animTime;
    private string infomation;
    private Tweener textTween;

    void Start()
    {
        textTween = text.DOText(infomation, animTime).SetAutoKill(false).Pause();
        SceneMain.Current.RegisterEvent<string>(AppConfig.EventKey.TIP, GetTip);
    }
    public void GetTip(object infomation)
    {
        this.infomation = (string)infomation;
        if (text != null && infomation != null)
        {
            if (showAnim)
            {
                text.text = "";
                textTween.ChangeEndValue(infomation).Restart();
            }
            else
            {
                text.text = this.infomation;
            }
        }
    }
    void OnDestroy()
    {
        SceneMain.Current.RemoveEvent<string>(AppConfig.EventKey.TIP, GetTip);
    }
}

