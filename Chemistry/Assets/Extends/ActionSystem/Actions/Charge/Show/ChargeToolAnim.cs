using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using WorldActionSystem;
using System;
namespace WorldActionSystem
{
    public class ChargeToolAnim : ChargeToolBingding
    {
        [SerializeField]
        private Animation anim;
        [SerializeField]
        private Transform animParent;
        [SerializeField]
        private string loadAnimName;
        [SerializeField]
        private string chargeAnimName;
        [SerializeField]
        private Transform scaleTrans;
     
        private ScaleChargeCtrl scaleCtrl;
        private AnimChargeCtrl animCtrl;
        
        protected override void Awake()
        {
            base.Awake();
            scaleCtrl = new ScaleChargeCtrl(scaleTrans,target.capacity);
            animCtrl = new AnimChargeCtrl(animParent, anim);
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
                StartAsync(onComplete);//先执行异步计时，不然重置动画的依据不足

                scaleCtrl.SubAsync(data, animTime);
                animCtrl.PlayAnim(chargeAnimName, center, animTime);
            }
            else
            {
                scaleCtrl.Sub(data);
            }
        }

        protected override void OnLoad(Vector3 center, ChargeData data, UnityAction onComplete)
        {
            if (onComplete != null)
            {
                StartAsync(onComplete);//先执行异步计时，不然重置动画的依据不足

                scaleCtrl.AddAsync(data, animTime);
                animCtrl.PlayAnim(loadAnimName, center, animTime);
            }
            else
            {
                scaleCtrl.Add(data);
            }
            Debug.Log("OnLoad:" + data);
        }
        protected override void OnBeforeCompleteAsync()
        {
            base.OnBeforeCompleteAsync();
            if(animCtrl != null && animCtrl.Started)//异步开始后开始
            {
                animCtrl.StopAnim();
            }
        }
    }

}