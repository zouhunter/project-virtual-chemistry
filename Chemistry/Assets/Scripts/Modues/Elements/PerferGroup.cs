using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 程序内部预先组装好的集合
/// </summary>
public class PerferGroup : MonoBehaviour {
#if UNITY_EDITOR
    [InspectorButton("Record")]
    public int _Record_;
    public void Record()
    {
        objects.Clear();
        PerfabInfo perfabInfo;
        foreach (Transform item in transform)
        {
            perfabInfo = new PerfabInfo();
            perfabInfo.index = item.GetSiblingIndex();
            perfabInfo.position = item.localPosition;
            perfabInfo.rotation = item.localRotation;
            perfabInfo.scale = item.localScale;
            perfabInfo.elementName = item.name;
            objects.Add(perfabInfo);
        }
        objects.Sort();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    [InspectorButton("LoadPerfab")]
    public bool _Load_ = false;
    public void LoadPerfab()
    {
        if (!_Load_)
        {
            Clear();
        }
        else
        {
            GameObject item;
            GameObject perfab;
            string path1;
            string path2;
            for (int i = 0; i < objects.Count; i++)
            {
                path1 = "Assets/Resources/Prefabs/Element/" + objects[i].elementName + ".prefab";
                path2 = "Assets/Resources/Prefabs/Medicine/" + objects[i].elementName + ".prefab";
                //path = UnityEditor.AssetDatabase.GetAssetPath();
                perfab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path1) ?? UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path2);

                item = UnityEditor.PrefabUtility.InstantiatePrefab(perfab) as GameObject;
                item.transform.SetParent(transform, true);
                item.transform.localPosition = objects[i].position;
                item.transform.localRotation = objects[i].rotation;
                item.transform.localScale = objects[i].scale;
                item.SetActive(true);
                loadedObject.Add(item);
            }
        }
        _Load_ = !_Load_;
    }
#endif
    public List<PerfabInfo> objects = new List<PerfabInfo>();
    public List<GameObject> loadedObject = new List<GameObject>();

    public void Load()
    {
        loadedObject.Clear();
        PerfabInfo perfabInfo;
        for (int i = 0; i < objects.Count; i++)
        {
            perfabInfo = objects[i];
            loadedObject.Add(perfabInfo.LoadGameObject(transform));
        }
    }
    public void Clear()
    {
        for (int i = 0; i < loadedObject.Count; i++)
        {
            DestroyImmediate(loadedObject[i]);
        }
        loadedObject.Clear();
    }
}

