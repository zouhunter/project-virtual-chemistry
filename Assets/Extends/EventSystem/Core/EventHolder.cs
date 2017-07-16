using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class EventHolder : IEventHolder
{
    #region 
    protected static volatile IEventHolder m_instance;
    protected readonly object m_syncRoot = new object();
    protected static readonly object m_staticSyncRoot = new object();
    protected EventHolder()
    {
    }
    public static IEventHolder Instance
    {
        get
        {
            if (m_instance == null)
            {
                lock (m_staticSyncRoot)
                {
                    if (m_instance == null) m_instance = new EventHolder();
                }
            }

            return m_instance;
        }
    }
    static EventHolder()
    {

    }
    #endregion

    public Dictionary<string, Delegate> m_needHandle = new Dictionary<string, Delegate>();
    public void NoMessageHandle<T>(INotification<T> rMessage)
    {
        Debug.LogWarning("MessageDispatcher: Unhandled Message of type " + rMessage.ObserverName);
    }

    #region 注册注销事件
    public void AddDelegate(string key, Delegate handle)
    {
        // First check if we know about the message type
        if (!m_needHandle.ContainsKey(key))
        {
            m_needHandle.Add(key, handle);
        }
        else
        {
            m_needHandle[key] = Delegate.Combine(m_needHandle[key], handle);
        }
    }
    public bool RemoveDelegate(string key, Delegate handle)
    {
        if (m_needHandle.ContainsKey(key))
        {
            m_needHandle[key] = Delegate.Remove(m_needHandle[key], handle);
            if (m_needHandle[key] == null)
            {
                m_needHandle.Remove(key);
                return false;
            }
        }
        return true;
    }
    public void RemoveDelegates(string key)
    {
        if (m_needHandle.ContainsKey(key))
        {
            m_needHandle.Remove(key);
        }
    }
    public bool HaveEvent(string key)
    {
        return m_needHandle.ContainsKey(key);
    }
    #endregion

    #region 触发事件
    public void NotifyObserver<T>(INotification<T> rMessage)
    {
        bool lReportMissingRecipient = true;

        if (m_needHandle.ContainsKey(rMessage.ObserverName))
        {
            var body = rMessage.GetType().GetProperty("Body");

            if (body != null)
            {
                var data = body.GetValue(rMessage, null);

                if (data != null)
                {
                    m_needHandle[rMessage.ObserverName].DynamicInvoke(data);
                }
                else
                {
                    m_needHandle[rMessage.ObserverName].DynamicInvoke();
                }
            }
            else
            {
                m_needHandle[rMessage.ObserverName].DynamicInvoke();
            }

            lReportMissingRecipient = false;
        }

        // If we were unable to send the message, we may need to report it
        if (lReportMissingRecipient)
        {
            NoMessageHandle(rMessage);
        }
    }
    #endregion
}
