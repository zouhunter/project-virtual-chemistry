using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
//using System.Tuples;

public class InstallObjectPosSetter : MonoBehaviour
{
    [System.Serializable]
    public class PosTemp
    {
        [System.Serializable]
        public class TransformTemp
        {
            public Vector3 position;
            public Vector3 eular;
            public Vector3 size;
        }

        public string key;
        public List<TransformTemp> objTransforms = new List<TransformTemp>();
    }

    public List<Transform> objectList = new List<Transform>();
    public List<PosTemp> switchList = new List<PosTemp>();
    public static string key { get; set; }
    void Start()
    {
        PosTemp data = switchList.Find(x => x.key == key);
        Transform titem;
        if (data != null)
        {
            for (int i = 0; i < objectList.Count; i++)
            {
                titem = objectList[i];
                var item = data.objTransforms[i];
                titem.localPosition = item.position;
                titem.localEulerAngles = item.eular;
                titem.localScale = item.size;
            }
        }
    }

    public void RegisterTransform(string key, Transform target, Vector3 pos, Vector3 rot, Vector3 size)
    {
        ///更新坐标数
        if (!objectList.Contains(target))
        {
            objectList.Add(target);
            for (int i = 0; i < switchList.Count; i++)
            {
                switchList[i].objTransforms.Add(new PosTemp.TransformTemp());
            }
        }

        ///添加状态数
        var oldPosTemp = switchList.Find(x => x.key == key);
        if (oldPosTemp == null)
        {
            oldPosTemp = new PosTemp();
            oldPosTemp.key = key;
            for (int i = 0; i < objectList.Count; i++)
            {
                oldPosTemp.objTransforms.Add(new PosTemp.TransformTemp());
            }
            switchList.Add(oldPosTemp);
        }
        

        ///记录坐标等信息
        var index = objectList.IndexOf(target);
        oldPosTemp.objTransforms[index].position = pos;
        oldPosTemp.objTransforms[index].eular = rot;
        oldPosTemp.objTransforms[index].size = size;
    }
}
