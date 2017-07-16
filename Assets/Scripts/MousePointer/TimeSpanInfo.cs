using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class TimeSpanInfo {
    public float spanTime;
    public float timer;

    public TimeSpanInfo(float spanTime)
    {
        this.spanTime = spanTime;
    }

    public bool OnSpanComplete()
    {
        timer += Time.deltaTime;
        if (timer > spanTime)
        {
            timer = 0;
            return true;
        }
        return false;
    }
}
