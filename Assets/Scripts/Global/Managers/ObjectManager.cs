using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ObjectManager : Singleton<ObjectManager>
{
    //创建对象池字典
    public Dictionary<string, List<GameObject>> poolObj = new Dictionary<string, List<GameObject>>();

    private List<GameObject> currentList;
    /// <summary>
    /// 用于创建静止的物体，指定父级、坐标
    /// </summary>
    /// <returns></returns>
    public GameObject GetPoolObject(GameObject pfb, Transform parent, bool world, bool resetLocalPosition = false, bool resetLocalScale = false,bool activepfb = false)
    {
        pfb.SetActive(true);
        GameObject currGo;
        ////Debug.Log(pfb.name);
        //如果有预制体为名字的对象小池
        if (poolObj.ContainsKey(pfb.name))
        {
            currentList = poolObj[pfb.name];
            //遍历每数组，得到一个隐藏的对象
            for (int i = 0; i < currentList.Count; i++)
            {
                if (currentList[i] != null && !currentList[i].activeSelf)
                {
                    currentList[i].SetActive(true);
                    currentList[i].transform.SetParent(parent, world);
                    if (resetLocalPosition)
                    {
                        currentList[i].transform.localPosition = Vector3.zero;
                    }

                    if (resetLocalScale)
                    {
                        currentList[i].transform.localScale = Vector3.one;
                    }
                    pfb.SetActive(activepfb);
                    return currentList[i];
                }
            }
            //当没有隐藏对象时，创建一个并返回
            currGo = CreateAGameObject(pfb, parent, world, resetLocalPosition, resetLocalScale);
            currentList.Add(currGo);
            pfb.SetActive(activepfb);
            return currGo;
        }
        currGo = CreateAGameObject(pfb, parent, world, resetLocalPosition,resetLocalScale);
        //如果没有对象小池
        poolObj.Add(currGo.name, new List<GameObject>() { currGo });
        pfb.SetActive(activepfb);
        return currGo;
    }

    GameObject CreateAGameObject(GameObject pfb, Transform parent, bool world, bool resetLocalPositon,bool resetLocalScale)
    {
        GameObject currentGo = Instantiate(pfb);
        currentGo.name = pfb.name;
        currentGo.transform.SetParent(parent,world);
        if (resetLocalPositon)
        {
            currentGo.transform.localPosition = Vector3.zero;
        }
        if(resetLocalScale){
            currentGo.transform.localScale = Vector3.one;
        }
        return currentGo;
    }

    public void SavePoolObject(GameObject go,bool world = false)
    {
        go.transform.SetParent(transform, world);
        go.SetActive(false);
    }

    public void ClearAllObject()
    {
        //transform.DetachChildren();

        if (currentList != null)
            currentList.Clear();

        if (poolObj != null)
            poolObj.Clear();

        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    public void ClearObjectByPrefab(GameObject pfb)
    {
        if (poolObj == null)
            return;

        if (!poolObj.ContainsKey(pfb.name))
            return;

        var currList = poolObj[pfb.name];
        poolObj.Remove(pfb.name);
        currList.Clear();
    }
}
