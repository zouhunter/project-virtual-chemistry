using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 实现对实验的宏观操作
/// </summary>
public partial class ExpConfigPanel
{
    /// <summary>
    /// 保存配制
    /// </summary>
    public void OnSaveBtnClicked()
    {
        if (string.IsNullOrEmpty(title.text))
        {
            //Facade.Instance.SendNotification<string>("tipInfo", "请填写实验名称");
        }
        else
        {
            recordlist.SaveCurrStap();
            operateExperiment.name = title.text;
            operateExperiment.CopyTo(handleExperiment);
            if (!playerExperiments.experiments.Contains(handleExperiment))
            {
                playerExperiments.experiments.Add(handleExperiment);
            }
            OnQuitBtnClicked();
        }
    }

    /// <summary>
    /// 记录一步
    /// </summary>
    public void OnRecordBtnClicked()
    {
        recordlist.RecordAStap();
    }

    /// <summary>
    /// 移除一步
    /// </summary>
    public void OnRemoveBtnClicked()
    {
        //handleExperiment.CopyTo(operateExperiment);
        //LoadExperiment(operateExperiment);
        recordlist.RemoveSatp();
    }
    /// <summary>
    /// 关闭配制面版
    /// </summary>
    public void OnQuitBtnClicked()
    {
        gameObject.SetActive(false);
        handPanel.SetActive(false);
        Laboratory.ChangedOperateType(OperateType.Nil);
    }
}

public partial class ExpConfigPanel : MonoBehaviour
{
    public GameObject handPanel;
    public RecordList recordlist;
    public InputField title;
    public ExperimentObject playerExperiments;

    private Experiment handleExperiment;
    private Experiment operateExperiment = new Experiment();

    /// <summary>
    /// 配制实验
    /// </summary>
    /// <param name="experiment"></param>
    public void LoadExperiment(Experiment experiment)
    {
        if (experiment == null)
        {
            handleExperiment = new Experiment();
        }
        else
        {
            handleExperiment = experiment;
        }
        handleExperiment.CopyTo(operateExperiment);
        recordlist.LoadExperiment(operateExperiment);
        title.text = operateExperiment.name??"";
    }
}

