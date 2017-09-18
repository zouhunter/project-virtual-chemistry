using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace ReactSystem
{
    public interface IReactSystemCtrl
    {
        event UnityAction onComplete;
        event UnityAction<IContainer> onStepBreak;
        IContainer ActiveItem { get; }
        Func<IContainer, int, Dictionary<IContainer, int>> GetConnectedDic { get; set; }
        Func<IContainer, List<ISupporter>> GetSupportList { get; set; }

        void ReStart();
        bool TryStartProducer();
        void TryNextContainer();
        void InitExperiment(List<RunTimeElemet> elements);
    }
}