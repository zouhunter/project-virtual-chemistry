using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Tuples;
namespace FlowSystem
{
    /// <summary>
    /// 进入和输出事件处理,节点关联功能
    /// </summary>
    public class InOutItemBehaiver : MonoBehaviour, IInOutItem, INodeParent
    {
        public ItemInfo itemInfo;

        public AutoRact autoRact;
        public List<Interact> interact = new List<Interact>();
        private List<INodeItem> nodeItems = new List<INodeItem>();
        public Transform Trans {
            get { return transform;
            }
        }

        private IInteract activeRact;
        private List<int> functionInNodes = new List<int>();
        private UnityAction onComplete;
        private Dictionary<IInteract,TimeSpanInfo> timeSpanDic = new Dictionary<IInteract, TimeSpanInfo>();

        public string Name
        {
            get
            {
                return itemInfo.name;
            }
        }

        public List<INodeItem> ChildNodes
        {
            get
            {
                return nodeItems;
            }
        }

        void Start()
        {
            RegisterNodes();
            if (itemInfo.autoRact)
            {
                activeRact = autoRact;
            }
        }

        void Update()
        {
            if (activeRact == null){
                return;
            }
            if (!timeSpanDic.ContainsKey(activeRact)){
                timeSpanDic.Add(activeRact, new TimeSpanInfo(activeRact.RactTime));
            }
            if (timeSpanDic[activeRact].OnSpanComplete())
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                    activeRact = null;
                }
            }
        }

        private void RegisterNodes()
        {
            INodeItem nodeItem;
            foreach (Transform item in transform)
            {
                nodeItem = item.GetComponent<INodeItem>();
                if (nodeItem != null)
                {
                    nodeItem.Body = this;
                    nodeItems.Add(nodeItem);
                }
            }
        }
        /// <summary>
        /// 输入并反应
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="type"></param>
        /// <param name="onComplete"></param>
        public void FunctionIn(int nodeId, string type,UnityAction onComplete)
        {
            this.onComplete = onComplete;
            functionInNodes.Add(nodeId);
            activeRact = interact.Find((x) => x.nodeID == nodeId && x.intype == type);
            if (activeRact != null)
            {
                activeRact.Interact.Invoke();
                //EventFacade.Instance.SendNotification(AppConfig.EventKey.TIP, activeRact.Information);
            }
            else
            {
                //PresentationData data = PresentationData.Allocate("<color=red>警告</color>", name + "没有配制对应的输入类型" + type, "");
                //EventFacade.Instance.SendNotification(AppConfig.EventKey.OPEN_PRESENTATION, data);
            }
        }

        public void ResetBodyTransform(INodeParent otherParent, Vector3 rPos, Vector3 rDir)
        {
            transform.position = otherParent.Trans.TransformPoint(rPos);
            transform.forward = otherParent.Trans.TransformDirection(rDir);
        }

        public bool AutoReact()
        {
            activeRact = autoRact;
            autoRact.autoExportEvent.Invoke();
            if (!string.IsNullOrEmpty(activeRact.OutType))
            {
                //EventFacade.Instance.SendNotification(AppConfig.EventKey.TIP, autoRact.information);
                return true;
            }
            return false;
        }

        public void RecordReactInfo(IFlowSystemCtrl ctrl)
        {
            Debug.Log(name);
            ///没有反应对象，直接跳过
            if (activeRact == null){
                return;
            }
            //没有输出内容，直接跳过
            if (string.IsNullOrEmpty(activeRact.OutType))
            {
                return;
            }

            //遍历记录输出信息
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                if (ChildNodes[i].Info.enterOnly || functionInNodes.Contains(ChildNodes[i].Info.nodeID))
                {
                    continue;
                }

                if (ChildNodes[i].ConnectedNode != null)
                {
                    IInOutItem inout = ChildNodes[i].ConnectedNode.Body.Trans.GetComponent<IInOutItem>();

                    ctrl.AddActiveItem(inout, ChildNodes[i].ConnectedNode.Info.nodeID, activeRact.OutType);
                }
                else 
                {
                    activeRact.InteractOutPutFiled.Invoke();
                    //PresentationData data = PresentationData.Allocate("警告", activeRact.outPutFiledinformation, "");
                    //EventFacade.Instance.SendNotification(AppConfig.EventKey.OPEN_PRESENTATION, data);
                }
            }
        }
    }
}