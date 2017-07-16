using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// 设置面版
/// </summary>
public class ToolWindow : ScriptableWizard
{
    static ToolWindow window;
    [MenuItem("Tools/ToolWindow")]
    static void wizard()
    {
        ScriptableWizard.DisplayWizard<ToolWindow>("工具面版", "关闭");
    }

    public SerializingToText stotext;
    void OnEnable()
    {
        stotext = new SerializingToText();
    }
    protected override bool DrawWizardGUI()
    {
        stotext.GUI();
        return base.DrawWizardGUI();
    }
    void OnWizardCreate()
    {
    }
}
