using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    /// <summary>
    /// 注册所有安装命令
    /// </summary>
    public class InstallObjectsHolder : ActionHolder
    {
        [SerializeField]
        private IInstallStart startParent;
        [SerializeField]
        private IInstallEnd endParent;

        IInstallCtrl intallController;

        // Use this for initialization
        void Start()
        {
            startParent = GetComponentInChildren<IInstallStart>();
            endParent = GetComponentInChildren<IInstallEnd>();
            intallController = new InstallController(startParent, endParent);
            RegisterActionCommand();
        }

        protected override void RegisterActionCommand()
        {
            endParent.GetInstallDicAsync(OnAllInstallPosInit);
        }

        void OnAllInstallPosInit(Dictionary<string, List<InstallPos>> dic)
        {
            ActionCommand cmd;
            foreach (var item in dic)
            {
                cmd = new InstallCommand(item.Key, intallController, item.Value);
                ActionSystem.Instance.AddActionCommand(cmd);
            }
        }

        void Update()
        {
            intallController.Reflesh();
        }

    }

}