using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using FlowSystem;

[CustomPropertyDrawer(typeof(ConfigGroup))]
public class GroupDrawer : PropertyDrawer {
    float widthBt = 30;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }
    void addArrayTools(Rect position, SerializedProperty property)
    {
        string path = property.propertyPath;

        int arrayInd = path.LastIndexOf(".Array");
        bool bIsArray = arrayInd >= 0;

        if (bIsArray)
        {
            SerializedObject so = property.serializedObject;
            string arrayPath = path.Substring(0, arrayInd);
            SerializedProperty arrayProp = so.FindProperty(arrayPath);

            //Next we need to grab the index from the path string
            int indStart = path.IndexOf("[") + 1;
            int indEnd = path.IndexOf("]");

            string indString = path.Substring(indStart, indEnd - indStart);

            int myIndex = int.Parse(indString);
            Rect rcButton = position;
            rcButton.height = EditorGUIUtility.singleLineHeight;
            rcButton.x = position.xMax - widthBt * 5;
            rcButton.width = widthBt;

            bool lastEnabled = GUI.enabled;

            if (GUI.Button(rcButton, "o"))
            {
                var name = property.FindPropertyRelative("name");
                var elements = property.FindPropertyRelative("elements");

                GameObject parent = new GameObject(name.stringValue);
                GameObject child = null;
                for (int i = 0; i < elements.arraySize; i++)
                {
                    var pos = elements.GetArrayElementAtIndex(i).FindPropertyRelative("position");
                    var rot = elements.GetArrayElementAtIndex(i).FindPropertyRelative("rotation");
                    var element = elements.GetArrayElementAtIndex(i).FindPropertyRelative("element");

                    child = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab((GameObject)element.objectReferenceValue);
                    child.transform.SetParent(parent.transform, false);
                    child.transform.position = pos.vector3Value;
                    child.transform.rotation = rot.quaternionValue;
                }
            }

            if (myIndex == 0)
                GUI.enabled = false;

            rcButton.x += widthBt;
            if (GUI.Button(rcButton, "^"))
            {
                arrayProp.MoveArrayElement(myIndex, myIndex - 1);
                so.ApplyModifiedProperties();

            }

            rcButton.x += widthBt;
            GUI.enabled = lastEnabled;
            if (myIndex >= arrayProp.arraySize - 1)
                GUI.enabled = false;

            if (GUI.Button(rcButton, "v"))
            {
                arrayProp.MoveArrayElement(myIndex, myIndex + 1);
                so.ApplyModifiedProperties();
            }

            GUI.enabled = lastEnabled;

            rcButton.x += widthBt;
            if (GUI.Button(rcButton, "-"))
            {
                arrayProp.DeleteArrayElementAtIndex(myIndex);
                so.ApplyModifiedProperties();
            }

            rcButton.x += widthBt;
            if (GUI.Button(rcButton, "+"))
            {
                arrayProp.InsertArrayElementAtIndex(myIndex);
                so.ApplyModifiedProperties();
            }
        }
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        addArrayTools(position, property);
        Rect rc = position;
        if (!property.isExpanded)
            rc.width -= widthBt * 5;

        EditorGUI.PropertyField(rc, property, label, true);
    }
}
