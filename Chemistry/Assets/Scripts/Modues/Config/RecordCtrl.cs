using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class RecordCtrl {

    private List<RecordingPrefab> records = new List<RecordingPrefab>();
    public event UnityAction<RecordingPrefab> onActivePrefabChanged;

    /// <summary>
    /// 设置激活状态的prefab
    /// </summary>
    public void ChangeActivePrefab(RecordingPrefab activeItem)
    {
        if(onActivePrefabChanged!=null)
            onActivePrefabChanged(activeItem);
    }
    /// <summary>
    /// 记录场景中打开的对象
    /// </summary>
    /// <param name="item"></param>
    public void SaveRecord(RecordingPrefab item)
    {
        if (!records.Contains(item))
        {
            records.Add(item);
            ChangeActivePrefab(item);
        }
    }
    /// <summary>
    /// 清除打开的对象
    /// </summary>
    /// <param name="item"></param>
    public void RemoveRecord(RecordingPrefab item)
    {
        if (records.Contains(item))
        {
            records.Remove(item);
            ChangeActivePrefab(null);
        }
    }
    /// <summary>
    /// 查找对象
    /// </summary>
    /// <param name="key"></param>
    /// <param name="recordItem"></param>
    /// <returns></returns>
    public List<RecordingPrefab> GetRecordingPrefabs()
    {
        return records;
    }
}

