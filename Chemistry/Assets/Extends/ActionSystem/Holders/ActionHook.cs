using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{
    public enum HookExecuteType
    {
        BeforeStart =1,
        BeforeComplete=0
    }
    public interface IActionHook
    {
        //HookExecuteType ExecuteType { get; }
        int QueueID { get; }
        bool Complete { get; }
        bool Started { get; }
        UnityAction onEndExecute { get; set; }
        void OnUnDoExecute();
        void OnEndExecute(bool force);
        void OnStartExecute(bool isForceAuto);
    }

    public abstract class ActionHook : MonoBehaviour, IActionHook
    {
        protected bool _complete;
        public bool Complete { get { return _complete; } }
        protected bool _started;
        public bool Started { get { return _started; } }
        [SerializeField, Range(0, 10)]
        private int queueID;
        public int QueueID
        {
            get
            {
                return queueID;
            }
        }
        public string CameraID { get { return null; } }
        protected abstract bool autoComplete { get; }
        protected float autoTime = 2;
        Coroutine coroutine;
        public UnityAction onEndExecute { get; set; }
        public Toggle.ToggleEvent onBeforeEndExecuted;
        public static bool log = false;
        public virtual void OnStartExecute(bool auto)
        {
            if(log) Debug.Log("onStart Execute Hook :" + this,gameObject);
            if (!_started)
            {
                _started = true;
                _complete = false;
                gameObject.SetActive(true);
                //onBeforeStart.Invoke(auto);
                if (autoComplete && coroutine == null && gameObject.activeInHierarchy) {
                    coroutine = StartCoroutine(AutoComplete());
                }
            }
            else
            {
                Debug.LogError("already started" + name, gameObject);
            }
        }
        protected virtual IEnumerator AutoComplete()
        {
            yield return new WaitForSeconds(autoTime);
            if(!Complete) OnEndExecute(false);
        }
        public virtual void OnEndExecute(bool force)
        {
            if (!Complete)
            {
                CoreEndExecute(force);
            }
        }

        public virtual void CoreEndExecute(bool force)
        {
            if (log) Debug.Log("onEnd Execute Hook :" + this + ":" + force, gameObject);
            if (!_complete)
            {
                _started = true;
                _complete = true;
                onBeforeEndExecuted.Invoke(force);

                if (onEndExecute != null)
                {
                    onEndExecute.Invoke();
                }
                if (autoComplete && coroutine != null)
                {
                    StopCoroutine(coroutine);
                    coroutine = null;
                }
            }
            else
            {
                Debug.LogError("already completed" + this, gameObject);
            }

        }

        public virtual void OnUnDoExecute()
        {
            _started = false;
            _complete = false;
            if (autoComplete && coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
    }
}