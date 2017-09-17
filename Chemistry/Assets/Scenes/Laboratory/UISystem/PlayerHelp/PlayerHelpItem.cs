using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerHelpItem : MonoBehaviour,IPointerClickHandler {
    public Text title;
    public Text info;

    private InputField titleInput;
    private InputField infoInput;

    public void InitPlayerHelpItem(string title,string info,InputField titleInput, InputField infoInput)
    {
        this.title.text = title;
        this.info.text = info;
        this.titleInput = titleInput;
        this.infoInput = infoInput;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        titleInput.text = title.text;
        infoInput.text = info.text;
    }

    public void LoadBack()
    {
        title.text = titleInput.text;
        info.text = infoInput.text;
    }
}

