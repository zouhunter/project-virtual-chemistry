using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class EntryScene : UIBehaviour {
    public Button startBtn;
    public Button quitBtn;

    protected override void Awake()
    {
        base.Awake();
        startBtn.onClick.AddListener(LoadScene);
        quitBtn.onClick.AddListener(QuitGame);
        GameManager.LunchFrameWork();
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void LoadScene()
    {
        SceneData sceneData = new SceneData("Laboratory", UnityEngine.SceneManagement.LoadSceneMode.Single);
        SceneLoadCommond sceneLoad = new SceneLoadCommond();
        sceneLoad.Execute(sceneData);
        gameObject.SetActive(false);
    }
}

