using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
public class SettingAsset {
   
    [MenuItem("Tools/CreateAsset")]
    static void CreateWindow()
    {
        EditorWindow.GetWindow<CreateAssetWindow>("资源创建面版");
    }
}
public class CreateAssetWindow :EditorWindow
{
    public string scriptObject;
    void OnEnable()
    {
        scriptObject = Selection.activeObject.name;
    }
    void OnGUI ()
    {
        EditorGUILayout.LabelField("脚本名");
        scriptObject =EditorGUILayout.TextField(scriptObject);
        if (GUILayout.Button("创建"))
        {
            SettingAssetUtility.CreateAsset(scriptObject.ToString());
        }
    }
}