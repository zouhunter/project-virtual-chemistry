using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldActionSystem
{
    [AddComponentMenu(MenuName.LinkObj)]
    public class LinkObj : ActionObj
    {
        public LinkItem[] LinkItems { get { return linkItems; } }
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Link;
            }
        }

        [SerializeField]
        private LinkItem[] linkItems;
        [SerializeField]
        private List<LinkGroup> defultLink;

        private Vector3[] startPositions;
        private Quaternion[] startRotation;
        private Coroutine coroutine;
        public Dictionary<LinkItem, List<LinkPort>> ConnectedDic { get { return _connectedDic; } }
        private Dictionary<LinkItem, List<LinkPort>> _connectedDic = new Dictionary<LinkItem, List<LinkPort>>();
        protected override void Start()
        {
            base.Start();
            InitLinkItems();
        }

        void InitLinkItems()
        {
            startPositions = new Vector3[linkItems.Length];
            startRotation = new Quaternion[linkItems.Length];
            for (int i = 0; i < startPositions.Length; i++)
            {
                startPositions[i] = linkItems[i].transform.localPosition;
                startRotation[i] = linkItems[i].transform.localRotation;
            }
        }
        public void TryActiveLinkItem()
        {
            var notLinked = linkItems.Where(x => !ConnectedDic.ContainsKey(x)).FirstOrDefault();
            if (notLinked != null)
            {
                angleCtrl.UnNotice(anglePos);
                anglePos = notLinked.transform;
            }
            else
            {
                TryComplete();
            }
        }

        public void TryActiveLinkPort(LinkItem pickedUp)
        {
            for (int i = 0; i < pickedUp.ChildNodes.Count; i++)
            {
                var node = pickedUp.ChildNodes[i];
                if (node.ConnectedNode == null && node.connectAble.Count > 0)
                {
                    for (int j = 0; j < node.connectAble.Count; j++)
                    {
                        var info = node.connectAble[j];
                        var otheritem = (from x in linkItems
                                         where(x.Name == info.itemName && x != pickedUp && !x.transform.IsChildOf(pickedUp.transform))
                                         select x).FirstOrDefault();

                        if (otheritem != null)
                        {
                            var otherNode = otheritem.ChildNodes[info.nodeId];
                            if (otherNode != null && otherNode.ConnectedNode == null)
                            {
                                angleCtrl.UnNotice(anglePos);
                                anglePos = otherNode.transform;
                            }
                        }
                    }
                }
            }

        }

        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            if (auto)
            {
                if (coroutine == null)
                {
                    coroutine = StartCoroutine(AutoLinkItems());
                }
            }
            else
            {
                foreach (var item in linkItems)
                {
                    item.PickUpAble = true;
                }
                TryActiveLinkItem();
            }
        }


        protected override void OnBeforeEnd(bool force)
        {
            base.OnBeforeEnd(force);
            if (coroutine != null) StopCoroutine(coroutine);
            foreach (var item in linkItems)
            {
                item.PickUpAble = false;
            }
        }

        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            LinkUtil.DetachConnectedPorts(ConnectedDic, transform);
            ConnectedDic.Clear();
            ResetPositions();
        }
        public void TryComplete()
        {
            if (LinkItems.Length == ConnectedDic.Count)
            {
                OnEndExecute(false);
            }
        }
        private void ResetPositions()
        {
            for (int i = 0; i < startPositions.Length; i++)
            {
                linkItems[i].transform.localPosition = startPositions[i];
                linkItems[i].transform.localRotation = startRotation[i];
            }
        }

        private IEnumerator AutoLinkItems()
        {
            for (int i = 0; i < defultLink.Count; i++)
            {
                var linkGroup = defultLink[i];

                var portA = linkItems[linkGroup.ItemA].ChildNodes[linkGroup.portA];
                var portB = linkItems[linkGroup.ItemB].ChildNodes[linkGroup.portB];

                angleCtrl.UnNotice(anglePos);
                anglePos = portA.transform;

                yield return MoveBToA(portA, portB);
                LinkUtil.AttachNodes(portB, portA);
                LinkUtil.RecordToDic(ConnectedDic, portA);
                LinkUtil.RecordToDic(ConnectedDic, portB);
            }

            TryComplete();
        }

        private IEnumerator MoveBToA(LinkPort portA, LinkPort portB)
        {
            //var linkInfoA = portA.connectAble.Find(x => x.itemName == portB.Body.Name);
            var linkInfoB = portB.connectAble.Find(x => x.itemName == portA.Body.name);

            var pos = portA.Body.Trans.TransformPoint(linkInfoB.relativePos);
            var forward = portA.Body.Trans.TransformDirection(linkInfoB.relativeDir);
            var startPos = portB.Body.transform.localPosition;
            var startforward = portB.Body.transform.forward;

            for (float j = 0; j < 1f; j += Time.deltaTime)
            {
                portB.Body.transform.localPosition = Vector3.Lerp(startPos, pos, j);
                portB.Body.transform.forward = Vector3.Lerp(startforward, forward, j);
                yield return null;
            }

        }
    }
}