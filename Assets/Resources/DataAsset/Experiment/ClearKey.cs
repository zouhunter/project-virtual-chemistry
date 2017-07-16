using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class ClearKey : MonoBehaviour {

    public string[] keys;
    void OnGUI()
    {
        if (GUILayout.Button("清除指定key"))
        {
            for (int i = 0; i < keys.Length; i++)
            {
                PlayerPrefs.DeleteKey(keys[i]);
            }
        }
    }
}
