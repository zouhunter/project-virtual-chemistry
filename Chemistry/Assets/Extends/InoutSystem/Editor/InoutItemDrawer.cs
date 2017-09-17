using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEditor;
using FlowSystem;

[CustomEditor(typeof(InOutItemBehaiver))]
public class AutoRactDrawer : Editor{

    InOutItemBehaiver obj;
    SerializedProperty interact;
    SerializedProperty autoract;

    void OnEnable()
    {
        autoract = serializedObject.FindProperty("autoRact");
        interact = serializedObject.FindProperty("interact");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (AutoReact())
        {
            EditorGUILayout.PropertyField(autoract, true);
        }
        else
        {

            EditorGUILayout.PropertyField(interact, true);
        }
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    bool AutoReact()
    {
        obj = (InOutItemBehaiver)target;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemInfo"), true);
        return obj.itemInfo.autoRact;
    }
}
