using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace Connector
{
    public class NodeConnectController : INodeConnectController
    {
        public event UnityAction<IPortItem[]> onConnected;
        public event UnityAction<IPortItem> onMatch;
        public event UnityAction<IPortItem> onDisMatch;
        public event UnityAction<IPortItem[]> onDisconnected;
        public Dictionary<IPortParent, List<IPortItem>> ConnectedDic { get { return connectedNodes; } }

        private float timeSpan;
        private float spanTime;
        private float sphereRange = 0.0001f;
        private IPortParent pickedUpItem;
        private IPortItem activeNode;
        private IPortItem targetNode;
        private Dictionary<IPortParent, List<IPortItem>> connectedNodes = new Dictionary<IPortParent, List<IPortItem>>();
        public NodeConnectController(float sphereRange, float spanTime)
        {
            this.spanTime = spanTime;
            this.sphereRange = sphereRange;
        }

        public void Update()
        {
            timeSpan += Time.deltaTime;
            if (pickedUpItem != null && timeSpan > spanTime)
            {
                timeSpan = 0f;
               
                if (!FindConnectableObject())
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
                IPortItem tempNode;
                foreach (var item in pickedUpItem.ChildNodes)
                {
                    if (FindInstallableNode(item, out tempNode))
                    {
                        activeNode = item;
                        targetNode = tempNode;
                        if (onMatch != null) {
                            onMatch(activeNode);
                            onMatch(targetNode);
                        }
                        return true;
                    }
                }
            }
         
            return false;
        }

        private bool FindInstallableNode(IPortItem item, out IPortItem node)
        {
            Collider[] colliders = Physics.OverlapSphere(item.Pos, sphereRange, 1 << LayerConst.nodeLayer);
            if (colliders != null && colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    IPortItem tempNode = collider.GetComponent<IPortItem>();
                    if (tempNode == null)
                    {
                        //Debug.Log(collider + " have no iportItem");
                        continue;
                    }
                    //主被动动连接点，非自身点，相同名，没有建立连接
                    if (tempNode.Body != item.Body && tempNode.ConnectedNode == null)
                    {
                        if (tempNode.connectAble.Find((x) => x.itemName == item.Body.Name && x.nodeId == item.NodeID) != null)
                        {
                            node = tempNode;
                            return true;
                        }
                    }
                }
            }
            node = null;
            return false;
        }

        public void SetActiveItem(IPortParent item)
        {
            this.pickedUpItem = item;
            List<IPortItem> olditems = new List<IPortItem>();
            if (connectedNodes.ContainsKey(item))
            {
                List<IPortItem> needClear = new List<IPortItem>();
                for (int i = 0; i < connectedNodes[item].Count; i++)
                {
                    IPortItem nodeItem = connectedNodes[item][i];
                    needClear.Add(nodeItem);

                    IPortItem target = nodeItem.ConnectedNode;
                    connectedNodes[target.Body].Remove(target);
                    Debug.Log(connectedNodes[item][i].Detach());

                    olditems.Add(nodeItem);
                    olditems.Add(target);
                }

                for (int i = 0; i < needClear.Count; i++)
                {
                    connectedNodes[item].Remove(needClear[i]);
                }
                if (onDisconnected != null) onDisconnected.Invoke(needClear.ToArray());
            }
        }

        public void SetDisableItem(IPortParent item)
        {
            pickedUpItem = null;
            targetNode = null;
            activeNode = null;
        }

        public void TryConnect()
        {
            if (activeNode != null && activeNode != null)
            {
                if (targetNode.Attach(activeNode))
                {
                    activeNode.ResetTransform();

                    if (!connectedNodes.ContainsKey(pickedUpItem))
                    {
                        connectedNodes[pickedUpItem] = new List<IPortItem>();
                    }

                    connectedNodes[pickedUpItem].Add(activeNode);

                    if (!connectedNodes.ContainsKey(targetNode.Body))
                    {
                        connectedNodes[targetNode.Body] = new List<IPortItem>();
                    }

                    connectedNodes[targetNode.Body].Add(targetNode);

                    if (onConnected != null) onConnected.Invoke(new IPortItem[] { activeNode, targetNode });
                    activeNode = null;
                    targetNode = null;
                }
            }
        }
    }
}