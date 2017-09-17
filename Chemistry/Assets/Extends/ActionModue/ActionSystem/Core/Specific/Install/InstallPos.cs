using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace WorldActionSystem
{
    /// <summary>
    /// 模拟安装坐标功能
    /// </summary>
    public class InstallPos :MonoBehaviour, ActionObj
    {
        public string stapName;
        public bool autoInstall;

        public bool Installed { get { return obj != null; } }
        public InstallObj obj { get; private set; }

        public Renderer render;
        public IInstallCtrl installCtrl { private get; set; }

        public IRemoteController RemoteCtrl
        {
            get
            {
                return ActionSystem.Instance.remoteController;
            }
        }

        void Start()
        {
           if(render == null) render = GetComponent<Renderer>();
        }

        public void Attach(InstallObj obj)
        {
            this.obj = obj;
            if (installCtrl.CurrStapComplete())
            {
                RemoteCtrl.EndExecuteCommand();
            }
        }

        public InstallObj Detach()
        {
            InstallObj old = obj;
            obj = null;
            return old;
        }

        void OnMouseEnter()
        {
            if (!Installed)
            {
                render.enabled = true;
                CancelInvoke("HideRender");
            }
        }

        void OnMouseExit()
        {
            //render.enabled = false;
            Invoke("HideRender", 1);
        }

        void HideRender()
        {
            if (render != null)
            {
                render.enabled = false;
            }
        }
    }

}