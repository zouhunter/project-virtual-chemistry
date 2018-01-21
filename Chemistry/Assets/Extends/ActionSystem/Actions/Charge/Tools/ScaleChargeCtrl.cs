using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using WorldActionSystem;
using System;
using UnityEngine.Events;
using UnityEngine.Assertions.Comparers;

namespace WorldActionSystem
{
    public class ScaleChargeCtrl
    {
        private float currentValue;
        private Vector3 startScale;
        private Vector3 targetScale;
        private float timer = 0;
        private float lerpTime = 2f;
        private Transform transform;
        private bool asyncActive;
        private float capicty;

        public void Sub(ChargeData arg0)
        {
            var data = arg0;
            data.value = -arg0.value;
            Add(data);
        }
        public void SubAsync(ChargeData arg0, float lerpTime)
        {
            var data = arg0;
            data.value = -arg0.value;
            AddAsync(data, lerpTime);
        }
        public void AddAsync(ChargeData arg0, float lerpTime)
        {
            currentValue += arg0.value;
            AsyncSet(lerpTime);
        }
        public void Add(ChargeData arg0)
        {
            currentValue += arg0.value;
            QuickSet();
        }

        private void QuickSet()
        {
            transform.localScale = new Vector3(transform.localScale.x, currentValue / capicty, transform.localScale.z);
        }

        private void AsyncSet(float lerpTime)
        {
            this.lerpTime = lerpTime;
            startScale = transform.localScale;
            targetScale = new Vector3(transform.localScale.x, currentValue/ capicty, transform.localScale.z);
            timer = 0;
            asyncActive = true;
        }

        public ScaleChargeCtrl(Transform transform,float capicty)
        {
            this.capicty = capicty;
            this.transform = transform;
        }

        public void Update()
        {
            if (asyncActive && timer < lerpTime)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, timer / lerpTime);
                timer += Time.deltaTime;

                if(timer >= lerpTime){
                    asyncActive = false;
                }
            }
        }
    }

}