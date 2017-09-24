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
    private bool isPresent;
    private Queue<PresentationData> queueData = new Queue<PresentationData>();
    private PresentationData activeData;
    void Start()
    {
        conferBtn.onClick.AddListener(OnConferBtnClikded);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        Time.timeScale = 0;
    }
    private void OnDisable()
    {
        Time.timeScale = 1;
    }
    void OnConferBtnClikded()
    {
        isPresent = false;
         
        if (activeData != null && activeData.onSelect != null) {
            activeData.onSelect.Invoke();
        }

        if(queueData.Count > 0)
        {
            TryInvoke();
        }
        else
        {
            Destroy(gameObject);
        }
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
            var data = (PresentationData)message;
            queueData.Enqueue(data);
            TryInvoke();
        }
    }
    private void TryInvoke()
    {
        if(!isPresent && queueData.Count > 0)
        {
            isPresent = true;
            activeData = queueData.Dequeue();
            LoadInfoToUI(activeData);
            SceneMain.Current.InvokeEvents<string>(AppConfig.EventKey.TIP, activeData.tip);
        }
    }
}
