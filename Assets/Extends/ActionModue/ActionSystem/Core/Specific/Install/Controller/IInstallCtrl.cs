using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public interface IInstallCtrl
    {
        //刷新状态
        void Reflesh();

        bool CurrStapComplete();
        void SetStapActive(string stapName);
        void AutoInstallWhenNeed(string stapName);
        void EndInstall(string stapName);
        void UnInstall(string stapName);
        void QuickUnInstall(string stapName);

    }
}