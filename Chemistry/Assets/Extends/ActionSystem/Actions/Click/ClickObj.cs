using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    [AddComponentMenu(MenuName.ClickObj)]
    public class ClickObj : ActionObj
    {
        private float autoCompleteTime { get { return Config.autoExecuteTime; } }

        public override ControllerType CtrlType
        {
            get
            {
               return ControllerType.Click;
            }
        }

        private Coroutine waitCoroutine;
#if ActionSystem_G
        [HideInInspector]
#endif
        public UnityEvent onMouseDown, onMouseEnter, onMouseExit;
      
        protected override void Start(){
            base.Start();
            InitLayer();
        }
        private void InitLayer()
        {
            GetComponentInChildren<Collider>().gameObject.layer =LayerMask.NameToLayer( Layers.clickItemLayer);
        }

        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            if (auto){
                if (waitCoroutine == null)
                {
                    StartCoroutine(WaitClose());
                }
            }
        }
        IEnumerator WaitClose()
        {
            yield return new WaitForSeconds(autoCompleteTime);
            OnEndExecute(false);
        }
        public override void OnEndExecute(bool force)
        {
            base.OnEndExecute(force);
            if (auto && waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }
        }
        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();
            if (auto && waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }
        }
        
        public void OnMouseDown()
        {
            onMouseDown.Invoke();
        }
        public void OnMouseEnter()
        {
            onMouseEnter.Invoke();
        }
        public void OnMouseExit()
        {
            onMouseExit.Invoke();
        }
    }

}