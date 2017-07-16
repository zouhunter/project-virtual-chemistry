using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
[System.Serializable]

public class DoTweenPanel {
    public enum PanelTweenType
    {
        Scale,
        Move,
        Rotate
    }
    private bool isOpen;
    public bool IsOpen
    {
        get {
            return isOpen;
        }
        set
        {
            isOpen = value;
            Play(isOpen);
        }
    }

    public bool autoClose;
    public float m_animTime = 0.2f;
    public PanelTweenType type;
    public float startScale;

    private Tweener open;
    private Tweener close;
    private Transform panel;
    //public event UnityAction onPanelOpen;

    public void InitTween(Transform panel)
    {
        this.panel = panel;

        switch (type)
        {
            case PanelTweenType.Scale:
                RegisterScaleAnim();
                break;
            case PanelTweenType.Move:
                break;
            case PanelTweenType.Rotate:
                break;
            default:
                break;
        }

       
    }

    void RegisterScaleAnim()
    {
        open = panel.DOScale(startScale, m_animTime).SetRelative(false).From().OnPlay(() => panel.gameObject.SetActive(true)).Pause().SetAutoKill(false);
        close = panel.DOScale(startScale, m_animTime * 0.5f).SetRelative(false).OnComplete(() => { panel.DORewind(); panel.gameObject.SetActive(false); }).Pause().SetAutoKill(false);
    }
    public void Play(bool isOpen)
    {
        if (isOpen)
        {
            panel.gameObject.SetActive(true);
            if (autoClose) {
                GameManager.closeEvent += CloseEvent;
            }

            open.Restart();
            close.Pause();
        }
        else
        {
            open.Pause();
            close.Restart();
        }
    }

    private void CloseEvent()
    {
        IsOpen = false;
    }
}

