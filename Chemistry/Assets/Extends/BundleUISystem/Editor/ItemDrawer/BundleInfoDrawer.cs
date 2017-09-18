using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem.Internal;
using BundleUISystem;

[CustomPropertyDrawer(typeof(BundleInfo))]
public class BundleInfoDrawer : PropertyDrawer
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
        var assetName = property.FindPropertyRelative("assetName");
        var bundleName = property.FindPropertyRelative("bundleName");
        var typeProp = property.FindPropertyRelative("type"); ;
        var parentLayerProp = property.FindPropertyRelative("parentLayer");
        var resetProp = property.FindPropertyRelative("reset");
        var buttonProp = property.FindPropertyRelative("button");
        var toggleProp = property.FindPropertyRelative("toggle");
        float height = EditorGUIUtility.singleLineHeight;

        Rect rect = new Rect(position.xMin, position.yMin, position.width * 0.9f, height);
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
        }
        switch ((ItemInfoBase.Type)typeProp.enumValueIndex)
        {
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
            default:
                break;
        }
        rect = new Rect(position.max.x - position.width * 0.1f, position.yMin, position.width * 0.1f, height);

        if (GUI.Button(rect, "open",EditorStyles.textField))
        {
            var select = EditorUtility.DisplayDialog("提示", "是否打开新场景并加载资源", "是", "否");
            if (!select)
            {
                return;
            }
            BundlePreview.Data data = new BundlePreview.Data();
            var assetUrl = property.serializedObject.FindProperty("assetUrl");
            var menu = property.serializedObject.FindProperty("menu");
            if (string.IsNullOrEmpty(assetUrl.stringValue) || string.IsNullOrEmpty(menu.stringValue))
            {
                return;
            }
            data.assetUrl = assetUrl.stringValue;
            data.menu = menu.stringValue;
            var bdinfo = new BundleInfo();
            bdinfo.assetName = assetName.stringValue;
            bdinfo.bundleName = bundleName.stringValue;
            bdinfo.reset = resetProp.boolValue;
            data.rbundles.Add(bdinfo);
            var path = AssetDatabase.GUIDToAssetPath("018159907ea26db409399b839477ad27");
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
            GameObject holder = new GameObject("holder");
            BundlePreview preview = holder.AddComponent<BundlePreview>();
            preview.data = data;
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

       
        if (!property.isExpanded)
        {
            return;
        }

        rect = new Rect(position.xMin, position.yMin + height, position.width, height);
        EditorGUI.PropertyField(rect, assetName, new GUIContent("name"));

        rect.y += height;
        EditorGUI.PropertyField(rect, bundleName, new GUIContent("bundle"));

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
        EditorGUI.PropertyField(rect, resetProp, new GUIContent("reset"));
    }
    void Worning(Rect rect, string info)
    {
        GUI.color = Color.red;
        EditorGUI.SelectableLabel(rect, info);
        GUI.color = Color.white;
    }
}
