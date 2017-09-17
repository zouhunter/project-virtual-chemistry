using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public static class SingleNameUtility {
    public const string address = "(Clone)";

    private static Dictionary<string, int> idDespatch = new Dictionary<string, int>();

    public static void ReName(this GameObject item)
    {
        string name = item.name;
        if (idDespatch.ContainsKey(name))
        {
            int count = idDespatch[name] = idDespatch[name]++;
            for (int i = 0; i < count; i++)
            {
                name += address;
            }
            item.name = name;
        }
        else
        {
            idDespatch[name] = 1;
            item.name = name + address;
        }
    }
}
