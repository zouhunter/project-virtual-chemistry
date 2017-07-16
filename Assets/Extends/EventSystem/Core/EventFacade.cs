using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
public class EventFacade : IEventFacade
{
    protected IEventHolder m_EventHolder;
	public static volatile IEventFacade Instance = new EventFacade();
    protected readonly object m_syncRoot = new object();
    public EventFacade()
    {
        InitializeEventHolder();
    }
    protected virtual void InitializeEventHolder()
    {
        if (m_EventHolder != null) return;
        m_EventHolder = EventHolder.Instance;
    }


    #region 访问事件系统
    public void RegisterEvent(string noti, UnityAction even)
    {
        m_EventHolder.AddDelegate(noti, even);
    }

    public void RegisterEvent<T>(string noti, UnityAction<T> even)
    {
        m_EventHolder.AddDelegate(noti, even);
    }
    
    public void RemoveEvent(string noti, UnityAction even)
    {
        m_EventHolder.RemoveDelegate(noti, even);
    }

    public void RemoveEvent<T>(string noti, UnityAction<T> even)
    {
        m_EventHolder.RemoveDelegate(noti, even);
    }

    public void RemoveEvents(string noti)
    {
        m_EventHolder.RemoveDelegates(noti);
    }

    #endregion


    /// <summary>
    /// 通知观察者
    /// </summary>
    /// <param name="notification"></param>
	private void NotifyObservers<T>(INotification<T> notification)
    {
        if (m_EventHolder.HaveEvent(notification.ObserverName))
        {
            m_EventHolder.NotifyObserver<T>(notification);
        }
    }
    public void SendNotification(string observeName)
    {
        SendNotification<object>(observeName, null, null);
    }
    public void SendNotification<T>(string observeName, T body)
    {
        SendNotification<T>(observeName, body, null);
    }
    public void SendNotification<T>(string observeName, T body, Type type)
    {
        EventNotifaction<T> notify = EventNotifaction<T>.Allocate(observeName, body, type);
        NotifyObservers(notify);
        notify.Release();
    }
}
