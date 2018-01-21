using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public interface IActionObj: IComparable<IActionObj>
    {
        string Name { get; }
        int QueueID { get; }
        bool Complete { get; }
        bool Started { get; }
        bool QueueInAuto { get; }
        ControllerType CtrlType { get; }
        /// <summary>
        /// 相机id:
        /// 1.defult  -> 主摄像机
        /// 2.null    -> 不改变相机状态
        /// 3.自定义  -> 移动
        /// 4.没找到  -> 主摄像机
        /// </summary>
        string CameraID { get; }

        UnityAction onEndExecute { get; set; }

        void OnUnDoExecute();
        void OnEndExecute(bool force);
        void OnStartExecute(bool isForceAuto);
    }
}