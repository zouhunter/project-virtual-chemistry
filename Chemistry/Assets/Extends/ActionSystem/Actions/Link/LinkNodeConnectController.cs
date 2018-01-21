using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    public class LinkConnectController
    {
        public UnityAction<LinkPort[]> onConnected { get; set; }
        public UnityAction<LinkPort> onMatch { get; set; }
        public UnityAction<LinkPort> onDisMatch { get; set; }
        public UnityAction<LinkPort[]> onDisconnected { get; set; }
        private Dictionary<LinkItem, List<LinkPort>> ConnectedDic { get; set; }
        private Transform Parent { get; set; }
        private float timer;
        private const float spanTime = 0.5f;
        private LinkItem pickedUpItem;
        private LinkPort activeNode;
        private LinkPort targetNode;

        public void Update()
        {
            timer += Time.deltaTime;
            if (pickedUpItem != null && timer > spanTime)
            {
                timer = 0f;

                if (FindConnectableObject())
                {
                    if (onMatch != null)
                    {
                        onMatch(activeNode);
                        onMatch(targetNode);
                    }
                }
                else
                {
                    if (targetNode != null)
                    {
                        onDisMatch.Invoke(targetNode);
                    }
                    if (activeNode != null)
                    {
                        onDisMatch.Invoke(activeNode);
                    }
                    activeNode = null;
                    targetNode = null;
                }
            }
        }

        public bool FindConnectableObject()
        {
            if (pickedUpItem != null)
            {
                LinkPort tempNode;
                foreach (var item in pickedUpItem.ChildNodes)
                {
                    if (LinkUtil.FindInstallableNode(item, out tempNode))
                    {
                        activeNode = item;
                        targetNode = tempNode;
                        return true;
                    }
                }
            }

            return false;
        }
     

        public void SetActiveItem(LinkItem item)
        {
            this.pickedUpItem = item;

            List<LinkPort> disconnected = new List<LinkPort>();

            if (ConnectedDic.ContainsKey(item))
            {
                LinkPort[] connectedPort = ConnectedDic[item].ToArray();
                for (int i = 0; i < connectedPort.Length; i++)
                {
                    LinkPort port = ConnectedDic[item][i];
                    LinkPort otherPort = port.ConnectedNode;

                    if (otherPort.Body.transform.IsChildOf(item.transform))
                    {
                        continue;//子对象不用清除
                    }
                    else
                    {
                        ConnectedDic[item].Remove(port);
                        ConnectedDic[otherPort.Body].Remove(otherPort);
                        LinkUtil.DetachNodes(port, otherPort, Parent);
                        disconnected.Add(port);
                        disconnected.Add(otherPort);
                    }
                }

                if (onDisconnected != null)
                    onDisconnected.Invoke(disconnected.ToArray());
            }
        }

        public void SetState(Transform parent, Dictionary<LinkItem, List<LinkPort>> ConnectedDic)
        {
            this.ConnectedDic = ConnectedDic;
            this.Parent = parent;
        }

        public void SetDisableItem(LinkItem item)
        {
            pickedUpItem = null;
            targetNode = null;
            activeNode = null;
        }

        public void TryConnect()
        {
            if (activeNode != null && targetNode != null && !targetNode.Body.transform.IsChildOf(activeNode.Body.transform))
            {
                LinkUtil.RecordToDic(ConnectedDic,activeNode);

                LinkUtil.RecordToDic(ConnectedDic, targetNode);

                LinkUtil.AttachNodes(activeNode, targetNode);

                if (onConnected != null)
                    onConnected.Invoke(new LinkPort[] { activeNode, targetNode });

                activeNode = null;
                targetNode = null;
            }
        }

       

    }
}