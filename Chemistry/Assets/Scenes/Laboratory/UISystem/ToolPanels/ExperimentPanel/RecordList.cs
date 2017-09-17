using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class RecordList
{
    /// <summary>
    /// 上一步
    /// </summary>
    public void OnLastStapClicked()
    {
        if (indexCtrl.LastStap())
        {
            SetCurrStap(indexCtrl.currStap);
        }
        SaveCurrStap();
    }

    /// <summary>
    /// 下一步
    /// </summary>
    public void OnNextStapClicked()
    {
        if (indexCtrl.NextStap())
        {
            SetCurrStap(indexCtrl.currStap);
        }
        SaveCurrStap();
    }
    
}

public partial class RecordList : MonoBehaviour {
    /// <summary>
    /// 步数信息
    /// </summary>
    public Text stapShow;
    public AStap aStap;

    public Button lastBtn;
    public Button nextBtn;

    private ListIndexCtrl indexCtrl;
    //private Experiment experiment;
    /// <summary>
    /// 加载实验配制
    /// </summary>
    /// <param name="experiment"></param>
    public void LoadExperiment(Experiment experiment)
    {
        //this.experiment = experiment;
        if (experiment.name == null || experiment.name == "")
        {
            experiment.staps = new List<StapInfo>();
            experiment.staps.Add(new StapInfo());
        }
        indexCtrl = new ListIndexCtrl(experiment);
        SetCurrStap(indexCtrl.currStap);
    }
    /// <summary>
    /// 记录当前步骤
    /// </summary>
    /// <param name="stapInfo"></param>
    public void RecordAStap()
    {
        SaveCurrStap();
        StapInfo stapInfo = new global::StapInfo();
        indexCtrl.InsertAStap(stapInfo);
        SetCurrStap(stapInfo);
    }
    /// <summary>
    /// 移除一步
    /// </summary>
    public void RemoveSatp()
    {
        if(indexCtrl.RemoveAStap())
        {
            SetCurrStap(indexCtrl.currStap);
        }
    }

    public void SaveCurrStap()
    {
        //保存当前信息
        aStap.SaveCurrentInfomation();
    }
    /// <summary>
    /// 加载当前步骤
    /// </summary>
    /// <param name="stap"></param>
    void SetCurrStap(StapInfo stap)
    {
        ShowTextAndButton();
        aStap.gameObject.SetActive(true);
        aStap.LoadStapInfoMation(stap);
    }

    /// <summary>
    /// 加载完成后显示当前步骤信息
    /// </summary>
    void ShowTextAndButton()
    {
        int curr;
        int total;
        int state = indexCtrl.GetStapProgress(out curr, out total);
        stapShow.text = string.Format("{0}/{1}", curr + 1, total);
        #region button的可点击性
        if (state == -2){
            lastBtn.interactable = false;
            nextBtn.interactable = false;
        }
        else if(state == -1)
        {
            lastBtn.interactable = false;
            nextBtn.interactable = true;
        }
        else if (state == 1)
        {
            lastBtn.interactable = true;
            nextBtn.interactable = false;
        }
        else
        {
            lastBtn.interactable = true;
            nextBtn.interactable = true;
        }
        #endregion
    }

   
}
