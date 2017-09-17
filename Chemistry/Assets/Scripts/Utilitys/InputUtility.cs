using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using UnityEngine.Events;
using System.IO;

public static class InputUtility {
    /// <summary>
    /// 是否在一定时间完成点击
    /// </summary>
    /// <param name="Timer"></param>
    /// <param name="keyNum"></param>
    /// <returns></returns>
    public static bool HaveClickedInTime(ref float Timer,int keyNum)
    {
        if (Input.GetMouseButtonDown(keyNum)|| Input.GetMouseButtonUp(keyNum))
        {
            return HaveExecuteTwicePerSecond(ref Timer);
        }
        return false;
    }
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
    private static bool HaveExecuteTwicePerSecond(ref float timer)
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


   
  

}
