using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
namespace WorldActionSystem
{
    public class ActionGroupObj : ScriptableObject
    {
        public string groupKey;
        public int totalCommand;
        public List<ActionPrefabItem> prefabList = new List<ActionPrefabItem>();
    }
}
