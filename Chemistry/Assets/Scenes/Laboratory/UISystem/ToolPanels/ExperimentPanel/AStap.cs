using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class AStap : MonoBehaviour {
    public Text stapIndex;
    public InputField stapName;
    public InputField tipInfo;
    public Slider time;
    public Toggle needRecord;
    public ElemntsList elementList;
    public InfomationContain infoContain;

    private StapInfo stapInfo;
    void Start(){
        needRecord.onValueChanged.AddListener(ChangeInstallState);
    }

    /// <summary>
    /// 改变状态
    /// </summary>
    /// <param name="needInstall"></param>
    void ChangeInstallState(bool needInstall)
    {
        elementList.gameObject.SetActive(needInstall);
        infoContain.gameObject.SetActive(!needInstall);
    }
    /// <summary>
    /// 加载当前步骤信息
    /// </summary>
    public void LoadStapInfoMation(StapInfo stapInfo)
    {
        this.stapInfo = stapInfo;
        stapName.text = stapInfo.name;
        stapIndex.text = string.Format("第{0}步",stapInfo.index + 1);
        tipInfo.text = stapInfo.tipInfo;
        needRecord.isOn = stapInfo.needOperate;
        time.value = stapInfo.showTime;
        infoContain.inputFiled.text = stapInfo.infomation;
        elementList.SetInstalls(stapInfo.installs);
    }
    /// <summary>
    /// 保存当前信息
    /// </summary>
    public void SaveCurrentInfomation()
    {
        stapInfo.name = stapName.text;
        stapInfo.tipInfo = tipInfo.text;
        stapInfo.needOperate = needRecord.isOn;
        stapInfo.showTime = (int)time.value;
        stapInfo.infomation = infoContain.GetInfomation();
        //stapInfo.installs = elementList.GetInstalls();
        Debug.Log("保存步骤");
    }
}
