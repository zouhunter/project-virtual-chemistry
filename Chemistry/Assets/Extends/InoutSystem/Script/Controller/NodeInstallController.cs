using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace FlowSystem
{

    public class NodeConnectController : INodeConnectController
    {
        private INodeParent pickedUpItem;
        private INodeItem activeNode;
        private INodeItem targetNode;

        private TimeSpanInfo timeSpan;

        private float sphereRange = 0.0001f;

        public INodeParent SelectedItem
        {
            get
            {
                return pickedUpItem;
            }
        }
        public INodeItem ActiveNode
        {
            get
            {
                return activeNode;
            }
        }
        public INodeItem TargetNode
        {
            get
            {
                return targetNode;
            }
        }

        private Dictionary<INodeParent, List<INodeItem>> connectedNodes = new Dictionary<INodeParent, List<INodeItem>>();
        public NodeConnectController(float sphereRange, float spanTime)
        {
            timeSpan = new TimeSpanInfo(spanTime);
            this.sphereRange = sphereRange;
        }

        public bool WaitForPickUp()
        {
            if (pickedUpItem != null)
            {
                if (timeSpan.OnSpanComplete())
                {
                    return true;
                }
            }
            return false;
        }

        public List<INodeItem> PickUpInOutItem(INodeParent item)
        {
            this.pickedUpItem = item;
            List<INodeItem> olditems = new List<INodeItem>();
            if (connectedNodes.ContainsKey(item))
            {
                List<INodeItem> needClear = new List<INodeItem>();
                for (int i = 0; i < connectedNodes[item].Count; i++)
                {
                    INodeItem nodeItem = connectedNodes[item][i];
                    needClear.Add(nodeItem);

                    INodeItem target = nodeItem.ConnectedNode;
                    connectedNodes[target.Body].Remove(target);
                    Debug.Log(connectedNodes[item][i].Detach());

                    olditems.Add(nodeItem);
                    olditems.Add(target);
                }

                for (int i = 0; i < needClear.Count; i++)
                {
                    connectedNodes[item].Remove(needClear[i]);
                }
            }
            return olditems;
        }

        public void PutDownInOutItem(bool connected)
        {
            if (connected)
            {
                if (!connectedNodes.ContainsKey(pickedUpItem))
                {
                    connectedNodes[pickedUpItem] = new List<INodeItem>();
                }

                connectedNodes[pickedUpItem].Add(activeNode);

                if (!connectedNodes.ContainsKey(TargetNode.Body))
                {
                    connectedNodes[TargetNode.Body] = new List<INodeItem>();
                }

                connectedNodes[TargetNode.Body].Add(targetNode);
            }

            pickedUpItem = null;
            targetNode = null;
            activeNode = null;
        }

        public bool TryConnectItem()
        {
            if (ActiveNode != null && TargetNode != null)
            {
                if (targetNode.Attach(activeNode))
                {
                    activeNode.ResetTransform();
                    return true;
                }
            }
            return false;
        }

        public bool FindConnectableObject()
        {
            if (pickedUpItem != null)
            {
                INodeItem tempNode;
                foreach (var item in pickedUpItem.ChildNodes)
                {
                    if (FindInstallableNode(item, out tempNode))
                    {
                        activeNode = item;
                        targetNode = tempNode;
                        return true;
                    }
                }
            }
            activeNode = null;
            targetNode = null;
            return false;
        }

        private bool FindInstallableNode(INodeItem item, out INodeItem node)
        {
            Collider[] colliders = Physics.OverlapSphere(item.Pos, sphereRange, LayerMask.GetMask(LayerConst.nodeLayer));
            if (colliders != null && colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    NodeItemBehaiver tempNode = collider.GetComponent<NodeItemBehaiver>();
                    //主被动动连接点，非自身点，相同名，没有建立连接
                    if (tempNode.Body != item.Body && tempNode.Info.nodeName == item.Info.nodeName && tempNode.ConnectedNode == null)
                    {
                        if (tempNode.connectAble.Find((x) => x.itemName == item.Body.Name && x.nodeId == item.Info.nodeID) != null)
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
    }
}