using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Tuples;
namespace ReactSystem
{
    /// <summary>
    /// 流动反应，并触发事件
    /// </summary>
    public class ReactSystemCtrl : IReactSystemCtrl
    {
        public IContainer ActiveItem
        {
            get
            {
                return activeItem;
            }
        }
        public Func<IContainer, int, Dictionary<IContainer, int>> GetConnectedDic { get; set; }
        public Func<IContainer, List<ISupporter>> GetSupportList { get; set; }
        private List<RunTimeElemet> elements;
        private IContainer activeItem;
        private Queue<IContainer> reactTuple = new Queue<IContainer>();
        private bool isReact;

        public event UnityAction onComplete;
        public event UnityAction<IContainer> onStepBreak;

        readonly List<IContainer> containers = new List<IContainer>();
        readonly List<ISupporter> supporters = new List<ISupporter>();
        public void InitExperiment(List<RunTimeElemet> elements)
        {
            this.elements = elements;
        }

        public void ReStart()
        {
            ResatToBeginState();

            GameObject item;
            IActiveAble inoutItem;
            for (int i = 0; i < elements.Count; i++)
            {
                RunTimeElemet element = elements[i];
                item = GameObject.Instantiate(element.element, element.position, element.rotation) as GameObject;
                item.name = element.name;

                inoutItem = item.GetComponent<IActiveAble>();
                if (inoutItem != null)
                {
                    if (inoutItem is IContainer)
                    {
                        var container = inoutItem as IContainer;
                        container.onExport += OnReact;
                        container.onComplete += OnOneStep;
                        container.onGetSupports += GetSupporter;
                        containers.Add(container);
                    }
                    else if(inoutItem is ISupporter)
                    {
                        supporters.Add(inoutItem as ISupporter);
                    }
                }

            }
        }

        public bool TryStartProducer()
        {
            if (elements == null) return false;
            if (GetConnectedDic == null) return false;
            foreach (var item in supporters)
            {
                item.Active();
            }
            foreach (var item in containers)
            {
                item.Active();
            }
            return true;
        }

        public void TryNextContainer()
        {
            if (isReact) return;

            if (reactTuple.Count > 0)
            {
                var container = reactTuple.Dequeue();
                container.Active(true);
                isReact = true;
            }
        }
        private List<string> GetSupporter(IContainer container)
        {
            var list = new List<string>();
            var supporters = GetSupportList(container);
            foreach (var item in supporters)
            {
                item.Active(true);
                var supp = item.GetSupport();
                if (supp != null)
                {
                    list.AddRange(supp);
                }
            }
            return list;
        }
        public void ResatToBeginState()
        {
            reactTuple.Clear();
            activeItem = null;
            foreach (var item in containers)
            {
                GameObject.Destroy(item.Go);
            }
            foreach (var item in supporters)
            {
                GameObject.Destroy(item.Go);
            }
            containers.Clear();
            supporters.Clear();
        }

        private bool OnReact(IContainer item, int id, string[] type)
        {
            activeItem = item;
            var outInfo = GetConnectedDic(activeItem, id);

            if (outInfo != null && outInfo.Count != 0)
            {
                foreach (var node in outInfo)
                {
                    var outInoutItem = node.Key;
                    var outInoutId = node.Value;
                    outInoutItem.Import(outInoutId, type);
                    reactTuple.Enqueue(outInoutItem);
                }
            }
            else
            {
                if (onStepBreak != null) onStepBreak.Invoke(activeItem);
            }

            return outInfo != null && outInfo.Count != 0;
        }
        private void OnOneStep(IContainer item)
        {
            isReact = false;
            if (reactTuple.Count == 0)
            {
                if (onComplete != null) onComplete.Invoke();
            }
        }

    }
}