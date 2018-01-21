using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    public class LinkCtrl : OperateController
    {
        private LinkObj linkObj { get; set; }
        private IHighLightItems highter;

        private LinkConnectController linkConnectCtrl;
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Link;
            }
        }
        private PickUpController pickCtrl { get { return ActionSystem.Instence.pickupCtrl; } }

        public LinkCtrl()
        {
            highter = new ShaderHighLight();
            linkConnectCtrl = new LinkConnectController();
            linkConnectCtrl.onDisMatch = OnDisMath;
            linkConnectCtrl.onMatch = OnMatch;
            linkConnectCtrl.onConnected = OnConnected;
            pickCtrl.onPickup += (OnPickUp);
            pickCtrl.onPickdown += (OnPickDown);
            pickCtrl.onPickStay += (OnPickStay);
        }
        public override void Update()
        {
            if (linkConnectCtrl != null)
                linkConnectCtrl.Update();
        }

        void OnMatch(LinkPort item)
        {
            highter.HighLightTarget(item.gameObject, Color.green);
        }
        void OnDisMath(LinkPort item)
        {
            highter.UnHighLightTarget(item.gameObject);
        }
        void OnPickUp(PickUpAbleItem obj)
        {
            if (obj is LinkItem)
            {
                var linkItem = obj as LinkItem;
                if (linkItem)
                {
                    linkObj = linkItem.GetComponentInParent<LinkObj>();
                    linkObj.TryActiveLinkPort(linkItem);
                    if (linkObj)
                    {
                        linkConnectCtrl.SetState(linkObj.transform, linkObj.ConnectedDic);
                        linkConnectCtrl.SetActiveItem(linkItem);
                    }
                }
            }
        }

        void OnPickDown(PickUpAbleItem obj)
        {
            if (obj is LinkItem)
            {
                linkConnectCtrl.SetDisableItem(obj as LinkItem);
                linkObj.TryActiveLinkItem ();
            }
        }
        void OnPickStay(PickUpAbleItem go)
        {
            if(go is LinkItem)
            {
                linkConnectCtrl.TryConnect();
                pickCtrl.PickDown();
                Debug.Log("OnPickStatu");
            }
        }

        void OnConnected(LinkPort[] nodes)
        {
            foreach (var item in nodes)
            {
                var childNodes = item.Body.ChildNodes;
                foreach (var node in childNodes)
                {
                    highter.UnHighLightTarget(node.gameObject);
                }
            }
        }
    }
}