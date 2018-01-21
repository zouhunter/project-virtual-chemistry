using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    public class LinkItem : PickUpAbleItem
    {
        private List<LinkPort> _childNodes = new List<LinkPort>();
        public List<LinkPort> ChildNodes
        {
            get
            {
                InitPorts();
                return _childNodes;
            }
        }
        public Transform Trans
        {
            get
            {
                return transform;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            InitPorts();
            InitLayer();
        }
        private void InitPorts()
        {
            if(_childNodes == null || _childNodes.Count == 0)
            {
                var nodeItems = GetComponentsInChildren<LinkPort>(true);
                _childNodes.AddRange(nodeItems);
            }
        }

        private void InitLayer()
        {
            Collider.gameObject.layer =LayerMask.NameToLayer( Layers.pickUpElementLayer);
        }
        public void ResetBodyTransform(LinkItem otherParent, Vector3 rPos, Vector3 rdDir)
        {
            transform.position = otherParent.Trans.TransformPoint(rPos);
            transform.forward = otherParent.Trans.TransformDirection(rdDir);
        }

        public override void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }
    }

}