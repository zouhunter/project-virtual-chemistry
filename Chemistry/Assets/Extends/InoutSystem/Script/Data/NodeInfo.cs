using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace FlowSystem
{
    [System.Serializable]
    public class NodeInfo
    {
        public string nodeName;
        public int nodeID;
        public bool enterOnly;//只进不出
    }
}
