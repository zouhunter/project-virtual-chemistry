using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using FlowSystem;
using BundleUISystem;
/// <summary>
/// 负责实验的初始化，加载元素，重置元素
/// </summary>
public class FlowSystemPanel : UIPanelTemp
{
    public ExperimentDataObject experimentData;

    public bool autoStart;
    public bool autoNext;
    public Button startBtn;
    public Button interactBtn;
    public Button nextBtn;
    public Button closeBtn;
    FlowSystemCtrl _systemCtrl;


    public ElementGroup groupParent;
    void Start()
    {
        _systemCtrl = new FlowSystemCtrl(groupParent);
        _systemCtrl.InitExperiment(experimentData);

        startBtn.onClick.AddListener(RestartExperiment);
        interactBtn.onClick.AddListener(StartExperiment);
        nextBtn.onClick.AddListener(NextExperiment);
        closeBtn.onClick.AddListener(OnCloseSystem);
        if (autoNext) nextBtn.gameObject.SetActive(false);
        if (autoStart) startBtn.onClick.Invoke();
    }

    void RestartExperiment()
    {
        interactBtn.interactable = true;
        nextBtn.interactable = false;
        _systemCtrl.ReStart();
    }
    void StartExperiment()
    {
        _systemCtrl.StartProducer();
        interactBtn.interactable = false;

        if (!_systemCtrl.NextContainer(()=> { if (autoNext) StartExperiment(); else { nextBtn.interactable = true; } }))
        {
            SceneMain.Current.InvokeEvents<string>(AppConfig.EventKey.TIP, "实验结束");
        }
    }
    void NextExperiment()
    {
       var haveNext = _systemCtrl.NextContainer(() => { nextBtn.interactable = true; });
        if (!haveNext)
        {
            nextBtn.interactable = false;
            SceneMain.Current.InvokeEvents<string>(AppConfig.EventKey.TIP, "实验结束");
        }
    }
    void OnCloseSystem()
    {
        _systemCtrl.QuitSystem();
        Destroy(gameObject);
    }
}
