using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Experiment
{
    public string name;
    public List<StapInfo> staps = new List<StapInfo>();

    public void CopyTo(Experiment other)
    {
        other.name = name;
        other.staps.Clear();
        StapInfo newStap;
        for (int i = 0; i < staps.Count; i++)
        {
            newStap = new StapInfo();
            staps[i].CopyTo(newStap);
            other.staps.Add(newStap);
        }
    }
}

[System.Serializable]
public class StapInfo {
    public int index = 0;
    public string name = "";
    public string tipInfo = "";
    public string infomation = "";
    public int showTime = 0;
    public bool needOperate = false;
    public List<ElementInstall> installs = new List<ElementInstall>();

    public void CopyTo(StapInfo other)
    {
        other.index = index;
        other.name = name;
        other.tipInfo = tipInfo;
        other.infomation = infomation;
        other.showTime = showTime;
        other.needOperate = needOperate;
        other.installs.Clear();
        ElementInstall install;
        for (int i = 0; i < installs.Count; i++)
        {
            install = installs[i];
            other.installs.Add(install);
        }
    }
}

[System.Serializable]
public struct ElementInstall
{
    public string name;
    public bool clearNext;
    public bool haveAnim;
    public bool moveQuick;
    public Translation translation;
}

[System.Serializable]
public struct Translation
{
    public Vector3 position;
    public Vector3 rotation;
}
