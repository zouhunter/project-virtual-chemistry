using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem;
using Connector;
using ReactSystem;
public class ExperimentChoisePanel : UIPanelTemp
{
    [SerializeField]
    private Button cancle;
    [SerializeField]
    private Button cl2;
    [SerializeField]
    private Button h2so4Cu;
    [SerializeField]
    private Button feso4;

    [SerializeField]
    private Button other1;
    [SerializeField]
    private Button other2;

    public ReactGroupObj cudata;
    public ReactGroupObj fedata;
    public WorldActionSystem.ActionCommand command;


    private void Awake()
    {
        cl2.onClick.AddListener(OpenCl2);
        h2so4Cu.onClick.AddListener(OpenH2so4Cu);
        feso4.onClick.AddListener(OpenFeso4);
        cancle.onClick.AddListener(Close);
        other1.onClick.AddListener(() => OnOtherButtonClicked(other1));
        other2.onClick.AddListener(() => OnOtherButtonClicked(other2));
    }

    private void OnOtherButtonClicked(Button other1)
    {
        var text = other1.GetComponentInChildren<Text>();
        PresentationData data = new global::PresentationData();
        data.title = "温馨提示";
        data.infomation = string.Format("实验{0}还未进行配制", text.text);
        UIGroup.Open("PresentationPanel", data);
    }

    private void Close()
    {
        Destroy(gameObject);
    }

    private void OpenCl2()
    {
        UIGroup.Open("FlowSystemPanel");
        Close();
    }

    private void OpenH2so4Cu()
    {
        UIGroup.Open("ReactSystemPanel",cudata);
        Close();
    }

    private void OpenFeso4()
    {
        UIGroup.Open("ActionPanel", command);
        Close();
    }


}
