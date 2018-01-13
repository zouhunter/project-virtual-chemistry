using System;
using UnityEngine;
using System.Collections;
namespace ReactSystem
{
    public interface ITube:IElement
    {
        event Func<ITube, int, string[], bool> onExport;//发生反应事件
        void Import(int nodeID, string[] type);
    }
}