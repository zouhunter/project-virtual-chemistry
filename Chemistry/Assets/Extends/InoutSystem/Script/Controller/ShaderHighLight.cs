using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;
using WorldActionSystem;
public class ShaderHighLight
{
    private float freq = 1;
     Dictionary<GameObject, Highlighter> highlightDic = new Dictionary<GameObject, Highlighter>();

    public void HighLightTarget(GameObject go, Color color)
    {
        Highlighter highlighter;
        if (!highlightDic.ContainsKey(go))
        {
            highlighter = go.gameObject.AddComponent<Highlighter>();
            highlighter.On();
            highlighter.SeeThroughOn();
            highlightDic.Add(go, highlighter);
        }
        highlightDic[go].FlashingOn(Color.white, color, freq);
    }

    public void UnHighLightTarget(GameObject go)
    {
        Highlighter highlighter;
        if (highlightDic.TryGetValue(go, out highlighter))
        {
            highlighter.Off();
        }
    }
}
