﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

using BundleUISystem;

public class PrefabItemPanel : UIPanelTemp
{
    private Transform createParent { get { return Laboratory.Current.installParent; } }
    //private Toggle open;
    public GameObject panel;
    public PrefabItemObject items;
    public GameObject btnPfb;
    public BuildingCtrl buildCtrl { get { return Laboratory.Current.buildCtrl; } }


    public string perfabPath;

    private List<GameObject> created = new List<GameObject>();

    protected override  void OnEnable()
    {
        base.OnEnable();
        Laboratory.onOperateTypeChanged += OnOperateTypeChanged;
    }
    void Start()
    {
        SceneMain.Current.RegisterEvent(AppConfig.EventKey.ClickEmpty, Close);
        GameObject btn;
        PrefabItem item;
        for (int i = 0; i < items.prefabItem.Count; i++)
        {
            item = items.prefabItem[i];
            btn = Instantiate(btnPfb);
            btn.transform.SetParent(btnPfb.transform.parent, false);
            btn.GetComponentInChildren<Text>().text = item.prefabName;
            btn.GetComponent<Image>().sprite = item.sprite;
            GameObject perfab = Resources.Load<GameObject>(perfabPath + item.prefabName);
            if (perfab != null)
            {
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnItemButtonClicked(perfab);
                });
            }
        }
        Destroy(btnPfb);
    }

    void OnDisable()
    {
        Laboratory.onOperateTypeChanged -= OnOperateTypeChanged;
    }

    void OnItemButtonClicked(GameObject perfab)
    {
        switch (Laboratory.operateType)
        {
            case OperateType.Config:
                created.Add(buildCtrl.OnCreateButtonClicked(perfab, createParent));
                break;
            case OperateType.Domon:
                created.Add(buildCtrl.OnCreateButtonClicked(perfab, createParent));
                //InstanceUtility.MouseBehavierInstance(perfab, createParent);
                break;
            case OperateType.Operate:
                created.Add(buildCtrl.OnCreateButtonClicked(perfab, createParent));
                //InstanceUtility.MouseBehavierInstance(perfab, createParent);
                break;
            default:
                break;
        }
        SceneMain.Current.InvokeEvents(AppConfig.EventKey.ClickEmpty);
    }
    void Close()
    {
        SceneMain.Current.RemoveEvent(AppConfig.EventKey.ClickEmpty, Close);
        Destroy(gameObject);
    }
    /// <summary>
    /// 模式改变是清除创建的对象
    /// </summary>
    /// <param name="type"></param>
    void OnOperateTypeChanged(OperateType type)
    {
        BuildingItem buildItem;
        foreach (var item in created)
        {
            buildItem = item.GetComponent<BuildingItem>();
            buildCtrl.RemoveBuilding(buildItem);
            DestroyImmediate(buildItem);
            ObjectManager.Instance.SavePoolObject(item,true);
        }
        created.Clear();
    }
}

