using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public class ChargeBinding : MonoBehaviour
    {
        [SerializeField]
        protected float animTime = 2;

        protected float timer = 0;
        protected bool asyncActive = false;
        protected UnityAction onComplete { get; set; }

        protected virtual void Update()
        {
            if (asyncActive && timer < animTime)
            {
                timer += Time.deltaTime;
                if (timer > animTime)
                {
                    CompleteAsync();
                }
            }
        }
        protected virtual void StartAsync(UnityAction onComplete)
        {
            TryCleanLast();
            this.onComplete = onComplete;
            asyncActive = true;
            timer = 0;
        }

        private void CompleteAsync()
        {
            asyncActive = false;
            OnBeforeCompleteAsync();
            if (onComplete != null){
                var action = onComplete;
                onComplete = null;
                action.Invoke();
            }
        }
        private void TryCleanLast()
        {
            //OnBeforeCompleteAsync();
            if (onComplete != null)
            {
                onComplete.Invoke();
                onComplete = null;
            }
        }
        protected virtual void OnBeforeCompleteAsync() { }
    }
}