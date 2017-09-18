using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Connector
{
    [ExecuteInEditMode]
    public class PortItemBehaiver : MonoBehaviour, IPortItem
    {
        #region Propertys
        public IPortParent Body { get; set; }
        public IPortItem ConnectedNode { get; set; }
        public GameObject Render {
            get
            {
                return gameObject;
            }
        }
        public Vector3 Pos
        {
            get
            {
                return transform.position;
            }
        }
        public int NodeID { get { return _nodeId; } }
        public List<ConnectAble> connectAble
        {
            get
            {
                return _connectAble;
            }
        }
        #endregion
        public int _nodeId;
        public List<ConnectAble> _connectAble;

        void Awake()
        {
            gameObject.layer = LayerConst.nodeLayer;
        }

        public bool Attach(IPortItem item)
        {
            item.ConnectedNode = this;
            ConnectedNode = item;
            return true;
        }

        public void ResetTransform()
        {
            if (ConnectedNode != null)
            {
                ConnectAble connect = connectAble.Find(x => { return x.itemName == ConnectedNode.Body.Name && x.nodeId == ConnectedNode.NodeID; });
                if (connect != null)
                {
                    Body.ResetBodyTransform(ConnectedNode.Body, connect.relativePos, connect.relativeDir);
                }
            }
        }


        public IPortItem Detach()
        {
            IPortItem outItem = ConnectedNode;
            if (ConnectedNode != null)
            {
                ConnectedNode.ConnectedNode = null;
                ConnectedNode = null;
            }
            return outItem;
        }
    }

}