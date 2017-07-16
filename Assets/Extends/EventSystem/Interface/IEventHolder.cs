using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IEventHolder {
    void AddDelegate(string key, Delegate handle);
    bool RemoveDelegate(string key, Delegate handle);
    void RemoveDelegates(string key);
    bool HaveEvent(string key);
    void NotifyObserver<T>(INotification<T> notify);
}
