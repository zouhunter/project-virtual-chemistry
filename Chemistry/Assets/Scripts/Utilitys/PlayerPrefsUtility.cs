using UnityEngine;
using System.Collections;

public static class PlayerPrefsUtility {

    /// <summary>
    /// 读取保存的数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static int GetIntFromPlayerPrefs(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetInt(key, default(int));
        }
        return PlayerPrefs.GetInt(key);
    }
    public static float GetFloatFromPlayerPrefs(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetFloat(key, default(float));
        }
        return PlayerPrefs.GetFloat(key);
    }
    public static string GetStringFromePlayerPrefs(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetString(key, default(string));
        }
        return PlayerPrefs.GetString(key);
    }
    /// <summary>
    /// 第一次设置返回true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetValueToPlayerPrefs(string key, object value)
    {
        if (value is string)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetString(key, (string)value);
                return true;
            }
            PlayerPrefs.SetString(key, (string)value);
            return false;
        }
        else if (value is float)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetFloat(key, (float)value);
                return true;
            }
            PlayerPrefs.SetFloat(key, (float)value);
            return false;
        }
        else if (value is int)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, (int)value);
                return true;
            }
            PlayerPrefs.SetInt(key, (int)value);
            return false;
        }
        return false;
    }
}
