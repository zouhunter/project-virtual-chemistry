using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace FlowSystem
{
    [System.Serializable]
    public class RunTimeElemet
    {
        public string name;
        public int id;
        public Vector3 position;
        public Quaternion rotation;
        public GameObject element;
        public bool startActive;
    }
}