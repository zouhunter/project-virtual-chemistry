using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.Yaml.Serialization;
#endif
/// <summary>
/// 全局变量
/// </summary>
public partial class ConfigManager
{

}
/// <summary>
/// 数据加载和注册
/// </summary>
public partial class ConfigManager : Singleton<ConfigManager>
{
    #region 记录
#if UNITY_EDITOR
    [Space(4)]
    [InspectorButton("SaveToPerfer")]
    public int 保存预设Group;
    public ReceiveAbleObject perferSystem;
    public void SaveToPerfer()
    {
        PerferGroup[] groups = FindObjectsOfType<PerferGroup>();
        perferSystem.systems.Clear();
        ReceiveSystem system;
        for (int i = 0; i < groups.Length; i++)
        {
            system = new ReceiveSystem();
            system.systemName = groups[i].name;
            system.perfabInfos = groups[i].objects;
            perferSystem.systems.Add(system);
        }
        UnityEditor.EditorUtility.SetDirty(perferSystem);
    }
    [Space(4)]
    [InspectorButton("SaveToElement")]
    public int 记录元素;
    public PrefabItemObject elements;
    public string elementsresourePath;
    public void SaveToElement()
    {
        SavePrefabItem(elements, elementsresourePath);
    }
    [Space(4)]
    [InspectorButton("SaveToMedecim")]
    public int 记录药品;
    public PrefabItemObject medecim;
    public string medecimsourePath;
    public void SaveToMedecim()
    {
        SavePrefabItem(medecim, medecimsourePath);
    }
    static void SavePrefabItem(PrefabItemObject prefebObj,string path)
    {
        GameObject[] lodeadsObj = Resources.LoadAll<GameObject>(path);
        prefebObj.prefabItem.Clear();
        PrefabItem pItem;
        foreach (var item in lodeadsObj)
        {
            pItem = item.GetComponentSecure<RecordingPrefab>().prefabItemInfo;
            prefebObj.prefabItem.Add(pItem);
        }
        UnityEditor.EditorUtility.SetDirty(prefebObj);
    }


    [Space(4)]
    [InspectorButton("SaveToYAML")]
    public int 保存YAML测试;
    public string savePath;
    public void SaveToYAML()
    {
        YamlSerializer seri = new YamlSerializer();
        seri.SerializeToFile(savePath + "yaml.txt", perferSystem.systems);
        AssetDatabase.Refresh();
    }
    [Space(4)]
    [InspectorButton("LoadFromYAML")]
    public int 加载YAML测试;
    public void LoadFromYAML()
    {
        YamlSerializer seri = new YamlSerializer();
        object[] data = seri.DeserializeFromFile(savePath + "yaml.txt", typeof(List<ReceiveSystem>));
        List<ReceiveSystem>[] data0 = data.OfType<List<ReceiveSystem>>().ToArray();
        Debug.Log(data0[0][0].perfabInfos[0].elementName);

    }
#endif
    #endregion
}
