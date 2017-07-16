using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
public static class ExtensionUtility
{
     //安全获取
    public static T GetComponentSecure<T>(this GameObject go) where T :Component
    {
        if (go.GetComponent<T>() == null)
        {
            return go.AddComponent<T>();
        }
        return go.GetComponent<T>();
    }
    //安全获取
    public static Component GetComponentSecure(this GameObject go, string str)
    {
        Type tp = Type.GetType(str);
        if (go.GetComponent(str) == null)
        {
            return go.AddComponent(tp);
        }
        return go.GetComponent(tp);
    }
    //安全获取
    public static void AddComponentSecure(this GameObject go, string type)
    {
        Type tp = Type.GetType(type);
        if (tp.IsClass)
        {
            if (go.GetComponent(tp) == null)
            {
                go.AddComponent(tp);
            }
        }
    }
    /// <summary>
    /// 在Update中定时执行
    /// </summary>
    /// <param name="mono"></param>
    /// <param name="stick"></param>
    /// <param name="total"></param>
    /// <returns></returns>
    public static bool UpdateTimeStick(this GameObject go, ref int stick, int total)
    {
        if (stick < total)
        {
            stick++;
            return false;
        }
        else
        {
            stick = 0;
            return true;
        }
    }
}
