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
    /// 流动反应，并触发事件
    /// </summary>
    public class FlowSystemCtrl : IFlowSystemCtrl
    {
        private ElementGroup groupPrefab;
        private ElementGroup group;
        private string experimentName;
        private List<RunTimeElemet> elements;
        readonly List<GameObject> loadedItems = new List<GameObject>();

        public IInOutItem ActiveItem
        {
            get
            {
                return activeItem;
            }
        }

        public string ExperimentName
        {
            get
            {
                return experimentName;
            }
        }

        private IInOutItem activeItem;
        private Queue<Tuple<IInOutItem, int, string>> reactTuple = new Queue<Tuple<IInOutItem, int, string>>();
        private Queue<IInOutItem> startActive = new Queue<IInOutItem>();
        public FlowSystemCtrl(ElementGroup group)
        {
            this.groupPrefab = group;
        }

        public void InitExperiment(ExperimentDataObject objectData)
        {
            experimentName = objectData.expName;
            elements = objectData.elements;
        }

        public void ReStart()
        {
            ResatToBeginState();

            GameObject item;
            IInOutItem inoutItem;
            for (int i = 0; i < elements.Count; i++)
            {
                RunTimeElemet element = elements[i];
                item = GameObject.Instantiate(element.element, element.position, element.rotation) as GameObject;
                item.transform.SetParent(group.transform);
                inoutItem = item.GetComponent<IInOutItem>();
                loadedItems.Add(item);

                if (element.startActive)
                {
                    startActive.Enqueue(inoutItem);
                }
            }
        }

        private void ResatToBeginState()
        {
            activeItem = null;
            reactTuple.Clear();
            loadedItems.Clear();
            if (group != null)
            {
                GameObject.Destroy(group.gameObject);
            }
            group = GameObject.Instantiate<ElementGroup>(groupPrefab);
        }


        public bool StartProducer()
        {
            if (startActive.Count > 0)
            {
                while (startActive.Count > 0)
                {
                    IInOutItem item = startActive.Dequeue();
                    if (item.AutoReact())
                    {
                        activeItem = item;
                    }
                }
                return true;
            }
            else
            return false;
        }

        public bool NextContainer(UnityAction onComplete)
        {
            if (ActiveItem == null) return false;

            ActiveItem.RecordReactInfo(this);

            if (reactTuple.Count > 0)
            {
                Tuple<IInOutItem, int, string> item = reactTuple.Dequeue();
                activeItem = item.Element1;
                activeItem.FunctionIn(item.Element2, item.Element3,onComplete);
                return true;
            }
            return false;
        }

        public void AddActiveItem(IInOutItem item, int nodeID, string type)
        {
            reactTuple.Enqueue(new Tuple<IInOutItem, int, string>(item, nodeID, type));
        }

    }
}