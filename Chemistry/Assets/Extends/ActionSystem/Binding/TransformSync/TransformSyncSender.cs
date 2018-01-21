using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem.Binding
{
 
    public class TransformSyncSender : ActionObjEventSender
    {
        public Transform target;
        private const string SyncStartKey = "TransformSyncStart";
        private const string SyncStopKey = "TransformSyncStop";
        private TransformSyncBody body;
        protected override void Awake()
        {
            base.Awake();
            body = new TransformSyncBody(key, target);
        }
        protected override void OnBeforeActive(bool forceAuto)
        {
            base.OnBeforeActive(forceAuto);
            eventCtrl.NotifyObserver(SyncStartKey, body);
        }
        protected override void OnBeforeUnDo()
        {
            base.OnBeforeUnDo();
            eventCtrl.NotifyObserver(SyncStopKey, key);
        }
        protected override void OnBeforeComplete(bool force)
        {
            base.OnBeforeComplete(force);
            eventCtrl.NotifyObserver(SyncStopKey, key);
        }
    }
}

