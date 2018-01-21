using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

namespace WorldActionSystem
{
    public interface IOperateController
    {
        ControllerType CtrlType { get; }
        UnityAction<string> userErr { get; set; }
        void Update();
    }

}