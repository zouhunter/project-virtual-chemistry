using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class InfomationContain : MonoBehaviour {
    public InputField inputFiled;
	public string GetInfomation () {
        return inputFiled.text;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
