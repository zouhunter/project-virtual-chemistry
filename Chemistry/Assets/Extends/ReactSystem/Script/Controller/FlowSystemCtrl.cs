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
        public Func<IElement, int, KeyValuePair<IElement, int>> GetConnectedDic { get; set; }
        public Func<IElement, List<IElement>> GetConnectedList { get; set; }
        public event UnityAction onComplete;
        public event UnityAction<IElement> onStepBreak;
        public event UnityAction onStepComplete;

        private List<RunTimeElemet> elements;
        private Queue<Tuple<IContainer, int, string[]>> reactTuple = new Queue<Tuple<IContainer, int, string[]>>();
        private bool isReact;

        readonly List<IContainer> containers = new List<IContainer>();
        readonly List<ISupporter> supporters = new List<ISupporter>();
        readonly List<GameObject> created = new List<GameObject>();
        public void InitExperiment(List<RunTimeElemet> elements){
            this.elements = elements;
        }

        public void ReStart()
        {
            ResatToBeginState();

            GameObject item;
            IElement element;
            for (int i = 0; i < elements.Count; i++)
            {
                RunTimeElemet runele = elements[i];
                item = GameObject.Instantiate(runele.element, runele.position, runele.rotation) as GameObject;
                item.name = runele.name;
                element = item.GetComponent<IElement>();
                if (element != null)
                {
                    created.Add(element.Go);
                    if (element is ITube)
                    {
                        var tube = element as ITube;
                        tube.onExport += OnReact;

                        if(element is IContainer)
                        {
                            var container = element as IContainer;
                            container.onComplete += OnOneStep;
                            container.onGetSupports += GetSupporter;
                            containers.Add(container);
                        }
                    }
                    else if(element is ISupporter)
                    {
                        supporters.Add(element as ISupporter);
                    }
               
                }

            }
        }

        public bool TryStartProducer()
        {
            if (elements == null) return false;
            if (GetConnectedDic == null) return false;
            var active = false;
            foreach (var item in supporters){
                var statu = item.Active();
                active |= statu;
            }

            foreach (var item in containers)
            {
                var statu = item.Active();
                active |= statu;
            }
            return active;
        }

        public void TryNextContainer()
        {
            if (isReact) return;

            if (reactTuple.Count > 0)
            {
                var container = reactTuple.Dequeue();
                Debug.Log("container active:" + container.Element1);
                container.Element1.Import(container.Element2, container.Element3);
                container.Element1.Active(true);
                isReact = true;
            }
        }

        private List<string> GetSupporter(IContainer container)
        {
            var list = new List<string>();
            var connected = GetConnectedList(container);
            foreach (var item in connected)
            {
                if(item is ISupporter)
                {
                    var support = item as ISupporter;
                    support.Active(true);
                    var supp = support.GetSupport();
                    if (supp != null){
                        list.AddRange(supp);
                    }
                }
               
            }
            return list;
        }

        public void ResatToBeginState()
        {
            foreach (var item in created){
                GameObject.Destroy(item);
            }
            created.Clear();
            reactTuple.Clear();
            containers.Clear();
            supporters.Clear();
        }

        private bool OnReact(ITube activeItem, int id, string[] type)
        {
            var connectedInfo = GetConnectedDic(activeItem, id);

            if(connectedInfo.Key != null && connectedInfo.Key is ITube)
            {
                var tube = connectedInfo.Key as ITube;
                var outInoutId = connectedInfo.Value;

                if(tube is IContainer)
                {
                    //等待化学反应结束
                    reactTuple.Enqueue(new Tuple<IContainer, int, string[]>( tube as IContainer,outInoutId,type));
                }
                else
                {
                    //直接进入下下一步
                    tube.Import(outInoutId, type);
                }
            }
            else
            {
                if (onStepBreak != null) onStepBreak.Invoke(activeItem);
            }

            return connectedInfo.Key != null;
        }

        private void OnOneStep(IContainer item)
        {
            isReact = false;
            if (reactTuple.Count == 0)
            {
                if (onComplete != null) onComplete.Invoke();
            }
            else
            {
                if (onStepComplete != null) onStepComplete.Invoke();
            }
        }

    }
}