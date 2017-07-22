using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class Destrict
{
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float minZ;
    public float maxZ;
}
public class HandPanel : MonoBehaviour
{
    public Destrict Des;
    public Image centerImage;
    public string sendName;
    [Space(5)]
    public float moveSpeed;
    public float rotSpeed;

    [Space(5)]
    public Button up;
    public Button down;
    public Button left;
    public Button right;
    public Button forward;
    public Button back;
    public Button leftR;
    public Button rightR;

    public RecordCtrl recordCtrl { get { return Laboratory.Current.recordCtrl; } }
    private bool selected;
    private Transform selectedObj;
    private bool pressed;
    private UnityAction action;
    void Start()
    {
        recordCtrl.onActivePrefabChanged += OnItemSelected;
        EventDispatcher.Get(up).onPointerDown += (x)=> OnDirectionButtonPressed(Vector3.up * moveSpeed * Time.deltaTime);
        EventDispatcher.Get(down).onPointerDown += (x)=> OnDirectionButtonPressed(Vector3.down * moveSpeed * Time.deltaTime);
        EventDispatcher.Get(left).onPointerDown += (x)=> OnDirectionButtonPressed(Vector3.left * moveSpeed * Time.deltaTime);
        EventDispatcher.Get(right).onPointerDown += (x)=> OnDirectionButtonPressed(Vector3.right * moveSpeed * Time.deltaTime);
        EventDispatcher.Get(forward).onPointerDown += (x) => OnDirectionButtonPressed(Vector3.forward * moveSpeed * Time.deltaTime);
        EventDispatcher.Get(back).onPointerDown += (x) => OnDirectionButtonPressed(Vector3.back * moveSpeed * Time.deltaTime);

        EventDispatcher.Get(leftR).onPointerDown += (x) => OnRotationButtonPressed(Vector3.forward * rotSpeed * Time.deltaTime);
        EventDispatcher.Get(rightR).onPointerDown += (x) => OnRotationButtonPressed(Vector3.back * rotSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 选中对象
    /// </summary>
    /// <param name="recordItem"></param>
    void OnItemSelected(RecordingPrefab recordItem)
    {
        selected = recordItem == null ? false : true;
        if (selected)
        {
            centerImage.sprite = recordItem.prefabItemInfo.sprite;
            //Facade.Instance.SendNotification<string>(sendName, "选中了" + recordItem.prefabItemInfo.prefabName);
            selectedObj = recordItem.gameObject.transform;
        }
    }
    #region 操作方法
    void OnDirectionButtonPressed(Vector3 target)
    {
        pressed = true;
        action = () =>
        {
            selectedObj.position = DestrictPos(selectedObj.position + target);
        };
    }
    void OnRotationButtonPressed(Vector3 axis)
    {
        pressed = true;
        action = () =>
        {
            selectedObj.rotation =Quaternion.Euler(axis) * selectedObj.rotation;
        };
    }
    #endregion
    void Update()
    {
        if (!selected || !pressed || action == null) return;

        Debug.Log(selected.ToString() + pressed + action + "");

        if (Input.GetMouseButtonUp(0))
        {
            pressed = false;
            Debug.Log("Up");
        }
        else
        {
            action();
            Debug.Log("Up");

        }
     
    }

    void OnDestroy()
    {
        if (!program.isQuit)
        {
            recordCtrl.onActivePrefabChanged -= OnItemSelected;
        }
    }

    Vector3 DestrictPos(Vector3 pos)
    {
        if (pos.x > Des.maxX)
        {
            pos.x = Des.maxX;
        }
        else if(pos.x < Des.minX)
        {
            pos.x = Des.minX;
        }
        else if (pos.y > Des.maxY)
        {
            pos.y = Des.maxY;
        }
        else if (pos.y < Des.minY)
        {
            pos.y = Des.minY;
        }
        else if(pos.z < Des.minZ)
        {
            pos.z = Des.minZ;
        }
        else if(pos.z > Des.maxZ)
        {
            pos.z = Des.maxZ;
        }
        return pos;
    }
}

