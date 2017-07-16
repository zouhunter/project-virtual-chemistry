using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class IntorPanel : MonoBehaviour {
    public Text title;
    public GameObject prefab;
    private List<StapIntro> loaded = new List<StapIntro>();

     /// <summary>
     /// 加载实验步骤信息
     /// </summary>
     /// <param name="experiment"></param>
	public void LoadIntroduceInfomation (Experiment experiment) {

        ///清除已经加载的对象
        if (loaded != null)
        {
            foreach (var item in loaded)
            {
                ObjectManager.Instance.SavePoolObject(item.gameObject);
            }
        }
        ///加载标题
        title.text = experiment.name;
        ///创建步骤信息
        loaded.Clear();
        GameObject sitem;
        StapIntro intro;
        StapInfo info;
        Transform parent = prefab.transform.parent;
        for (int i = 0; i < experiment.staps.Count; i++)
        {
            info = experiment.staps[i];
            sitem = ObjectManager.Instance.GetPoolObject(prefab, parent, false);
            intro = sitem.GetComponent<StapIntro>();
            intro.InitStapIntro(info.name, info.infomation);
            loaded.Add(intro);
        }
	}
}
