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
    public Button nextBtn;
    public Button close;

    private ReactSystemCtrl _systemCtrl;
    public ConnectorCtrl groupParent;
    void Start()
    {
        startBtn.onClick.AddListener(RestartExperiment);
        interactBtn.onClick.AddListener(StartExperiment);
        nextBtn.onClick.AddListener(NextStep);
        close.onClick.AddListener(ClosePanel);
        _systemCtrl = new ReactSystemCtrl();
        _systemCtrl.GetConnectedDic = GetConnectedDic;
        _systemCtrl.GetSupportList = GetSupportList;
        _systemCtrl.InitExperiment(experimentData.elements);
        _systemCtrl.onComplete += () => { Debug.Log("Complete"); };
        _systemCtrl.onStepBreak += (x) => { Debug.Log("StepBreak" + x.Go.name); };

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
    List<ISupporter> GetSupportList(IContainer item)
    {
        var list = new List<ISupporter>();
        var nodeParent = item.Go.GetComponent<IPortParent>();
        List<IPortItem> nodeItems = null;
        if (groupParent.ConnectedDic.TryGetValue(nodeParent, out nodeItems))
        {
            foreach (var nodeItem in nodeItems)
            {
                var connectedItem = nodeItem.ConnectedNode.Body;
                var supporter = connectedItem.Trans.GetComponent<ISupporter>();
                if (supporter != null)
                {
                    list.Add(supporter);
                }
            }

        }
        return list;
    }

    Dictionary<IContainer, int> GetConnectedDic(IContainer item, int exportID)
    {
        var dic = new Dictionary<IContainer, int>();
        var nodeParent = item.Go.GetComponent<IPortParent>();
        List<IPortItem> nodeItems = null;
        if (groupParent.ConnectedDic.TryGetValue(nodeParent, out nodeItems))
        {
            var nodeItem = nodeItems.Find(x => x.NodeID == exportID);
            if (nodeItem != null)
            {
                var connectedItem = nodeItem.ConnectedNode.Body;
                var body = connectedItem.Trans.GetComponent<IContainer>();
                var id = nodeItem.ConnectedNode.NodeID;
                dic[body] = id;

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
