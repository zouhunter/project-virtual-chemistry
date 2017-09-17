using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;

public  static class PointerUtility {

    /// <summary>
    /// 在Update中调用判断是否进行了双击
    /// </summary>
    /// <returns></returns>
    public static bool HaveClickMouseTwice(ref float Timer, int keyNum)
    {
        if (Input.GetMouseButtonDown(keyNum))
        {
            return HaveExecuteTwicePerSecond(ref Timer);
        }
        return false;
    }
    /// <summary>
    /// 是否在指定的时间执行了两次
    /// </summary>
    /// <param name="offsetTime"></param>
    /// <param name="currentTime"></param>
    /// <param name="timer"></param>
    /// <param name="executeOnce"></param>
    /// <returns></returns>
    public static bool HaveExecuteTwicePerSecond(ref float timer)
    {
        if (Time.time - timer < 1f)
        {
            return true;
        }
        else
        {
            timer = Time.time;
            return false;
        }
    }



    public static void SimulateButtonClick(Button button)
    {
        if (button)
        {
            PointerEventData data = new PointerEventData(EventSystem.current);

            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
        }
    }
    public static void SimulateToggleClick(Toggle toggle)
    {
        if (toggle)
        {
            PointerEventData data = new PointerEventData(EventSystem.current);

            ExecuteEvents.Execute(toggle.gameObject, data, ExecuteEvents.pointerDownHandler);
        }
    }
}
