using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class PresentationData {
    public string title;
    public string infomation;
    public string tip;
    public UnityAction onSelect;

    /// <summary>
    /// 对象池
    /// </summary>
    public static ObjectPool<PresentationData> m_Pool = new ObjectPool<PresentationData>(1, 1);

    /// <summary>
    /// 得到
    /// </summary>
    /// <returns></returns>
    public static PresentationData Allocate(string title,string infomation,string tip,UnityAction onSelect =null)
    {
        PresentationData data = m_Pool.Allocate();
        data.tip = tip;
        data.infomation = infomation;
        data.title = title;
        data.onSelect = onSelect;
        return data;
    }

    /// <summary>
    /// 放回
    /// </summary>
    public void SaveBack()
    {
        title = default(string);
        infomation = default(string);
        tip = default(string);
        onSelect = null;
        m_Pool.Release(this);
    }

}
