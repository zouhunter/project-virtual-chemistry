using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class RecordingPrefab : MonoBehaviour {
    public HoverOutLine outline;
    public HoverHandShow hand;
    public BuildingInfo buildInfo;
    public PrefabItem prefabItemInfo;


    private RecordCtrl Record { get { return GameManager.recordCtrl; } }
    void Start()
    {
        Record.SaveRecord(this);
        if (outline.singleRenderer == null)
        {
            outline.singleRenderer = GetComponent<Renderer>();
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MouseBehavior GetMouseBehavior()
    {
        MouseBehavior mitem = gameObject.GetComponentSecure<MouseBehavior>();
        mitem.outline = outline;
        mitem.hand = hand;
        return mitem;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public BuildingItem GetBuildItem()
    {
        BuildingItem bitem = gameObject.GetComponentSecure<BuildingItem>();
        bitem.buildingInfo = buildInfo;
        bitem.Init();
        return bitem;
    }
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Record.ChangeActivePrefab(this);
        }
    }

    void OnDestroy()
    {
        if (!GameManager.isQuit)
        {
            GameManager.recordCtrl.RemoveRecord(this);
        }
    }
}

