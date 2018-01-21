using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public abstract class OperateController : IOperateController
    {
        public abstract ControllerType CtrlType { get; }
        public UnityAction<string> userErr { get; set; }
        public UnityAction<IPlaceItem> onSelect { get; set; }
        private CameraController cameraCtrl
        {
            get
            {
                return ActionSystem.Instence.cameraCtrl;
            }
        }
        protected Camera viewCamera
        {
            get
            {
                return cameraCtrl.currentCamera;
            }
        }
        public abstract void Update();

        protected virtual void SetUserErr(string errInfo)
        {
            if (userErr != null)
            {
                this.userErr(errInfo);
            }
        }
    }
}