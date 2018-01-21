using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using WorldActionSystem;
namespace WorldActionSystem
{
    public abstract class ActionObj : MonoBehaviour, IActionObj
    {
        [SerializeField]
        protected string m_name;
        public bool startActive;
        public bool endActive;
        protected bool _complete;
        public string Name { get { if (string.IsNullOrEmpty(m_name)) m_name = name; return m_name; } }
        public bool Complete { get { return _complete; } }
        protected bool _started;
        public bool Started { get { return _started; } }
        protected bool auto;
        [SerializeField, Range(0, 10)]
        private int queueID;
        public int QueueID
        {
            get
            {
                return queueID;
            }
        }
        [SerializeField]
        private bool _queueInAuto = true;
        public bool QueueInAuto { get { return _queueInAuto; } }
        [SerializeField]
        private string _cameraID;
        public string CameraID { get { return _cameraID; } }
        public Transform anglePos;
        public UnityAction onEndExecute { get; set; }

#if ActionSystem_G
        [HideInInspector]
#endif
        public Toggle.ToggleEvent onStartExecute, onBeforeComplete;
#if ActionSystem_G
        [HideInInspector]
#endif
        public UnityEvent onUnDoExecute;
        private ActionHook[] hooks;//外部结束钩子
        public ActionHook[] Hooks { get { return hooks; } }
        private HookCtroller hookCtrl;
        protected AngleCtroller angleCtrl { get { return ActionSystem.Instence.angleCtrl; } }
        private ActionGroup _system;
        public ActionGroup system { get { transform.SurchSystem(ref _system); return _system; } }
        protected ElementController elementCtrl { get { return ElementController.Instence; } }

        public abstract ControllerType CtrlType { get; }
        public static bool log = true;
        protected bool notice;
        protected virtual void Start()
        {
            hooks = GetComponentsInChildren<ActionHook>(false);
            if (hooks.Length > 0)
            {
                hookCtrl = new HookCtroller(this);
            }
            if (anglePos == null){
                anglePos = transform;
            }
            WorpCameraID();
            gameObject.SetActive(startActive);
        }
        private void WorpCameraID()
        {
            if (string.IsNullOrEmpty(_cameraID))
            {
                var node = GetComponentInChildren<CameraNode>();
                if (node != null)
                {
                    _cameraID = node.name;
                }
            }
        }
        protected virtual void Update()
        {
            if (Complete || !Started) return;

            if (!Config.angleNotice || this is AnimObj) return;

            if (notice)
            {
                angleCtrl.Notice(anglePos);
            }
            else
            {
                angleCtrl.UnNotice(anglePos);
            }

        }

        private void OnStartExecuteInternal(bool auto)
        {
            this.onStartExecute.Invoke(auto);
        }

        public virtual void OnStartExecute(bool auto = false)
        {
            if (log) Debug.Log("OnStartExecute:" + this);
            this.auto = auto;
            if (!_started)
            {
                _started = true;
                _complete = false;
                notice = true;
                gameObject.SetActive(true);
                OnStartExecuteInternal(auto);
            }
            else
            {
                Debug.LogError("already started", gameObject);
            }
        }


        public virtual void OnEndExecute(bool force)
        {
            notice = false;

            if (force)
            {
                if (!Complete) CoreEndExecute(true);
            }
            else
            {
                if (hooks.Length > 0)
                {
                    if (hookCtrl.Complete)
                    {
                        if (!Complete) CoreEndExecute(false);
                    }
                    else if (!hookCtrl.Started)
                    {
                        hookCtrl.OnStartExecute(auto);
                    }
                    else
                    {
                        Debug.Log("wait:" + Name);
                    }
                }
                else
                {
                    if (!Complete) CoreEndExecute(false);
                }
            }
        }
        protected virtual void OnBeforeEnd(bool force)
        {
            onBeforeComplete.Invoke(force);
        }

        private void CoreEndExecute(bool force)
        {
            angleCtrl.UnNotice(anglePos);

            if (log) Debug.Log("OnEndExecute:" + this + ":" + force, this);

            if (!_complete)
            {
                OnBeforeEnd(force);

                notice = false;
                _started = true;
                _complete = true;
                if (hooks.Length > 0){
                    hookCtrl.OnEndExecute();
                }
                gameObject.SetActive(endActive);
                if (onEndExecute != null) {
                    onEndExecute.Invoke();
                }
            }
            else
            {
                Debug.LogError("already completed", gameObject);
            }
        }

        private void OnUnDoExecuteInternal()
        {
            onUnDoExecute.Invoke();
        }
        public virtual void OnUnDoExecute()
        {
            angleCtrl.UnNotice(anglePos);

            if (log) Debug.Log("OnUnDoExecute:" + this, this);

            if (_started)
            {
                _started = false;
                _complete = false;
                notice = false;
                OnUnDoExecuteInternal();
                gameObject.SetActive(startActive);

                if (hooks.Length > 0)
                {
                    hookCtrl.OnUnDoExecute();
                }
            }
            else
            {
                Debug.LogError(this + "allready undo");
            }

        }

        public int CompareTo(IActionObj other)
        {
            if (QueueID > other.QueueID)
            {
                return 1;
            }
            else if (QueueID < other.QueueID)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}