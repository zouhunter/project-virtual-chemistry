using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

abstract public class Singleton<T> :MonoBehaviour where T : MonoBehaviour
{
    private static T instance = default(T);
    private static object lockHelper = new object();

    public static bool mManualReset = false;

    protected Singleton() { }
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockHelper)
                {
                    if (instance == null && !program.isQuit)
                    {
                        GameObject go = new GameObject(typeof(T).ToString());
                        go.transform.SetParent(program.Instence.transform);
                        instance = go.AddComponent<T>();
                    }
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        instance = GetComponent<T>();
    }

    protected void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
};

