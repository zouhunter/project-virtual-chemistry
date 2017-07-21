using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using System;
using UnityEngine.Events;
using BundleUISystem;

public class PresentationPanel : UIPanelTemp
{
    public bool loadSprteFromResource = true;
    public Transform presentation;
    public Text titleText;
    public Text textinfo;
    public Button conferBtn;
    

    //public ToggleDoTweenPanel tweenCtrl;

    void Start()
    {
        conferBtn.onClick.AddListener(OnConferBtnClikded);
    }

    void OnConferBtnClikded()
    {
        Destroy(gameObject);
    }

    void LoadInfoToUI(PresentationData currentPresent)
    {
        presentation.gameObject.SetActive(true);//以前不用加这个的，为什么Dotween有这个 问题呢

        textinfo.text = currentPresent.infomation;
        StartCoroutine(Resize(textinfo));

        titleText.text = currentPresent.title;
    }

    IEnumerator Resize(Text target)
    {
        target.resizeTextForBestFit = false;
        yield return new WaitForEndOfFrame();
        target.GetComponent<LayoutElement>().preferredHeight = target.preferredHeight;
    }

    public override void HandleData(object message)
    {
        if (message is PresentationData)
        {
            PresentationData stapInfo =(PresentationData)message;
            LoadInfoToUI(stapInfo);
            //EventFacade.Instance.SendNotification<string>( AppConfig.EventKey.TIP, stapInfo.tip);
        }
    }
}
