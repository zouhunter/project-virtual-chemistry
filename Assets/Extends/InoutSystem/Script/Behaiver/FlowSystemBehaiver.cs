using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace FlowSystem
{
    /// <summary>
    /// 负责实验的初始化，加载元素，重置元素
    /// </summary>
    public class FlowSystemBehaiver : MonoBehaviour
    {
        public ExperimentDataObject experimentData;

        public Button startBtn;
        public Button interactBtn;

        FlowSystemCtrl _systemCtrl;


        public ElementGroup groupParent;
        void Start()
        {
            _systemCtrl = new FlowSystemCtrl(groupParent);
            _systemCtrl.InitExperiment(experimentData);

            startBtn.onClick.AddListener(RestartExperiment);
            interactBtn.onClick.AddListener(StartExperiment);

        }

        void RestartExperiment()
        {
            interactBtn.interactable = true;
            _systemCtrl.ReStart();
        }
        void StartExperiment()
        {
            _systemCtrl.StartProducer();
            interactBtn.interactable = false;

            if (!_systemCtrl.NextContainer(StartExperiment))
            {
                PresentationData data = PresentationData.Allocate("警告", "实验结束", "实验成功与否？");
                EventFacade.Instance.SendNotification(AppConfig.EventKey.OPEN_PRESENTATION, data);
            }
        }
    }
}