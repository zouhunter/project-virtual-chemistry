using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class PlayerHelp
{
    public string title;
    [Multiline(4)]
    public string infomation;

    public PlayerHelp(string title,string infomation)
    {
        this.title = title;
        this.infomation = infomation;
    }
}