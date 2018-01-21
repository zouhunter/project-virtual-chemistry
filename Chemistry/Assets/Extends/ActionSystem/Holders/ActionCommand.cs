using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    [AddComponentMenu(MenuName.ActionCommand)]
    public class ActionCommand : MonoBehaviour, IActionCommand, IComparable<ActionCommand>
    {
        [SerializeField]
        private string _stepName;
        [SerializeField, Range(0, 10)]
        private int _queueID;
        public int QueueID { get { return _queueID; } }
        [SerializeField]
        private string _cameraID = CameraController.defultID;
        public string CameraID { get { return _cameraID; } }
        public string StepName { get { if (string.IsNullOrEmpty(_stepName)) _stepName = name; return _stepName; } }
        public bool Startd { get { return started; } }
        public bool Completed { get { return completed; } }
        private UserError userErr { get; set; }
        private StepComplete stepComplete { get; set; }//步骤自动结束方法
        public IActionObj[] ActionObjs { get { return actionObjs; } }
        protected ActionCtroller ActionCtrl { get { return ActionSystem.Instence.actionCtrl; } }
        public ActionObjCtroller ActionObjCtrl { get { return objectCtrl; } }

        protected IActionObj[] actionObjs;
        private ActionObjCtroller objectCtrl;
        [EnumMask]
        public ControllerType commandType;

#if ActionSystem_G
        [HideInInspector]
#endif
        public InputField.OnChangeEvent onBeforeActive, onBeforePlayEnd, onBeforeUnDo;

        private bool started;
        private bool completed;
        private ActionGroup _system;
        private ActionGroup system { get { transform.SurchSystem(ref _system); return _system; } }
        protected CommandController commandCtrl { get { return system == null ? null : system.CommandCtrl; } }

        protected virtual void Awake()
        {
            RegistActionObjs();
            WorpCameraID();
            objectCtrl = new ActionObjCtroller(this);
        }
        protected virtual void Start()
        {
            TryRegistToActionSystem();
        }
        private void TryRegistToActionSystem()
        {
            if (commandCtrl != null) commandCtrl.RegistCommand(this);
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
        private void RegistActionObjs()
        {
            actionObjs = GetComponentsInChildren<IActionObj>(false);
        }

        public void RegistAsOperate(UserError userErr)
        {
            this.userErr = userErr;
        }
        public void RegistComplete(StepComplete stepComplete)
        {
            this.stepComplete = stepComplete;
        }
        public int CompareTo(ActionCommand other)
        {
            if (other.QueueID > QueueID)
            {
                return -1;
            }
            else if (other.QueueID == QueueID)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        internal void UserError(string err)
        {
            if (userErr != null)
            {
                userErr.Invoke(StepName, err);
            }
        }

        /// <summary>
        /// 操作过程自动结束
        /// </summary>
        internal bool Complete()
        {
            if (!completed)
            {
                started = true;
                completed = true;
                OnEndExecute();
                if (stepComplete != null) stepComplete.Invoke(StepName);
                return true;
            }
            else
            {
                Debug.Log("already completed" + name);
                return false;
            }
        }

        public virtual bool StartExecute(bool forceAuto)
        {
            if (!started)
            {
                started = true;
                onBeforeActive.Invoke(StepName);
                ActionCtrl.OnStartExecute(objectCtrl, forceAuto);
                return true;
            }
            else
            {
                Debug.Log("already started" + name);
                return false;
            }
        }
        /// <summary>
        /// 强制结束
        /// </summary>
        public virtual bool EndExecute()
        {
            //Debug.Log("EndExecute", gameObject);

            if (!completed)
            {
                started = true;
                completed = true;
                OnEndExecute();
                return true;
            }
            else
            {
                Debug.Log("already completed" + name);
                return false;
            }

        }

        public void OnEndExecute()
        {
            onBeforePlayEnd.Invoke(StepName);
            ActionCtrl.OnEndExecute(objectCtrl);
        }

        public virtual void UnDoExecute()
        {
            started = false;
            completed = false;
            ActionCtrl.OnUnDoExecute(objectCtrl);
        }


    }
}

