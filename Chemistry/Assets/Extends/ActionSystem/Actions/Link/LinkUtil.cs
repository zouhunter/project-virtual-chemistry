using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{
    public static class LinkUtil
    {

        public static void AttachNodes(LinkPort moveAblePort, LinkPort staticPort)
        {
            moveAblePort.ConnectedNode = staticPort;
            staticPort.ConnectedNode = moveAblePort;
            moveAblePort.ResetTransform();
            moveAblePort.Body.transform.SetParent(staticPort.Body.transform);
        }

        public static void DetachNodes(LinkPort moveAblePort, LinkPort staticPort, Transform parent)
        {
            moveAblePort.ConnectedNode = null;
            staticPort.ConnectedNode = null;
            moveAblePort.Body.transform.SetParent(parent);
        }
        public static void RecordToDic(Dictionary<LinkItem, List<LinkPort>> ConnectedDic, LinkPort port)
        {
            var item = port.Body;

            if (!ConnectedDic.ContainsKey(item))
            {
                ConnectedDic[item] = new List<LinkPort>();
            }

            ConnectedDic[item].Add(port);
        }
        public static void DetachConnectedPorts(Dictionary<LinkItem, List<LinkPort>> dic, Transform parent)
        {
            foreach (var item in dic)
            {
                var linkItem = item.Key;
                var ports = item.Value;
                linkItem.transform.SetParent(parent);
                foreach (var port in ports)
                {
                    port.ConnectedNode = null;
                }
            }
        }

        public static void ClampRotation(Transform target)
        {
            Vector3 newRot = target.eulerAngles;
            System.Func<float, float> clamp = (value) =>
            {
                float newValue = value % 360;
                if (newValue < 45)
                {
                    newValue = 0;
                }
                else
                {
                    if (newValue < 135)
                    {
                        newValue = 90;
                    }
                    else
                    {
                        if (newValue < 225)
                        {
                            newValue = 180;
                        }
                        else
                        {
                            if (newValue < 315)
                            {
                                newValue = -90;
                            }
                        }
                    }
                }
                return newValue;
            };

            newRot.x = clamp(newRot.x);
            newRot.y = clamp(newRot.y);
            newRot.z = clamp(newRot.z);

            target.eulerAngles = newRot;
        }
        /// <summary>
        /// 空间查找触发的点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static bool FindTriggerNodes(LinkPort item, out List<LinkPort> nodes)
        {
            nodes = null;
            Collider[] colliders = Physics.OverlapSphere(item.Pos, item.Range, LayerMask.GetMask( Layers.linknodeLayer));
            if (colliders != null && colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    LinkPort tempNode = collider.GetComponentInParent<LinkPort>();
                    if (tempNode == null || tempNode == item || tempNode.Body == item.Body)
                    {
                        continue;
                    }
                    else
                    {
                        if (nodes == null)
                            nodes = new List<WorldActionSystem.LinkPort>();
                        nodes.Add(tempNode);
                    }

                }
            }
            return nodes != null && nodes.Count > 0;
        }

        /// <summary>
        /// 空间查找可以可以连接的点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool FindInstallableNode(LinkPort item, out LinkPort node)
        {
            if (item.ConnectedNode != null)
            {
                node = item.ConnectedNode;
                return false;
            }

            Collider[] colliders = Physics.OverlapSphere(item.Pos, item.Range, LayerMask.GetMask(Layers.linknodeLayer));
            if (colliders != null && colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    LinkPort tempNode = collider.GetComponentInParent<LinkPort>();
                    if (tempNode == null || tempNode == item || tempNode.Body == item.Body || tempNode.ConnectedNode != null)
                    {
                        Debug.Log("ignore:" + tempNode);
                        continue;
                    }
                    //主被动动连接点，非自身点，相同名，没有建立连接
                    if (tempNode.connectAble.Find((x) => x.itemName == item.Body.Name && x.nodeId == item.NodeID) != null)
                    {
                        node = tempNode;
                        return true;
                    }
                }
            }
            node = null;
            return false;
        }
    }
}