using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;

namespace WorldActionSystem.Binding
{
    public class ActionHighLighter : ActionObjBinding
    {
        [SerializeField]
        protected Color color = Color.green;
        [SerializeField]
        protected GameObject viewObj;
        protected Highlighter highLighter;
        protected bool noticeAuto { get { return Config.highLightNotice; } }
        protected override void Awake()
        {
            base.Awake();
            if (viewObj == null) viewObj = gameObject;
            highLighter = viewObj.GetComponent<Highlighter>();
            if (highLighter == null)
            {
                highLighter = viewObj.AddComponent<Highlighter>();
            }
        }
        protected void Update()
        {
            if (!noticeAuto) return;

            if (actionObj.Complete) return;

            if (actionObj.Started & !actionObj.Complete)
            {
                HighLight();
            }
            else
            {
                UnHighLight();
            }
        }

        protected override void OnBeforeActive(bool forceAuto)
        {
            if (noticeAuto)
            {
                HighLight();
            }
        }
        protected override void OnBeforeComplete(bool force)
        {
            if (noticeAuto)
            {
                UnHighLight();
            }
        }
        protected override void OnBeforeUnDo()
        {
            if (noticeAuto)
            {
                UnHighLight();
            }
        }

        public void HighLight()
        {
            if (highLighter)
            {
                highLighter.FlashingOn(Color.white, color);
            }
        }

        public void UnHighLight()
        {
            if (highLighter)
            {
                highLighter.FlashingOff();
                highLighter.Off();
            }
        }
    }
}