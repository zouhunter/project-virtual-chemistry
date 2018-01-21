using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    [System.Serializable]
    public class LinkInfo

    {
        public string itemName;
        public int nodeId;
        public Vector3 relativePos;
        public Vector3 relativeDir;
    }
}