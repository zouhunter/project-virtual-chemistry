using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;
[System.Serializable]
public class PerfabInfo:IComparable<PerfabInfo> {
    public int index;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public string elementName;
    public GameObject LoadGameObject(Transform parent)
    {
       GameObject perfab = Resources.Load<GameObject>(AppConfig.PathConfig.elementperfabPath + elementName);
       GameObject item = UnityEngine.Object.Instantiate(perfab);
        item.transform.SetParent(parent);
        item.transform.localPosition = position;
        item.transform.localRotation = rotation;
        item.transform.localScale = scale;
        item.name = elementName;
        return item;
    }

    public int CompareTo(PerfabInfo obj)
    {
        if (obj.index > index)
        {
            return -1;
        }
        else if (obj.index < index)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
