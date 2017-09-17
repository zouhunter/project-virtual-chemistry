using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using System;
public class AutoAddScript : ScriptableWizard
{
    public Transform parent;
    public string targetType;
    public string addType;
    public bool isClear;

    [MenuItem("Tools/遍历添加删除")]
    static void AutoAdd()
    {
        DisplayWizard<AutoAddScript>("添加脚本到指定类型对象", "关闭", "添加脚本");
    }
    void OnEnable()
    {
        parent = Selection.activeTransform;
    }
    void OnWizardCreate()
    {

    }
    void OnWizardOtherButton()
    {
        LoadChild(parent);
    }
    void LoadChild(Transform parent)
    {
        var type = parent.GetComponent(targetType);
        if (type != null)
        {
            if (isClear)
            {
                DestroyImmediate(parent.gameObject.GetComponentSecure(addType));
            }
            else
            {
                parent.gameObject.AddComponentSecure(addType);
            }
        }

        if (parent.childCount >= 0)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                LoadChild(child);
            }
        }
    }
}
