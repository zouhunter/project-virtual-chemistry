using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem;
using Connector;
using ReactSystem;
public class ReactSystemPanel : UIPanelTemp
{
    public ReactGroupObj experimentData;

    public Button startBtn;
    public Button interactBtn;
    public Button close;
    public InputField.SubmitEvent onStepBreak;
    public Button.ButtonClickedEvent onComplete;

    private ReactSystemCtrl _systemCtrl;
    public ConnectorCtrl groupParent;
    void Start()
    {
        startBtn.onClick.AddListener(RestartExperiment);
        interactBtn.onClick.AddListener(StartExperiment);
        close.onClick.AddListener(ClosePanel);
        _systemCtrl = new ReactSystemCtrl();
        _systemCtrl.GetConnectedDic = GetConnectedDic;
        _systemCtrl.GetConnectedList = GetSupportList;
        _systemCtrl.InitExperiment(experimentData.elements);
        _systemCtrl.onComplete += () => { onComplete.Invoke(); Debug.Log("Complete"); };
        _systemCtrl.onStepBreak += (x) => { onStepBreak.Invoke("步骤中断!"); Debug.Log("StepBreak" + x.Go.name); };
        _systemCtrl.onStepComplete += NextStep;
        RestartExperiment();
    }
    public override void HandleData(object data)
    {
        base.HandleData(data);
        if(data != null && data is ReactGroupObj)
        {
            experimentData = (ReactGroupObj)data;
        }
    }
    private void Update()
    {
        groupParent.Update();
    }
    List<IElement> GetSupportList(IElement item)
    {
        var list = new List<IElement>();
        var nodeParent = item.Go.GetComponent<IPortParent>();
        List<IPortItem> nodeItems = null;
        if (groupParent.ConnectedDic.TryGetValue(nodeParent, out nodeItems))
        {
            foreach (var nodeItem in nodeItems)
            {
                var connectedItem = nodeItem.ConnectedNode.Body;
                var element = connectedItem.Trans.GetComponent<IElement>();
                if (element != null){
                    list.Add(element);
                }
            }

        }
        return list;
    }

    KeyValuePair<IElement, int> GetConnectedDic(IElement item, int exportID)
    {
        var dic = new KeyValuePair<IElement, int>();
        var nodeParent = item.Go.GetComponent<IPortParent>();
        List<IPortItem> nodeItems = null;
        if (groupParent.ConnectedDic.TryGetValue(nodeParent, out nodeItems))
        {
            var nodeItem = nodeItems.Find(x => x.NodeID == exportID);
            if (nodeItem != null)
            {
                var connectedItem = nodeItem.ConnectedNode.Body;
                var body = connectedItem.Trans.GetComponent<IElement>();
                var id = nodeItem.ConnectedNode.NodeID;
                dic = new KeyValuePair<IElement, int>(body, id);
            }
        }
        return dic;
    }

    void RestartExperiment()
    {
        _systemCtrl.ReStart();
        groupParent.Start();
    }

    void StartExperiment()
    {
        var ok = _systemCtrl.TryStartProducer();
        if (!ok)
        {
            Debug.LogError("启动失败");
        }
    }

    void NextStep()
    {
        _systemCtrl.TryNextContainer();
    }
    void ClosePanel()
    {
        _systemCtrl.ResatToBeginState();
        Destroy(gameObject);
    }
}
