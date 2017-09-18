using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;
using HighlightingSystem;

public class HighLightBehaiver : MonoBehaviour
{
    public float freq = 1;
    public Color color = Color.green;
    private Dictionary<GameObject, Highlighter> highlightDic = new Dictionary<GameObject, Highlighter>();
    private Queue<Highlighter> highLighted = new Queue<Highlighter>();

    public void HighLightTarget(GameObject go)
    {
        UnHightLightAll();
        if (go == null) return;
        var lighter = GetHighLighterFromGo(go);
        lighter.FlashingOn(Color.white, color, freq);
        highLighted.Enqueue(lighter);
    }

    public void UnHighLightTarget(GameObject go)
    {
        if (go == null) return;
        var lighter = GetHighLighterFromGo(go);
        lighter.FlashingOff();
    }

    public void UnHightLightAll()
    {
        while (highLighted.Count > 0)
        {
            var lighter = highLighted.Dequeue();
            lighter.FlashingOff();
        }
    }

    private Highlighter GetHighLighterFromGo(GameObject go)
    {
        Highlighter highlighter;
        if (!highlightDic.TryGetValue(go, out highlighter))
        {
            var worp = go.GetComponent<HighLightWorp>();
            if (worp)
            {
                highlighter = worp.HighLighter;
            }
            else
            {
                highlighter = go.AddComponent<Highlighter>();
            }
            highlighter.On();
            highlighter.SeeThroughOff();
            highlightDic.Add(go, highlighter);
        }
        return highlighter;
    }
}
