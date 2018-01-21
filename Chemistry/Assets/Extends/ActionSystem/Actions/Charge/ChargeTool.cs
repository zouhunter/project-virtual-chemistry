using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WorldActionSystem
{
    /// <summary>
    /// 用于原料的吸取和填入
    /// </summary>
    public class ChargeTool : PickUpAbleItem, ISupportElement
    {
        [SerializeField]
        private ChargeData startData;
        [SerializeField]
        private List<string> _supportType;
        [SerializeField]
        private float _capacity;
        [SerializeField]
        private float triggerRange = 0.5f;
        private Vector3 startPos;
        private ChargeData chargeData;
        public ChargeEvent onLoad { get; set; }
        public ChargeEvent onCharge { get; set; }

        public bool charged { get { return chargeData.type != null && chargeData.value > 0; } }
        public ChargeData data { get { return (ChargeData)chargeData; } }
        public float capacity { get { return _capacity; } }

        public bool Started { get; private set; }
        public float Range { get { return triggerRange; } }

        private ElementController elementCtrl;

        protected override void Awake()
        {
            base.Awake();
            elementCtrl = ElementController.Instence;
            elementCtrl.RegistElement(this);
        }
        private void Start()
        {
            startPos = transform.localPosition;
            LoadData(transform.position, startData, null);
        }
        private void OnDestroy()
        {
            if (elementCtrl != null)
                elementCtrl.RemoveElement(this);
        }
        public override void OnPickDown()
        {
            base.OnPickDown();
            transform.localPosition = startPos;
        }
        public override void OnPickUp()
        {
            base.OnPickUp();
        }
        public override void OnPickStay()
        {
            base.OnPickStay();

        }
        public override void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }
        internal bool CanLoad(string type)
        {
            return _supportType.Contains(type);
        }
        /// <summary>
        /// 吸入
        /// </summary>
        /// <param name="chargeResource"></param>
        internal void LoadData(Vector3 center, ChargeData data, UnityAction onComplete)
        {
            chargeData = data;

            if (onLoad != null)
            {
                onLoad.Invoke(center, data, onComplete);
            }
            else
            {
                if (onComplete != null)
                    onComplete.Invoke();
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        internal void OnCharge(Vector3 center, float value, UnityAction onComplete)
        {
            var left = data.value - value;
            if (onCharge != null)
            {
                var d = new ChargeData(data.type, value);
                onCharge.Invoke(center, d, onComplete);
            }
            else
            {
                if (onComplete != null) onComplete.Invoke();
            }
            chargeData.value = left;
        }

        public void StepActive()
        {
            PickUpAble = true;
            Started = true;
        }

        public void StepComplete()
        {
            PickUpAble = false;
            Started = true;
        }

        public void StepUnDo()
        {
            PickUpAble = false;
            if(!string.IsNullOrEmpty(chargeData.type)) OnCharge(transform.position, chargeData.value, null);
            LoadData(transform.position, startData, null);
            Started = false;
        }
    }
}