using UnityEngine;
using System.Collections;
using System;
using Collections;

public class EventNotifaction<T> : INotification<T>
{
    public T Body { get; set; }
    public Type Type { get; set; }
	public string ObserverName { get; set; }
    public new string ToString
    {
        get
        {
            string msg = "";
            msg += "\nObserverName:" + ObserverName.ToString();
            msg += "\nBody:" + ((Body == null) ? "null" : Body.ToString());
            msg += "\nType:" + ((Type == null) ? "null" : Type.ToString());
            return msg;
        }
    }
    public bool IsUsing { get; set; }

    public bool Destroy { get; set; }
    public virtual void Clear()
    {
        Body = default(T);
        Type = null;
        ObserverName = null;
        IsUsing = false;
    }

    // ******************************** OBJECT POOL ********************************

    /// <summary>
    /// Allows us to reuse objects without having to reallocate them over and over
    /// </summary>
    private static ObjectPool<EventNotifaction<T>> sPool = new ObjectPool<EventNotifaction<T>>(1, 1);

    //public static int Length { get { return sPool.Length; } }
    /// <summary>
    /// Pulls an object from the pool.
    /// </summary>
    /// <returns></returns>
    public static EventNotifaction<T> Allocate(string ObserverName, T body, Type Type)
    {
        EventNotifaction<T> lInstance = sPool.Allocate();
        if (lInstance == null) { lInstance = new EventNotifaction<T>(); }

        lInstance.ObserverName = ObserverName;
        lInstance.Body = body;
        lInstance.Type = Type;

        lInstance.IsUsing = true;
        return lInstance;
    }

    /// <summary>
    /// Returns an element back to the pool.
    /// </summary>
    /// <param name="rEdge"></param>
    public void Release()
    {
        if (this == null) { return; }

        Clear();

        // Make it available to others.
        if (this is EventNotifaction<T>)
        {
            sPool.Release((EventNotifaction<T>)this);
        }
    }
}
