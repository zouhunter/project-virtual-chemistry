using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem.Binding
{
    public class TransformSyncRegister : ActionObjEventRegister
    {
        public Transform body;
        private bool active;
        private const string SyncStartKey = "TransformSyncStart";
        private const string SyncStopKey = "TransformSyncStop";
        private Transform target;
        private Vector3 lastPos;
        private Quaternion lastRot;

        private void Awake()
        {
            eventCtrl.AddDelegate<TransformSyncBody>(SyncStartKey, StartRotate);
            eventCtrl.AddDelegate<string>(SyncStopKey, StopRotate);
        }
        private void Update()
        {
            if (active && body && target)
            {
                if (lastPos != target.position || lastRot != target.rotation)
                {
                    lastPos = target.position;
                    lastRot = target.rotation;
#if UNITY_5_6_OR_NEWER
                body.transform.SetPositionAndRotation(target.position, target.rotation);
#else
                    body.transform.position = target.position;
                    body.transform.rotation = target.rotation;
#endif
                    body.transform.localScale = target.localScale;
                }

            }
        }

        private void StopRotate(string arg0)
        {
            if (key == arg0)
            {
                active = false;
            }
        }

        private void StartRotate(TransformSyncBody arg0)
        {
            if (arg0.key == key)
            {
                active = true;
                target = arg0.target;
            }
        }
    }
}