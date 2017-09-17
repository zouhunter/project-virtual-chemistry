using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class BuildingCtrl
{
    private List<BuildingItem> allBuildings = new List<BuildingItem>();

    public void RemoveBuilding(BuildingItem item)
    {
        if (allBuildings.Contains(item))
        {
            allBuildings.Remove(item);
        }
    }

    public void AddNewBuiliding(BuildingItem item)
    {
        if (!allBuildings.Contains(item))
        {
            allBuildings.Add(item);
        }
    }

    public bool HaveBuild(Vector3 pos)
    {
        for (int k = 0; k < allBuildings.Count; k++)
        {
            BuildingItem build = allBuildings[k];
            //print(build);
            for (int i = 0; i < build.buildingInfo.locat.GetLength(0); i++)
            {
                for (int j = 0; j < build.buildingInfo.locat.GetLength(1); j++)
                {
                    Grid grid = build.buildingInfo.locat[i, j];
                    if (grid.pos.x == pos.x && grid.pos.z == pos.z)
                    {
                        //垂直距离
                        if (build.buildingInfo.height > Mathf.Abs(pos.y - build.transform.position.y))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    public GameObject OnCreateButtonClicked(GameObject itemPfb,Transform parent)
    {
        GameObject item = ObjectManager.Instance.GetPoolObject(itemPfb,parent,true);
        item.GetComponentSecure<RecordingPrefab>().GetBuildItem();
        return item;
    }
}

