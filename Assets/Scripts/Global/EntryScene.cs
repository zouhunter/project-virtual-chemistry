using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class EntryScene : UIBehaviour {
    public Button startBtn;

    protected override void Awake()
    {
        base.Awake();
        startBtn.onClick.AddListener(LoadScene);
        GameManager.LunchFrameWork();
        startBtn.transform.DOShakeScale(3, 0.5f).SetLoops(-1);
    }

    private void LoadScene()
    {
        SceneData sceneData = new SceneData("Laboratory", UnityEngine.SceneManagement.LoadSceneMode.Single);
        SceneLoadCommond sceneLoad = new SceneLoadCommond();
        sceneLoad.Execute(sceneData);
        gameObject.SetActive(false);
    }
}

