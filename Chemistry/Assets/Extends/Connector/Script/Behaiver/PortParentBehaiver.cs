using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Connector
{
    public class PortParentBehaiver : MonoBehaviour, IPortParent
    {
        private List<IPortItem> _childNodes = new List<IPortItem>();
        public List<IPortItem> ChildNodes
        {
            get
            {
                return _childNodes;
            }
        }
        public string Name {
            get
            {
                return name;
            }
        }

        public Transform Trans
        {
            get
            {
                return transform;
            }
        }

        public void ResetBodyTransform(IPortParent otherParent, Vector3 rPos, Vector3 rdDir)
        {
            transform.position = otherParent.Trans.TransformPoint(rPos);
            transform.forward = otherParent.Trans.TransformDirection(rdDir);
        }

        private void Awake()
        {
            var nodeItems = GetComponentsInChildren<IPortItem>(true);
            _childNodes.AddRange(nodeItems);
            gameObject.layer = LayerConst.elementLayer;

            foreach (var item in nodeItems)
            {
                item.Body = this;
            }
        }
    }

}