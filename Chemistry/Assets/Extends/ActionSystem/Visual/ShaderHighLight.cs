using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
#if !NoFunction
using HighlightingSystem;
#endif
namespace WorldActionSystem
{
    public class ShaderHighLight : IHighLightItems
    {
#if !NoFunction
        private float freq = 1;
        public Dictionary<GameObject, Highlighter> highlightDic = new Dictionary<GameObject, Highlighter>();
#endif
        public void HighLightTarget(Renderer go, Color color)
        {
            if (go == null) return;
#if !NoFunction
            Highlighter highlighter;
            if (!highlightDic.ContainsKey(go.gameObject))
            {
                highlighter = go.gameObject.GetComponent<Highlighter>();
                if (highlighter == null)
                {
                    highlighter = go.gameObject.AddComponent<Highlighter>();
                }
                highlighter.On();
                highlighter.SeeThroughOn();
                highlightDic.Add(go.gameObject, highlighter);
            }
            highlightDic[go.gameObject].FlashingOn(Color.white, color, freq);
#endif
        }
        public void HighLightTarget(GameObject go, Color color)
        {
            if (go == null) return;
#if !NoFunction
            Highlighter highlighter;
            if (!highlightDic.ContainsKey(go))
            {
                highlighter = go.gameObject.GetComponent<Highlighter>();
                if (highlighter == null)
                {
                    highlighter = go.gameObject.AddComponent<Highlighter>();
                }
                highlighter.On();
                highlighter.SeeThroughOn();
                highlightDic.Add(go, highlighter);
            }
            highlightDic[go].FlashingOn(Color.white, color, freq);
#endif
        }

        public void UnHighLightTarget(Renderer go)
        {
            if (go == null) return;
#if !NoFunction
            Highlighter highlighter;
            if (highlightDic.TryGetValue(go.gameObject, out highlighter))
            {
                highlighter.Off();
            }
#endif
        }

        public void UnHighLightTarget(GameObject go)
        {
            if (go == null) return;
#if !NoFunction
            Highlighter highlighter;
            if (highlightDic.TryGetValue(go, out highlighter))
            {
                highlighter.Off();
            }
#endif
        }
    }
}