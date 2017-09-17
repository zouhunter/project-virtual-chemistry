using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class StapIntro : MonoBehaviour {
    public Text stapName;
    public Text stapInfo;

    public void InitStapIntro(string name,string info)
    {
        stapName.text = name;
        stapInfo.text = info;
    }
}
