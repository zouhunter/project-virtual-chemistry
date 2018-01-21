using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{

    public class ActionObjCtroller
    {
        public ActionCommand trigger { get;private set; }
        protected List<int> queueID = new List<int>();
        protected IActionObj[] actionObjs { get;private set; }
        public IActionObj[] StartedActions { get { return startedActions.ToArray(); } }
        protected bool isForceAuto;
        private Queue<IActionObj> actionQueue = new Queue<IActionObj>();
        private List<IActionObj> startedActions = new List<IActionObj>();
        public static bool log = false;
        public UnityAction<ControllerType> onCtrlStart { get; set; }
        public UnityAction<ControllerType> onCtrlStop { get; set; }
        private ActionGroup _system;
        private ActionGroup system { get { trigger.transform.SurchSystem(ref _system); return _system; } }
        private CameraController cameraCtrl
        {
            get
            {
                return ActionSystem.Instence.cameraCtrl;
            }
        }

        public ActionObjCtroller(ActionCommand trigger)
        {
            this.trigger = trigger;
            actionObjs = trigger.ActionObjs;
            ChargeQueueIDs();
        }

        public virtual void OnStartExecute(bool forceAuto)
        {
            this.isForceAuto = forceAuto;
            ExecuteAStep();
        }

        internal void OnPickUpObj(PickUpAbleItem obj)
        {
            var prio = startedActions.Find(x => x.Name == obj.Name);
            if (prio != null)
            {
                startedActions.Remove(prio);
                startedActions.Insert(0, prio);
            }
        }

        private void ChargeQueueIDs()
        {
            actionQueue.Clear();
            queueID.Clear();
            foreach (var item in actionObjs)
            {
                if (!queueID.Contains(item.QueueID))
                {
                    queueID.Add(item.QueueID);
                }
            }
            queueID.Sort();
        }
        public virtual void OnEndExecute()
        {
            StopUpdateAction(false);
            CompleteQueues();
            Array.Sort(actionObjs);
            foreach (var item in actionObjs)
            {
                if (!item.Started)
                {
                    item.OnStartExecute(isForceAuto);
                }
                if (!item.Complete)
                {
                    item.OnEndExecute(true);
                }
            }

        }

        public virtual void OnUnDoExecute()
        {
            StopUpdateAction(true);
            UnDoQueues();
            ChargeQueueIDs();
            Array.Sort(actionObjs);
            Array.Reverse(actionObjs);
            foreach (var item in actionObjs)
            {
                if (item.Started)
                {
                    item.OnUnDoExecute();
                }
            }
        }


        private void OnCommandObjComplete(IActionObj obj)
        {
            OnStopAction(obj);
            var notComplete = Array.FindAll<IActionObj>(actionObjs, x => x.QueueID == obj.QueueID && !x.Complete);
            if (notComplete.Length == 0)
            {
                if (!ExecuteAStep())
                {
                    if (!trigger.Completed)
                        trigger.Complete();
                }
            }
            else if (actionQueue.Count > 0)//正在循环执行
            {
                QueueExectueActions();
            }
        }

        public void CompleteOneStarted()
        {
            if (startedActions.Count > 0)
            {
                var action = startedActions[0];
                OnStopAction(action);
                action.OnEndExecute(true);
            }
            else
            {
                if (log) Debug.Log("startedActions.Count == 0");
            }
        }
        private void CompleteQueues()
        {
            while (actionQueue.Count > 0)
            {
                var action = actionQueue.Dequeue();
                if (!action.Complete)
                {
                    action.OnEndExecute(true);
                }
            }
        }
        private void UnDoQueues()
        {
            while (actionQueue.Count > 0)
            {
                var action = actionQueue.Dequeue();
                if (action.Started)
                {
                    action.OnUnDoExecute();
                }
            }
        }
        protected bool ExecuteAStep()
        {
            if (queueID.Count > 0)
            {
                var id = queueID[0];
                queueID.RemoveAt(0);
                var neetActive = Array.FindAll<IActionObj>(actionObjs, x => x.QueueID == id && !x.Started);
                if (isForceAuto)
                {
                    actionQueue.Clear();
                    foreach (var item in neetActive)
                    {
                        if (item.QueueInAuto)
                        {
                            actionQueue.Enqueue(item as ActionObj);
                        }
                        else
                        {
                            TryStartAction(item);
                        }
                    }
                    QueueExectueActions();
                }
                else
                {
                    foreach (var item in neetActive)
                    {
                        var obj = item;
                        TryStartAction(obj);
                    }
                }
                return true;
            }
            return false;
        }

        protected void QueueExectueActions()
        {
            if (actionQueue.Count > 0)
            {
                var actionObj = actionQueue.Dequeue();
                if (log) Debug.Log("QueueExectueActions" + actionObj);
                TryStartAction(actionObj);
            }
        }
        private void TryStartAction(IActionObj obj)
        {
            if (log) Debug.Log("Start A Step:" + obj);
            if (!obj.Started)
            {
                if (cameraCtrl != null)
                {
                    cameraCtrl.SetViewCamera(() =>
                    {
                        StartAction(obj);
                    }, GetCameraID(obj));
                }
                else
                {
                    StartAction(obj);
                }
            }
            else
            {
                Debug.LogError(obj + " allready started");
            }

        }

        private void StartAction(IActionObj obj)
        {
            if (!obj.Started)
            {
                obj.onEndExecute = () => OnCommandObjComplete(obj);
                obj.OnStartExecute(isForceAuto);
                OnStartAction(obj);
            }

        }

        /// <summary>
        /// 添加新的触发器
        /// </summary>
        /// <param name="action"></param>
        private void OnStartAction(IActionObj action)
        {
            startedActions.Add(action);
            if (onCtrlStart != null) onCtrlStart.Invoke(action.CtrlType);
        }

        /// <summary>
        /// 移除触发器
        /// </summary>
        /// <param name="action"></param>
        private void OnStopAction(IActionObj action)
        {
            startedActions.Remove(action);
            if (onCtrlStop != null && startedActions.Find(x=>x.CtrlType == action.CtrlType) == null){
                onCtrlStop.Invoke(action.CtrlType);
            }
        }

        private string GetCameraID(IActionObj obj)
        {
            //忽略匹配相机
            if (Config.quickMoveElement && obj is MatchObj && !(obj as MatchObj).ignorePass)
            {
                return null;
            }
            else if (Config.quickMoveElement && obj is InstallObj && !(obj as InstallObj).ignorePass)
            {
                return null;
            }
            //除要求使用特殊相机或是动画步骤,都用主摄像机
            else if (Config.useOperateCamera || obj is AnimObj)
            {
                if (string.IsNullOrEmpty(obj.CameraID))
                {
                    return trigger.CameraID;
                }
                else
                {
                    return obj.CameraID;
                }
            }
            //默认是主相机
            else
            {
                return CameraController.defultID;
            }
        }

        private void StopUpdateAction(bool force)
        {
            if (cameraCtrl != null)
            {
                cameraCtrl.StopStarted(force);
            }
        }
    }
}