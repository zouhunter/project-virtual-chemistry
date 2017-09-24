using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 处理输入和输出事件触发
/// </summary>
namespace ReactSystem
{
    public interface IActiveAble:IElement
    {
        bool Active(bool force = false);
    }
    public interface IContainer: IActiveAble,ITube
    {
        event UnityAction<IContainer> onComplete;
        event Func<IContainer,List<string>> onGetSupports;
        void AddStartElement(string[] types);
    }
}