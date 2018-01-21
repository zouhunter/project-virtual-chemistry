using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{
    [RequireComponent(typeof(ChargeTool))]
    public abstract class ChargeToolBingding : ChargeBinding
    {
        protected ChargeTool target;
        protected virtual void Awake()
        {
            target = GetComponent<ChargeTool>();
            target.onCharge = OnCharge;
            target.onLoad = OnLoad;
        }
        protected abstract void OnLoad(Vector3 center, ChargeData data, UnityAction onComplete);
        protected abstract void OnCharge(Vector3 center, ChargeData data, UnityAction onComplete);
    }
}