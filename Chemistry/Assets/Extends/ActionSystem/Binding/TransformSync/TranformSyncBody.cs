using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem.Binding
{
    public class TransformSyncBody
    {
        public string key;
        public Transform target;

        public TransformSyncBody(string key, Transform target)
        {
            this.key = key;
            this.target = target;
        }
    }
}