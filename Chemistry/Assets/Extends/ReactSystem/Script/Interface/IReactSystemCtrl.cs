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
        event UnityAction<IElement> onStepBreak;
        Func<IElement, int, KeyValuePair<IElement, int>> GetConnectedDic { get; set; }
        Func<IElement, List<IElement>> GetConnectedList { get; set; }

        void ReStart();
        bool TryStartProducer();
        void TryNextContainer();
        void InitExperiment(List<RunTimeElemet> elements);
    }
}