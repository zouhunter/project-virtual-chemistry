using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using uTween;

public class PanelTween01 : MonoBehaviour {

    public enum TweenType{
        ScalePanel,
        PosUpPanel,
        RotatePanel,
    }

    public TweenType type;
    public RectTransform panel;
    public bool quickClose = true;
    private float duration = 0.2f;

    private uTweener tween;
    private UnityAction<bool> playEvent;
    private bool targetBool;
    void Start()
    {
        switch (type)
        {
            case TweenType.ScalePanel:
                tween = uTweenScale.Begin(panel, Vector3.one * 0.8f, panel.localScale , duration);
                break;
            case TweenType.PosUpPanel:
                tween = uTweenPosition.Begin(panel,Vector3.left * 200 , Vector3.zero, duration);
                break;
            case TweenType.RotatePanel:
                tween = uTweenRotation.Begin(panel, Vector3.up * 30, Vector3.zero, duration);
                break;
            default:
                break;
        }
        tween.AddOnFinished(OnFinish);
        panel.gameObject.SetActive(false);

        if (playEvent != null)
        {
            playEvent(targetBool);
            playEvent = null;
        }
    }

    void Update()
    {
        tween.Update();
    }

    void OnFinish()
    {
        if (tween.direction == Direction.Reverse)
        {
            panel.gameObject.SetActive(false);
        }
    }

    public void TogglePlay(bool isOn)
    {
        if (tween == null)
        {
            targetBool = isOn;
            playEvent = TogglePlay;
            return;
        }

        if(isOn)
        {
            panel.gameObject.SetActive(true);
            tween.PlayForward();
        }
        else
        {
            if (quickClose)
            {
                panel.gameObject.SetActive(false);
            }
            else
            {
                tween.PlayReverse();
            }
        }
    }

    public void PlayForward()
    {
        TogglePlay(true);
    }

    public void PlayBackward()
    {
        TogglePlay(false);
    }
}
