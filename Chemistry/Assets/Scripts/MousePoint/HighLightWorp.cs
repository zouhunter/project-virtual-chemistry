using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;

public class HighLightWorp : MonoBehaviour
{
    public GameObject target;
    private Highlighter _highlighter;

    public Highlighter HighLighter
    {
        get
        {
            if (_highlighter == null)
            {
                if (target == null) {
                    target = gameObject;
                }
                _highlighter = target.AddComponent<Highlighter>();
            }
            return _highlighter;
        }

    }
}
