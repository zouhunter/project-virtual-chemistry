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
    /// 可操作对象具体行为实现
    /// </summary>
    public class InstallObj : MonoBehaviour
    {
        [Range(1,10)]
        public int animTime;
        
        public bool Installed { get { return target != null; } }

        private InstallPos target;
        private Vector3 pickUpPos;
        private Quaternion pickUpRotation;
        private Vector3 startPos;
        private Quaternion startRotation;
        private UnityAction onInstallOkEvent;

        private Tweener move;
        void Start()
        {
            startPos = transform.position;
            startRotation = transform.rotation;
            move = transform.DOMove(startPos, animTime).SetAutoKill(false).
                OnComplete(() => {
                    if (onInstallOkEvent != null)
                        onInstallOkEvent();
                }).Pause();
            onInstallOkEvent = () => { Debug.Log("install _ ok"); };
        }

        /// <summary>
        /// 动画安装
        /// </summary>
        /// <param name="target"></param>
        public void NormalInstall(InstallPos target)
        {
            if (!Installed)
            {
                transform.rotation = target.transform.rotation;
                move.ChangeValues(transform.position, target.transform.position).Restart();
                this.target = target;
            }
        }
        /// <summary>
        /// 定位安装
        /// </summary>
        /// <param name="target"></param>
        public void QuickInstall(InstallPos target)
        {
            if (!Installed)
            {
                transform.position = target.transform.position;
                transform.rotation = target.transform.rotation;
                this.target = target;
            }
        }

        public void NormalUnInstall()
        {
            if (Installed)
            {
                move.Pause();
                transform.rotation = startRotation;
                move.ChangeValues(transform.position, startPos).Restart();
                target = null;
            }
        }
        public void QuickUnInstall()
        {
            if (Installed)
            {
                move.Pause();
                transform.rotation = startRotation;
                transform.position = startPos;
                target.Detach();
                target = null;
            }
        }

        public void OnPickUp()
        {
            pickUpPos = transform.position;
        }

        public void OnPickDown()
        {
            move.ChangeValues(transform.position, pickUpPos).Restart();
        }
    }

}