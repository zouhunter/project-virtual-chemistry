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
        EventFacade.Instance.RegisterEvent<string>(AppConfig.EventKey.TIP, GetTip);
    }
    public void GetTip(string infomation)
    {
        this.infomation = infomation;
        if (text != null && infomation != null)
        {
            if (showAnim)
            {
                text.text = "";
                textTween.ChangeEndValue(infomation).Restart();
            }
            else
            {
                text.text = infomation;
            }
        }
    }
    void OnDestroy()
    {
        EventFacade.Instance.RemoveEvent<string>(AppConfig.EventKey.TIP, GetTip);
    }
}

