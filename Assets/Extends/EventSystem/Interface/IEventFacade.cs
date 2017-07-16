using UnityEngine;
using System;
using UnityEngine.Events;
public interface IEventFacade
{
    void RegisterEvent(string noti, UnityAction even);
    void RegisterEvent<T>(string noti, UnityAction<T> even);

    void RemoveEvent(string noti, UnityAction even);
    void RemoveEvent<T>(string noti, UnityAction<T> even);
    void RemoveEvents(string noti);

    void SendNotification(string notificationName);
    void SendNotification<T>(string notificationName, T body);
}
