using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(RunTimeTrigger))]
public class RunTimeTriggerDrawer : PropertyDrawer
{
    const float widthBt = 35;
    float singleLineHight;
    const int count = 6;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        singleLineHight = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded)
        {
            return singleLineHight;
        }

        var typeProp = property.FindPropertyRelative("type");
        var buttonListProp = property.FindPropertyRelative("button");
        var toggleListProp = property.FindPropertyRelative("toggle");
        var messageListProp = property.FindPropertyRelative("message");
        switch (typeProp.enumValueIndex)
        {
            case 0:
                return count * singleLineHight + EditorGUI.GetPropertyHeight(buttonListProp);
            case 1:
                return count * singleLineHight + EditorGUI.GetPropertyHeight(toggleListProp);
            case 2:
            case 3:
                return count * singleLineHight + EditorGUI.GetPropertyHeight(messageListProp);
            default:
                return count * singleLineHight;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var prefabProp = property.FindPropertyRelative("prefab");
        var typeProp = property.FindPropertyRelative("type");
        var parentProp = property.FindPropertyRelative("parent");
        var isWorldPop = property.FindPropertyRelative("isWorld");
        var buttonProp = property.FindPropertyRelative("button");
        var toggleProp = property.FindPropertyRelative("toggle");
        var messageProp = property.FindPropertyRelative("message");

        singleLineHight = EditorGUIUtility.singleLineHeight;

        Rect rect = new Rect(position.xMin, position.yMin, position.width, singleLineHight);
        property.isExpanded = EditorGUI.ToggleLeft(rect,new GUIContent(prefabProp.objectReferenceValue.name), property.isExpanded);

        if (!property.isExpanded)
        {
            return;
        }

        rect = new Rect(position.xMin, position.yMin + singleLineHight, position.width, singleLineHight);
        EditorGUI.PropertyField(rect, prefabProp, new GUIContent("预制体"));

        rect = new Rect(position.xMin, position.yMin + 2 * singleLineHight, position.width, singleLineHight);
        EditorGUI.PropertyField(rect, typeProp, new GUIContent("类型"));


        rect = new Rect(position.xMin, position.yMin + 3 * singleLineHight, position.width, singleLineHight);
        EditorGUI.PropertyField(rect, parentProp, new GUIContent("父节点"));

        rect = new Rect(position.xMin, position.yMin + 4 * singleLineHight, position.width, singleLineHight);
        EditorGUI.PropertyField(rect, isWorldPop, new GUIContent("3维物体"));

        rect = new Rect(position.xMin, position.yMin + 5 * singleLineHight, position.width, singleLineHight);
        switch (typeProp.enumValueIndex)
        {
            case 0:
                EditorGUI.PropertyField(rect, buttonProp, new GUIContent("Buttons"));
                break;
            case 1:
                EditorGUI.PropertyField(rect, toggleProp, new GUIContent("Toggles"));
                break;
            case 2:
            case 3:
                EditorGUI.PropertyField(rect, messageProp, new GUIContent("Keys"));
                break;
            default:
                break;
        }
        rect = new Rect(position.xMin, position.yMin + 6 * singleLineHight, position.width, singleLineHight);
        addArrayTools(rect, property,()=> {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefabProp.objectReferenceValue);
            go.transform.SetParent((Transform)parentProp.objectReferenceValue, isWorldPop.boolValue);
        });
    }

    void addArrayTools(Rect position, SerializedProperty property,UnityEngine.Events.UnityAction onCreate)
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
            rcButton.x = position.xMax - widthBt * 3;
            rcButton.width = widthBt;

            if (GUI.Button(rcButton, "o"))
            {
                onCreate.Invoke();
            }

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

}
