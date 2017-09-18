using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem.Internal;
using BundleUISystem;

[CustomPropertyDrawer(typeof(UIBundleInfo))]
public class UIBundleInfoDrawer : PropertyDrawer
{
    const float widthBt = 20;
    const int ht = 6;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        var typeProp = property.FindPropertyRelative("type");
        var buttonListProp = property.FindPropertyRelative("button");
        var toggleListProp = property.FindPropertyRelative("toggle");
        switch ((ItemInfoBase.Type)typeProp.enumValueIndex)
        {
            case ItemInfoBase.Type.Button:
                return ht * EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(buttonListProp);
            case ItemInfoBase.Type.Toggle:
                return ht * EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(toggleListProp);
            default:
                return ht * EditorGUIUtility.singleLineHeight;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var instanceIDProp = property.FindPropertyRelative("instanceID");
        var goodProp = property.FindPropertyRelative("good");
        var guidProp = property.FindPropertyRelative("guid");
        var assetName = property.FindPropertyRelative("assetName");
        var bundleName = property.FindPropertyRelative("bundleName");
        var typeProp = property.FindPropertyRelative("type"); ;
        var parentLayerProp = property.FindPropertyRelative("parentLayer");
        var boolProp = property.FindPropertyRelative("reset");
        var buttonProp = property.FindPropertyRelative("button");
        var toggleProp = property.FindPropertyRelative("toggle");
        float height = EditorGUIUtility.singleLineHeight;

        Rect rect = new Rect(position.x, position.y, position.width * 0.9f, height);

        if (GUI.Button(rect, assetName.stringValue, EditorStyles.toolbar))
        {
            //使用对象是UIGroupObj，将无法从button和Toggle加载
            if (property.serializedObject.targetObject is UIGroupObj)
            {
                if (typeProp.enumValueIndex == (int)ItemInfoBase.Type.Button || typeProp.enumValueIndex == (int)ItemInfoBase.Type.Toggle)
                {
                    typeProp.enumValueIndex = (int)ItemInfoBase.Type.Name;
                }
            }

            property.isExpanded = !property.isExpanded;

            if (property.isExpanded)
            {
                if (instanceIDProp.intValue == 0)
                {
                    string[] paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName.stringValue, assetName.stringValue);
                    if (paths != null && paths.Length > 0)
                    {
                        GameObject gopfb = AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);
                        if (gopfb != null)
                        {
                            var path = AssetDatabase.GetAssetPath(gopfb);
                            guidProp.stringValue = AssetDatabase.AssetPathToGUID(path);
                            GameObject go = PrefabUtility.InstantiatePrefab(gopfb) as GameObject;
                            var obj = property.serializedObject.targetObject;

                            if (obj is UIGroup)
                            {
                                if (go.GetComponent<Transform>() is RectTransform)
                                {
                                    go.transform.SetParent((obj as UIGroup).transform, false);
                                }
                                else
                                {
                                    go.transform.SetParent((obj as UIGroup).transform, true);
                                }
                            }
                            else if (obj is UIGroupObj)
                            {
                                if (go.GetComponent<Transform>() is RectTransform)
                                {
                                    var canvas = GameObject.FindObjectOfType<Canvas>();
                                    go.transform.SetParent(canvas.transform, false);
                                }
                                else
                                {
                                    go.transform.SetParent(null);
                                }
                            }

                            if (boolProp.boolValue)
                            {
                                go.transform.position = Vector3.zero;
                                go.transform.localRotation = Quaternion.identity;
                            }
                            instanceIDProp.intValue = go.GetInstanceID();
                        }
                    }
                }
            }
            else
            {
                var obj = EditorUtility.InstanceIDToObject(instanceIDProp.intValue);
                if (obj != null){
                    Object.DestroyImmediate(obj);
                }
                instanceIDProp.intValue = 0;
            }
        }

        if (!goodProp.boolValue)
        {
            Worning(rect, assetName.stringValue + " Changed！!!");
        }
        else
        {
            switch ((ItemInfoBase.Type)typeProp.enumValueIndex)
            {
                case ItemInfoBase.Type.Name:
                    break;
                case ItemInfoBase.Type.Button:
                    if (buttonProp.objectReferenceValue == null)
                    {
                        Worning(rect, "button lost!");
                    }
                    break;
                case ItemInfoBase.Type.Toggle:
                    if (toggleProp.objectReferenceValue == null)
                    {
                        Worning(rect, "toggle lost!");
                    }
                    break;
                case ItemInfoBase.Type.Enable:
                    break;
                default:
                    break;
            }
        }


        rect = new Rect(position.max.x - position.width * 0.1f, position.yMin, position.width * 0.1f, height);

        switch (Event.current.type)
        {
            case EventType.DragUpdated:
                if (rect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
                break;
            case EventType.DragPerform:
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        var obj = DragAndDrop.objectReferences[0];
                        if (obj is GameObject)
                        {
                            var path = AssetDatabase.GetAssetPath(obj);
                            if (!string.IsNullOrEmpty(path))
                            {
                                guidProp.stringValue = AssetDatabase.AssetPathToGUID(path);
                                AssetImporter importer = AssetImporter.GetAtPath(path);
                                assetName.stringValue = obj.name;
                                bundleName.stringValue = importer.assetBundleName;
                            }
                        }
                    }
                    DragAndDrop.AcceptDrag();
                }
                break;
            case EventType.Repaint:
                var path0 = AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
                var obj0 = AssetDatabase.LoadAssetAtPath<GameObject>(path0);
                goodProp.boolValue = obj0 != null;
                break;
        }


        if (GUI.Button(rect, "[-]", EditorStyles.textField))
        {
            if (goodProp.boolValue)
            {
                var path = AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                EditorGUIUtility.PingObject(obj);
            }
        }

        if (!property.isExpanded)
        {
            return;
        }

        EditorGUI.BeginDisabledGroup(true);
        rect = new Rect(position.xMin, position.yMin + height, position.width, height);
        EditorGUI.PropertyField(rect, assetName, new GUIContent("name"));

        rect.y += height;
        EditorGUI.PropertyField(rect, bundleName, new GUIContent("bundle"));
        EditorGUI.EndDisabledGroup();

        rect.y += height;
        EditorGUI.PropertyField(rect, typeProp, new GUIContent("type"));
        switch ((ItemInfoBase.Type)typeProp.enumValueIndex)
        {
            case ItemInfoBase.Type.Name:
                break;
            case ItemInfoBase.Type.Button:
                rect.y += height;
                EditorGUI.PropertyField(rect, buttonProp, new GUIContent("Button"));
                break;
            case ItemInfoBase.Type.Toggle:
                rect.y += height;
                EditorGUI.PropertyField(rect, toggleProp, new GUIContent("Toggle"));
                break;
            case ItemInfoBase.Type.Enable:
                break;
            default:
                break;
        }

        rect.y += height;
        EditorGUI.PropertyField(rect, parentLayerProp, new GUIContent("parentLayer"));

        rect.y += height;
        EditorGUI.PropertyField(rect, boolProp, new GUIContent("reset"));
    }

    void Worning(Rect rect, string info)
    {
        GUI.color = Color.red;
        EditorGUI.SelectableLabel(rect, info);
        GUI.color = Color.white;
    }
}
