using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem.Binding
{
    [RequireComponent(typeof(ActionObj))]
    public class ActionObjBinding : MonoBehaviour
    {
        protected ActionObj actionObj;
    
        protected virtual void Awake()
        {
            actionObj = gameObject.GetComponent<ActionObj>();
            actionObj.onStartExecute.AddListener(OnBeforeActive);
            actionObj.onBeforeComplete.AddListener(OnBeforeComplete);
            actionObj.onUnDoExecute.AddListener(OnBeforeUnDo);
        }
        private void OnDestroy()
        {
            if (actionObj)
            {
                actionObj.onStartExecute.RemoveListener(OnBeforeActive);
                actionObj.onBeforeComplete.RemoveListener(OnBeforeComplete);
                actionObj.onUnDoExecute.RemoveListener(OnBeforeUnDo);
            }
        }
        protected virtual void OnBeforeActive(bool forceAuto) { }
        protected virtual void OnBeforeComplete(bool force) { }
        protected virtual void OnBeforeUnDo() { }
    }
}