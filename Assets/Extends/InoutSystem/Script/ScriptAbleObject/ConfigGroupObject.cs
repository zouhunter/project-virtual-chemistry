using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
namespace FlowSystem
{
    [CreateAssetMenu(fileName = "ConfigGroupObject.asset",menuName ="生成/组记录文件")]
    public class ConfigGroupObject : ScriptableObject
    {
        public List<ConfigGroup> groupList = new List<ConfigGroup>();
    }
    [System.Serializable]
    public class ConfigGroup
    {
        public string name;
        public List<RunTimeElemet> elements;
        public ConfigGroup(string name)
        {
            this.name = name;
            elements = new List<FlowSystem.RunTimeElemet>();
        }
    }
}