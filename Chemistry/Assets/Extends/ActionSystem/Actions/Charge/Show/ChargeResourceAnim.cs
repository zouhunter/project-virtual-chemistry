using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{

    public class ChargeResourceAnim : ChargeResourceBinding
    {
        [SerializeField]
        private Transform scaleTrans;
        private ScaleChargeCtrl scaleCtrl;
        protected override void Awake()
        {
            base.Awake();
            scaleCtrl = new ScaleChargeCtrl(scaleTrans, target.capacity);
        }
        protected override void Update()
        {
            base.Update();
            scaleCtrl.Update();
        }
        protected override void OnCharge(Vector3 center, ChargeData data, UnityAction onComplete)
        {
            if (onComplete != null)
            {
                scaleCtrl.AddAsync(data, animTime);
                StartAsync(onComplete);
            }
            else
            {
                scaleCtrl.Add(data);
            }
        }
    }

}