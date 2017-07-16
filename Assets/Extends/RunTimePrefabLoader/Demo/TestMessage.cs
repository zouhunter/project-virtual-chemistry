using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class TestMessage : MonoBehaviour
{
   void OnGUI()
    {
        if (GUILayout.Button("发送message生成红色Cylinder"))
        {
            EventFacade.Instance.SendNotification<Color>("cylinder", Color.red);
        }
        if (GUILayout.Button("发送message生成绿色Cylinder"))
        {
            EventFacade.Instance.SendNotification<Color>("cylinder", Color.green);
        }
    }
}
