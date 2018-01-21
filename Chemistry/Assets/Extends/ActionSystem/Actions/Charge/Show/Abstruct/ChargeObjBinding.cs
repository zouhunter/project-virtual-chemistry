using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{
    [RequireComponent(typeof(ChargeObj))]
    public abstract class ChargeObjBinding : ChargeBinding
    {
        protected ChargeObj target;
        protected virtual void Awake()
        {
            target = GetComponent<ChargeObj>();
            target.onCharge = OnCharge;
        }

        protected virtual void OnCharge(Vector3 center, ChargeData data, UnityAction onComplete)
        {
            if (onComplete != null)
            {
                StartAsync(onComplete);
            }
        }
    }
}