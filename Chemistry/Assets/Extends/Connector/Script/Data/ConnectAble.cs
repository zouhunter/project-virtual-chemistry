using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace Connector
{
    [System.Serializable]
    public class ConnectAble
    {
        public string itemName;
        public int nodeId;
        public Vector3 relativePos;
        public Vector3 relativeDir;
    }
}