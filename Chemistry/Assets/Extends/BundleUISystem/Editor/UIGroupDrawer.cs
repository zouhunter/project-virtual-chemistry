using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Rotorz.ReorderableList;
using Rotorz.ReorderableList.Internal;
using BundleUISystem;
using System;
using UnityEngine.Serialization;

using AssetBundle = UnityEngine.AssetBundle;
[CustomEditor(typeof(UIGroup)), CanEditMultipleObjects]
public class UIGroupDrawer : UIDrawerTemp
{
    protected override void DrawRuntimeItems()
    {
        ReorderableListGUI.Title("共用资源列表");
        ReorderableListGUI.ListField(groupObjsProp);
        base.DrawRuntimeItems();
    }
}
[CustomEditor(typeof(UIGroupObj))]
public class UIGroupObjDrawer : UIDrawerTemp
{

}

public abstract class UIDrawerTemp : Editor
{
    protected SerializedProperty script;
    protected SerializedProperty groupObjsProp;
    protected SerializedProperty bundlesProp;
    protected SerializedProperty prefabsProp;
    protected SerializedProperty rbundlesProp;
    protected SerializedProperty assetUrlProp;
    protected SerializedProperty menuProp;
    protected SerializedProperty defultTypeProp;
    protected DragAdapt bundlesAdapt;
    protected DragAdapt prefabsAdapt;
    protected DragAdapt rbundlesAdapt;
    protected bool swink;
    private string query;
    private SerializedProperty prefabsPropWorp;
    private SerializedProperty bundlesPropWorp;
    private SerializedProperty rbundlesPropWorp;
    protected DragAdapt prefabsAdaptWorp;
    protected DragAdapt bundlesAdaptWorp;
    protected DragAdapt rbundlesAdaptWorp;

#if AssetBundleTools
    protected string[] option = new string[] { "预制", "本地", "路径" };
#else
    protected string[] option = new string[] { "预制"};
#endif
    private void OnEnable()
    {
        script = serializedObject.FindProperty("m_Script");
        bundlesProp = serializedObject.FindProperty("bundles");
        bundlesAdapt = new DragAdapt(bundlesProp, "bundles");
        prefabsProp = serializedObject.FindProperty("prefabs");
        prefabsAdapt = new DragAdapt(prefabsProp, "prefabs");
        rbundlesProp = serializedObject.FindProperty("rbundles");
        rbundlesAdapt = new DragAdapt(rbundlesProp, "rbundles");
        groupObjsProp = serializedObject.FindProperty("groupObjs");
        defultTypeProp = serializedObject.FindProperty("defultType");
        assetUrlProp = serializedObject.FindProperty("assetUrl");
        menuProp = serializedObject.FindProperty("menu");

        var sobj = new SerializedObject(UIGroupObj.CreateInstance<UIGroupObj>());
        prefabsPropWorp = sobj.FindProperty("prefabs");
        bundlesPropWorp = sobj.FindProperty("bundles");
        rbundlesPropWorp = sobj.FindProperty("rbundles");
        prefabsAdaptWorp = new DragAdapt(prefabsPropWorp, "prefabsWorp");
        bundlesAdaptWorp = new DragAdapt(bundlesPropWorp, "bundlesWorp");
        rbundlesAdaptWorp = new DragAdapt(rbundlesPropWorp, "rbundlesWorp");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawScript();
        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            DrawOption();
            DrawToolButtons();
        }
       
        DrawRuntimeItems();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawScript()
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(script);
        EditorGUI.EndDisabledGroup();
    }
    private void DrawOption()
    {
        EditorGUI.BeginChangeCheck();
        defultTypeProp.enumValueIndex = EditorGUILayout.Popup(defultTypeProp.enumValueIndex, option, EditorStyles.toolbarPopup);// GUILayout.Toolbar(defultTypeProp.enumValueIndex, option, EditorStyles.toolbarButton);
    }
    protected virtual void DrawRuntimeItems()
    {
        EditorGUI.BeginChangeCheck();
        query = EditorGUILayout.TextField(query);
        if (EditorGUI.EndChangeCheck())
        {
            MarchList();
        }
        if (string.IsNullOrEmpty(query))
        {
            switch ((UILoadType)defultTypeProp.enumValueIndex)
            {
                case UILoadType.LocalPrefab:
                    ReorderableListGUI.Title("预制体动态加载资源信息列表");
                    Rotorz.ReorderableList.ReorderableListGUI.ListField(prefabsAdapt);
                    break;
                case UILoadType.LocalBundle:
                    ReorderableListGUI.Title("本地动态加载资源信息列表");
                    Rotorz.ReorderableList.ReorderableListGUI.ListField(bundlesAdapt);
                    break;
                case UILoadType.RemoteBundle:
                    ReorderableListGUI.Title("远端动态加载资源信息列表");
                    Rotorz.ReorderableList.ReorderableListGUI.ListField(rbundlesAdapt);
                    break;
                default:
                    break;
            }
        }
       else
        {
            ReorderableListGUI.Title("[March]");
            switch ((UILoadType)defultTypeProp.enumValueIndex)
            {
                case UILoadType.LocalPrefab:
                    Rotorz.ReorderableList.ReorderableListGUI.ListField(prefabsAdaptWorp);
                    break;
                case UILoadType.LocalBundle:
                    Rotorz.ReorderableList.ReorderableListGUI.ListField(bundlesAdaptWorp);
                    break;
                case UILoadType.RemoteBundle:
                    Rotorz.ReorderableList.ReorderableListGUI.ListField(rbundlesAdaptWorp);
                    break;
                default:
                    break;
            }
        }

    }
    private void MarchList()
    {
        SerializedProperty property = null;
        SerializedProperty targetProperty = null;
        if (!string.IsNullOrEmpty(query))
        {
            switch ((UILoadType)defultTypeProp.enumValueIndex)
            {
                case UILoadType.LocalPrefab:
                    property = prefabsProp;
                    targetProperty = prefabsPropWorp;
                    break;
                case UILoadType.LocalBundle:
                    property = bundlesProp;
                    targetProperty = bundlesPropWorp;
                    break;
                case UILoadType.RemoteBundle:
                    property = rbundlesProp;
                    targetProperty = rbundlesPropWorp;
                    break;
                default:
                    break;
            }
            targetProperty.ClearArray();
            for (int i = 0; i < property.arraySize; i++)
            {
                var assetNameProp = property.GetArrayElementAtIndex(i).FindPropertyRelative("assetName");
                if (assetNameProp.stringValue.ToLower().Contains(query.ToLower()))
                {
                    targetProperty.InsertArrayElementAtIndex(0);
                    SerializedPropertyUtility.CopyPropertyValue(targetProperty.GetArrayElementAtIndex(0), property.GetArrayElementAtIndex(i));
                }
            }
        }
    }

    private void DrawToolButtons()
    {
        var btnStyle = EditorStyles.miniButton;
        var widthSytle = GUILayout.Width(20);
        switch ((UILoadType)defultTypeProp.enumValueIndex)
        {
            case UILoadType.LocalPrefab:
                using (var hor = new EditorGUILayout.HorizontalScope(widthSytle))
                {
                    if (GUILayout.Button(new GUIContent("%", "移除重复"), btnStyle))
                    {
                        RemoveBundlesDouble(prefabsProp);
                    }
                    if (GUILayout.Button(new GUIContent("！", "排序"), btnStyle))
                    {
                        SortAllBundles(prefabsProp);
                    }
                    if (GUILayout.Button(new GUIContent("o", "批量加载"), btnStyle))
                    {
                        GroupLoadPrefabs(prefabsProp);
                    }
                    if (GUILayout.Button(new GUIContent("c", "批量关闭"), btnStyle))
                    {
                        CloseAllCreated(prefabsProp);
                    }
                }
                break;
            case UILoadType.LocalBundle:
                using (var hor = new EditorGUILayout.HorizontalScope(widthSytle))
                {
                    if (GUILayout.Button(new GUIContent("%", "移除重复"), btnStyle))
                    {
                        RemoveBundlesDouble(bundlesProp);
                    }
                    if (GUILayout.Button(new GUIContent("*", "快速更新"), btnStyle))
                    {
                        QuickUpdateBundles();
                    }
                    if (GUILayout.Button(new GUIContent("!", "排序"), btnStyle))
                    {
                        SortAllBundles(bundlesProp);
                    }
                    if (GUILayout.Button(new GUIContent("o", "批量加载"), btnStyle))
                    {
                        GroupLoadPrefabs(bundlesProp);
                    }
                    if (GUILayout.Button(new GUIContent("c", "批量关闭"), btnStyle))
                    {
                        CloseAllCreated(bundlesProp);
                    }
                }
                break;
            case UILoadType.RemoteBundle:
                using (var hor = new EditorGUILayout.HorizontalScope(widthSytle))
                {
                    if (GUILayout.Button(new GUIContent("%", "移除重复"), btnStyle))
                    {
                        RemoveBundlesDouble(rbundlesProp);
                    }
                    if (GUILayout.Button(new GUIContent("!", "排序"), btnStyle))
                    {
                        SortAllBundles(rbundlesProp);
                    }
                    if (GUILayout.Button(new GUIContent("o", "批量加载"), btnStyle))
                    {
                        GroupPreviewFromBundles();
                    }
                }
                break;
            default:
                break;
        }

        switch ((UILoadType)defultTypeProp.enumValueIndex)
        {
            case UILoadType.RemoteBundle:
                EditorGUILayout.PropertyField(assetUrlProp);
                EditorGUILayout.PropertyField(menuProp);
                break;
            default:
                break;
        }

    }

    private void GroupPreviewFromBundles()
    {
        BundlePreview.Data data = new BundlePreview.Data();
        var assetUrl = rbundlesProp.serializedObject.FindProperty("assetUrl");
        var menu = rbundlesProp.serializedObject.FindProperty("menu");
        Debug.Log(assetUrl);
        data.assetUrl = assetUrl.stringValue;
        data.menu = menu.stringValue;
        for (int i = 0; i < rbundlesProp.arraySize; i++)
        {
            var itemProp = rbundlesProp.GetArrayElementAtIndex(i);
            var assetProp = itemProp.FindPropertyRelative("assetName");
            var bundleProp = itemProp.FindPropertyRelative("bundleName");
            var resetProp = itemProp.FindPropertyRelative("reset");

            var bdinfo = new BundleInfo();
            bdinfo.assetName = assetProp.stringValue;
            bdinfo.bundleName = bundleProp.stringValue;
            bdinfo.reset = resetProp.boolValue;
            data.rbundles.Add(bdinfo);
        }
        var path = AssetDatabase.GUIDToAssetPath("018159907ea26db409399b839477ad27");
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
        GameObject holder = new GameObject("holder");
        BundlePreview preview = holder.AddComponent<BundlePreview>();
        preview.data = data;
        EditorApplication.ExecuteMenuItem("Edit/Play");
    }

    private void GroupLoadPrefabs(SerializedProperty proprety)
    {
        for (int i = 0; i < proprety.arraySize; i++)
        {
            var itemProp = proprety.GetArrayElementAtIndex(i);
            GameObject prefab = null;
            var prefabProp = itemProp.FindPropertyRelative("prefab");
            var assetNameProp = itemProp.FindPropertyRelative("assetName");
            var instanceIDProp = itemProp.FindPropertyRelative("instanceID");

            if (instanceIDProp.intValue != 0) continue;

            if (prefabProp == null)
            {
                var bundleNameProp = itemProp.FindPropertyRelative("bundleName");
                var guidProp = itemProp.FindPropertyRelative("guid");
                var paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleNameProp.stringValue, assetNameProp.stringValue);
                if(paths.Length > 0)
                {
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);
                    guidProp.stringValue = AssetDatabase.AssetPathToGUID(paths[0]);
                }
            }
            else
            {
                prefab = prefabProp.objectReferenceValue as GameObject;
            }

            if (prefab == null)
            {
                UnityEditor.EditorUtility.DisplayDialog("空对象", "找不到预制体" + assetNameProp.stringValue, "确认");
            }
            else
            {
                var resetProp = itemProp.FindPropertyRelative("reset");
                GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (target is UIGroup)
                {
                    if (go.GetComponent<Transform>() is RectTransform)
                    {
                        go.transform.SetParent((target as UIGroup).transform, false);
                    }
                    else
                    {
                        go.transform.SetParent((target as UIGroup).transform, true);
                    }
                }
                else if (target is UIGroupObj)
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

                if (resetProp.boolValue)
                {
                    go.transform.position = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                }

                instanceIDProp.intValue = go.GetInstanceID();
            }

        }
    }

    private void QuickUpdateBundles()
    {
        for (int i = 0; i < bundlesProp.arraySize; i++)
        {
            var itemProp = bundlesProp.GetArrayElementAtIndex(i);
            var guidProp = itemProp.FindPropertyRelative("guid");
            var goodProp = itemProp.FindPropertyRelative("good");
            var assetNameProp = itemProp.FindPropertyRelative("assetName");
            var bundleNameProp = itemProp.FindPropertyRelative("bundleName");

            if (!goodProp.boolValue)
            {
                UnityEditor.EditorUtility.DisplayDialog("空对象", assetNameProp.stringValue + "信息错误", "确认");
                continue;
            }

            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            UnityEditor.AssetImporter importer = UnityEditor.AssetImporter.GetAtPath(assetPath);

            assetNameProp.stringValue = obj.name;
            bundleNameProp.stringValue = importer.assetBundleName;

            if (string.IsNullOrEmpty(bundleNameProp.stringValue))
            {
                UnityEditor.EditorUtility.DisplayDialog("提示", "预制体" + assetNameProp.stringValue + "没有assetBundle标记", "确认");
                return;
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);

    }
    private void RemoveBundlesDouble(SerializedProperty property)
    {
        compair: List<string> temp = new List<string>();

        for (int i = 0; i < property.arraySize; i++)
        {
            var itemProp = property.GetArrayElementAtIndex(i);
            var assetNameProp = itemProp.FindPropertyRelative("assetName");
            if (!temp.Contains(assetNameProp.stringValue))
            {
                temp.Add(assetNameProp.stringValue);
            }
            else
            {
                property.DeleteArrayElementAtIndex(i);
                goto compair;
            }
        }
    }

    private void CloseAllCreated(SerializedProperty arrayProp)
    {
        TrySaveAllPrefabs(arrayProp);
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var item = arrayProp.GetArrayElementAtIndex(i);
            var instanceIDPorp = item.FindPropertyRelative("instanceID");
            var obj = EditorUtility.InstanceIDToObject(instanceIDPorp.intValue);
            if (obj != null){
                DestroyImmediate(obj);
            }
            instanceIDPorp.intValue = 0;
        }
    }
    private void SortAllBundles(SerializedProperty property)
    {
        for (int i = 0; i < property.arraySize; i++)
        {
            for (int j = i; j < property.arraySize - i - 1; j++)
            {
                var itemj = property.GetArrayElementAtIndex(j).FindPropertyRelative("assetName");
                var itemj1 = property.GetArrayElementAtIndex(j + 1).FindPropertyRelative("assetName");
                if (string.Compare(itemj.stringValue, itemj1.stringValue) > 0)
                {
                    property.MoveArrayElement(j, j + 1);
                }
            }
        }
    }

    private void TrySaveAllPrefabs(SerializedProperty arrayProp)
    {
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var item = arrayProp.GetArrayElementAtIndex(i);
            var instanceIDPorp = item.FindPropertyRelative("instanceID");
            var obj = EditorUtility.InstanceIDToObject(instanceIDPorp.intValue);
            if (obj == null) continue;
            var prefab = PrefabUtility.GetPrefabParent(obj);
            if (prefab != null)
            {
                var root = PrefabUtility.FindPrefabRoot((GameObject)prefab);
                if (root != null)
                {
                    PrefabUtility.ReplacePrefab(obj as GameObject, root, ReplacePrefabOptions.ConnectToPrefab);
                }
            }
        }
    }
}
