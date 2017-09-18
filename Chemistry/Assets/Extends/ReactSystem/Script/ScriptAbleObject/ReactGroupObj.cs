using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace ReactSystem
{
    [CreateAssetMenu(menuName = "创建/ReactGroupObj")]
    public class ReactGroupObj : ScriptableObject
    {
        public string expName;
        public List<RunTimeElemet> elements = new List<RunTimeElemet>();
    }
}