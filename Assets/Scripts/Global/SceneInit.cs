using UnityEngine;
using System.Collections;
public class SceneInit : MonoBehaviour {
#if UNITY_EDITOR
    [InspectorButton("InitScene")]
    public bool 初始化场景;
    public GameObject[] needHideItems;
    public GameObject[] needOpenItems;
    void InitScene()
    {
        foreach (var item in needHideItems)
        {
            item.SetActive(false);
        }
        foreach (var item in needOpenItems)
        {
            item.SetActive(true);
        }
    }
#endif
}
