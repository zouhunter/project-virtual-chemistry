using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem.Binding
{
    /// <summary>
    /// 在一个触发器时间内触发关闭和打开
    /// 或 打开或关闭事件
    /// </summary>
    public class ActionEventSwitch : ActionObjEventSender
    {
        public bool activeOnStart;
        public bool activeOnComplete;

        protected string resetKey { get { return "HideResetObjects"; } }
        protected string hideKey { get { return "HideObjects"; } }
        protected string showKey { get { return "UnHideObjects"; } }

        protected override void OnBeforeActive(bool forceAuto)
        {
            if(activeOnStart)
            {
                eventCtrl.NotifyObserver(showKey, key);
            }
            else
            {
                eventCtrl.NotifyObserver(hideKey, key);
            }
        }
        protected override void OnBeforeComplete(bool force)
        {
            if (activeOnComplete)
            {
                eventCtrl.NotifyObserver(showKey, key);
            }
            else
            {
                eventCtrl.NotifyObserver(hideKey, key);
            }
        }
        protected override void OnBeforeUnDo()
        {
            eventCtrl.NotifyObserver(resetKey, key);
        }

    }
}