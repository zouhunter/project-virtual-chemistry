using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ElementPoses : MonoBehaviour {

#if UNITY_EDITOR
    [InspectorButton("Record")]
    public int 记录;
    public void Record()
    {
        poses = new Vector3[transform.childCount];
        int index = 0;
        foreach (Transform item in transform)
        {
            poses[index++] = item.position;
        }
    }
#endif

    public Vector3[] poses;
    private List<int> usedIndex = new List<int>();
    void Start()
    {
        //放置位置
        //Laboratory.Current.display = GetEmptyPos;
    }
    /// <summary>
    /// 获取一个未使用的index
    /// </summary>
    /// <returns></returns>
    public Vector3 GetEmptyPos()
    {
        for (int i = 0; i < poses.Length; i++)
        {
            if (!usedIndex.Contains(i))
            {
                usedIndex.Add(i);
                return poses[i];
            }
        }
        return Vector3.zero;
    }
}

