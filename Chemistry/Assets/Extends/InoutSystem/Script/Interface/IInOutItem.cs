using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Tuples;
/// <summary>
/// 处理输入和输出事件触发
/// </summary>
namespace FlowSystem
{
    public interface IInOutItem
    {
        bool AutoReact();
        void FunctionIn(int nodeID, string type,UnityAction onComplete);
        void RecordReactInfo(IFlowSystemCtrl ctrl);
    }
}