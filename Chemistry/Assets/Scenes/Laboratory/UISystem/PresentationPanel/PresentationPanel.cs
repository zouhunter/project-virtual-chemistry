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

    PresentationData data;

    void Start()
    {
        conferBtn.onClick.AddListener(OnConferBtnClikded);
    }

    void OnConferBtnClikded()
    {
        if(data!= null &&data.onSelect != null)
        {
            data.onSelect.Invoke();
        }
        Time.timeScale = 1;
        Destroy(gameObject);
    }

    void LoadInfoToUI(PresentationData currentPresent)
    {
        presentation.gameObject.SetActive(true);//以前不用加这个的，为什么Dotween有这个 问题呢

        textinfo.text = currentPresent.infomation;
        //StartCoroutine(Resize(textinfo));

        titleText.text = currentPresent.title;
    }

    //IEnumerator Resize(Text target)
    //{
    //    target.resizeTextForBestFit = false;
    //    yield return new WaitForEndOfFrame();
    //    target.GetComponent<LayoutElement>().preferredHeight = target.preferredHeight;
    //}

    public override void HandleData(object message)
    {
        if (message is PresentationData)
        {
            Debug.Log(message);
            data = (PresentationData)message;
            LoadInfoToUI(data);
            SceneMain.Current.InvokeEvents<string>( AppConfig.EventKey.TIP, data.tip);
            Time.timeScale = 0;
        }
    }
}
