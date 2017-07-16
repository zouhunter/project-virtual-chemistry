using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class TestMessageget : MonoBehaviour,IRunTimeMessage {
    public event UnityAction OnDelete;
    public Renderer render;
    public void HandleMessage(object message)
    {
        if (message is Color)
        {
            render.material.color = (Color)message;
        }
    }

    void OnDestroy()
    {
        if (OnDelete != null)
            OnDelete.Invoke();
    }
}
