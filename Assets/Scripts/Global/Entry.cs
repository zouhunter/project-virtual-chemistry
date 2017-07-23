using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class Entry : SceneMain<Entry> {
    public Button startBtn;

    protected override void Awake()
    {
        program.StartGame();
        base.Awake();
        startBtn.onClick.AddListener(LoadScene);
        startBtn.transform.DOShakeScale(3, 0.5f).SetLoops(-1);
    }

    private void LoadScene()
    {
        SceneData sceneData = new SceneData("Laboratory", UnityEngine.SceneManagement.LoadSceneMode.Single);
        Facade.SendNotification<SceneData>("sceneLoad", sceneData);
    }
}

