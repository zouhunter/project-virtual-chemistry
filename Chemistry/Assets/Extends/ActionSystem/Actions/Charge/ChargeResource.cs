using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace WorldActionSystem
{
    public class ChargeResource : MonoBehaviour, ISupportElement
    {
        [SerializeField]
        private ChargeData startData;
        [SerializeField]
        private float _capacity =1;
        public string type { get { return startData.type; } }
        public float current { get; private set; }
        public ChargeEvent onChange { get; set; }
        public float capacity { get { return _capacity; } }

        #region ISupportElement
        public string Name { get { return name; } }
        public bool Started { get; private set; }

        public void StepActive()
        {
            Started = true;
        }

        public void StepComplete()
        {
            Started = true;
        }

        public void StepUnDo()
        {
            var extro = current - startData.value;
            if (onChange != null){//把多的去掉
                onChange.Invoke(transform.position, new ChargeData(type, -extro), null);
            }
            current = startData.value;
            Started = false;
        }

        #endregion

        private ElementController elementCtrl;

        protected void Awake()
        {
            elementCtrl = ElementController.Instence;
            elementCtrl.RegistElement(this);
            InitLayer();
        }
        private void OnDestroy()
        {
            if (elementCtrl != null)
                elementCtrl.RemoveElement(this);
        }
        private void Start()
        {
            InitCurrent();
        }

        public void Subtruct(float value, UnityAction onComplete)
        {
            current -= value;
            if (onChange != null){
                onChange.Invoke(transform.position, new ChargeData(type, -value), onComplete);
            }
            else
            {
                if (onComplete != null)
                    onComplete.Invoke();
            }
        }
        private void InitCurrent()
        {
            current = startData.value;
            if (onChange != null) {
                onChange.Invoke(transform.position, new ChargeData(type, current), null);
            }
        }

        private void InitLayer()
        {
            GetComponentInChildren<Collider>().gameObject.layer = LayerMask.NameToLayer(Layers.chargeResourceLayer);
        }
    }

}