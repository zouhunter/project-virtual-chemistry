using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class TextFontChange : ScriptableWizard
{
    [MenuItem("Tools/FontChange")]
    static void OpenWindow()
    {
        DisplayWizard<TextFontChange>("字体更换", "关闭");
    }

    public List<Transform> parent = new List<Transform>();
    public List<Text> texts = new List<Text>();
    public int fontSize;

    protected override bool DrawWizardGUI()
    {
        if (GUILayout.Button("清空"))
        {
            parent.Clear();
            texts.Clear();
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
            texts.Clear();
            for (int i = 0; i < parent.Count; i++)
            {
                LoadChild(parent[i]);
            }
        }
    }
    void SetFont()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            if (texts[i] != null)
            {
                texts[i].fontSize = fontSize;
                EditorUtility.SetDirty(texts[i]);
            }
        }
    }
    void LoadChild(Transform parent)
    {
        Text text = parent.GetComponent<Text>();
        if (text != null)
        {
            texts.Add(text);
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
