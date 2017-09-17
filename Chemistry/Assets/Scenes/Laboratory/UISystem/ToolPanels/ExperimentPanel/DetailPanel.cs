using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class DetailPanel : MonoBehaviour {
    public Text position;
    public Text rotation;

	// Use this for initialization
	public void SetPositionAndRotation (Translation translation) {
        position.text = string.Format("x轴：{0}\ny轴：{1}\nz轴：{2}",translation.position.x, translation.position.y, translation.position.z);
        rotation.text = string.Format("x轴：{0}\ny轴：{1}\nz轴：{2}",translation.rotation.x, translation.rotation.y, translation.rotation.z);
	}
}
