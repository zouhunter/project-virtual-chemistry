using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class ElemntsList
{
    /// <summary>
    /// 记录当前场景中数据
    /// </summary>
    public void OnRecordClicked()
    {
        installsHandle.Clear();
        ElementInstall element;
        foreach (Transform item in installParent)
        {
            element = new global::ElementInstall();
            element.name = item.name;
            element.translation.position = item.position;
            element.translation.rotation = item.eulerAngles;
            installsHandle.Add(element);
        }
        ClearUIPanel();
        LoadToUIPanel();
    }
    /// <summary>
    /// 移除记录
    /// </summary>
    public void OnRemoveClicked()
    {
        installsHandle.Clear();
        ClearUIPanel();
    }
}

public partial class ElemntsList : MonoBehaviour {
    private Transform installParent { get { return Laboratory.Current.installParent; } }
    private List<ElementInstall> installsHandle;
    public DetailPanel detailPanel;
    public GameObject itemPfb;
    private List<GameObject> objectList = new List<GameObject>();
    
    public void SetInstalls(List<ElementInstall> installs)
    {
        this.installsHandle = installs;//引用类型
        ClearUIPanel();
        LoadToUIPanel();
    }
    /// <summary>
    /// 加载出UI界面
    /// </summary>
    private void LoadToUIPanel()
    {
        GameObject item;
        ElementInstall insatll;
        for (int i = 0; i < installsHandle.Count; i++)
        {
            insatll = installsHandle[i];
            item = ObjectManager.Instance.GetPoolObject(itemPfb, itemPfb.transform.parent, false);
            item.GetComponent<ElementItem>().haveAnim.isOn = insatll.haveAnim;
            item.GetComponent<ElementItem>().clearNext.isOn = insatll.clearNext;
            item.GetComponent<ElementItem>().moveSwit.isOn = insatll.moveQuick;
            Translation trans = insatll.translation;
            item.GetComponent<Button>().onClick.AddListener(()=> { detailPanel.SetPositionAndRotation(trans); });
            objectList.Add(item);
        }
    }
    /// <summary>
    /// 清除ui界面
    /// </summary>
    private void ClearUIPanel()
    {
        GameObject item;
        for (int i = 0; i < objectList.Count; i++)
        {
            item = objectList[i];
            item.GetComponent<Button>().onClick.RemoveAllListeners();
            item.GetComponent<ElementItem>().haveAnim.isOn = false;
            item.GetComponent<ElementItem>().clearNext.isOn = false;
            item.GetComponent<ElementItem>().moveSwit.isOn = false;
            ObjectManager.Instance.SavePoolObject(item, false);
        }
        objectList.Clear();
    }
}
