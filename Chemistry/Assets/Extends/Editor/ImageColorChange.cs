using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class ImageColorChange : ScriptableWizard
{
    [MenuItem("Tools/ImageColorChange")]
    static void OpenWindow()
    {
        DisplayWizard<ImageColorChange>("图片色修改", "关闭");
    }

    public List<Transform> parent = new List<Transform>();
    public List<Image> images = new List<Image>();
    public Color color;

    protected override bool DrawWizardGUI()
    {
        if (GUILayout.Button("清空"))
        {
            parent.Clear();
            images.Clear();
        }
        if (GUILayout.Button("遍历获取"))
        {
            GetTexts();
        }
        if (GUILayout.Button("加载字号"))
        {
            if (parent != null)
            {
                SetFont();
            }
        }
        return base.DrawWizardGUI();
    }

    void OnWizardCreate()
    {
        Close();
    }
    void GetTexts()
    {
        if (parent != null)
        {
            images.Clear();
            for (int i = 0; i < parent.Count; i++)
            {
                LoadChild(parent[i]);
            }
        }
    }
    void SetFont()
    {
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i] != null)
            {
                images[i].color = color;
                EditorUtility.SetDirty(images[i]);
            }
        }
    }
    void LoadChild(Transform parent)
    {
        Image img = parent.GetComponent<Image>();
        if (img != null)
        {
            images.Add(img);
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
