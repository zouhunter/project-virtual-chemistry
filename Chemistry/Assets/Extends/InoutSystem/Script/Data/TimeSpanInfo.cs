using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace FlowSystem
{
    public class TimeSpanInfo
    {
        public float spanTime;
        public float timer;
        public bool active;
        public TimeSpanInfo(float spanTime)
        {
            active = true;
            this.spanTime = spanTime;
        }

        public bool OnSpanComplete()
        {
            if (!active) return false;

            timer += Time.deltaTime;
            if (timer > spanTime)
            {
                timer = 0;
                return true;
            }
            return false;
        }
    }
}
