using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class Fire : MonoBehaviour
{
    private bool isOn;
    public bool IsOn
    {
        get { return isOn; }
        set
        {
            isOn = value;
            if (isOn)
            {
                LightUp();
            }
            else
            {
                LightOff();
            }
        }
    }
    private GameObject wick;
    List<GameObject> sphereCasts = new List<GameObject>();
    public float lightDistence = 0.1f;
    [Range(1,20)]
    public int updateFrame = 10;
    private int stick = 0;
    void Start()
    {
        wick = transform.Find("FireMobile").gameObject;
    }
    void Update()
    {
        if (IsOn)
        {
            if (gameObject.UpdateTimeStick(ref stick, updateFrame))
            {
                TryLightOnOther();
            }
        }
    }
    /// <summary>
    /// 点燃接近的可燃物
    /// </summary>
    private void TryLightOnOther()
    {
        VectorUtility.GetColliderObjectAround(transform.position, lightDistence, ref sphereCasts, tag);
        for (int i = 0; i < sphereCasts.Count; i++)
        {
            Debug.Log(sphereCasts[i].name);
            Fire other = sphereCasts[i].GetComponent<Fire>();
            if (other != null)
            {
                other.IsOn = true;
            }
        }
    }
    /// <summary>
    /// 关闭灯芯
    /// </summary>
    private void LightOff()
    {
        wick.SetActive(false);
    }

    /// <summary>
    /// 点燃灯芯
    /// </summary>
    private void LightUp()
    {
        wick.SetActive(true);
    }
}

