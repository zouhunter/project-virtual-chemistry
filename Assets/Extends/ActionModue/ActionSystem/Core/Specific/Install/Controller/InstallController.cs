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
    /// 提供拿起安装和快速安装等功能
    /// </summary>
    public class InstallController : IInstallCtrl
    {
        IInstallStart startParent;
        IInstallEnd endParent;

        private InstallObj pickedUpObj;
        private bool pickedUp;
        private int distence { get { return startParent.Distence; } }
        private InstallPos installPos;

        private int elementLayer;
        private int elementInstallLayer;
        private Ray ray;
        private RaycastHit hit;
        private RaycastHit[] hits;

        public InstallController(IInstallStart startParent, IInstallEnd endParent)
        {
            this.startParent = startParent;
            this.endParent = endParent;

            elementLayer = (int)Mathf.Pow(2, 8);
            elementInstallLayer = (int)Mathf.Pow(2, 9);
        }

        #region 鼠标操作事件
        public void Reflesh()
        {
            if (Input.GetMouseButtonDown(0) &&(EventSystem.current == null ||( EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())))
            {
                OnLeftMouseClicked();
            }
            //else if (Input.GetMouseButtonDown(1)&& EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            //{
            //    OnRightMosueClicked();
            //}
            else
            {
                UpdateEveryFrame();
            }
        }

        public void OnLeftMouseClicked()
        {
            if (!pickedUp)
            {
                SelectAnElement();
            }
            else
            {
                TryInstallObject();
            }
        }

        public void OnRightMosueClicked()
        {
            if (pickedUp)
            {
                pickedUp = false;
                startParent.PickDownPickedUpObject();
            }
        }

        public void UpdateEveryFrame()
        {
            if (pickedUp)
            {
                MoveWithMouse();
            }
        }


        /// <summary>
        /// 在未屏幕锁的情况下选中一个没有元素
        /// </summary>
        void SelectAnElement()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, elementLayer))
            {
                pickedUpObj = hit.collider.GetComponent<InstallObj>();
                if (pickedUpObj != null && startParent.PickUpObject(pickedUpObj))
                {
                    pickedUp = true;
                }
            }
        }

        /// <summary>
        /// 尝试安装元素
        /// </summary>
        void TryInstallObject()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, elementInstallLayer))
            {
                installPos = hit.collider.GetComponent<InstallPos>();
                if (installPos != null)
                {
                    //判断当前坐标可否安装
                    if (endParent.CanPosInstall(installPos))
                    {
                        if (startParent.CanInstallToPos(installPos))
                        {
                            startParent.InstallPickedUpObject(installPos);
                            pickedUp = false;
                        }
                    }
                }
            }
            else
            {
                startParent.PickDownPickedUpObject();
                pickedUp = false;
            }
        }

        /// <summary>
        /// 跟随鼠标
        /// </summary>
        void MoveWithMouse()
        {
            pickedUpObj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distence));
        }
        #endregion

        /// <summary>
        /// 结束当前步骤安装
        /// </summary>
        /// <param name="stapName"></param>
        public void EndInstall(string stapName)
        {
            SetStapActive(stapName);
            List<InstallPos> posList = endParent.GetNotInstalledPosList();
            startParent.InstallPosListObjects(posList);
        }

        public bool CurrStapComplete()
        {
            return endParent.AllElementInstalled();
        }

        public void SetStapActive(string stapName)
        {
            endParent.SetStapActive(stapName);
        }

        /// <summary>
        /// 自动安装部分需要进行自动安装的零件
        /// </summary>
        /// <param name="stapName"></param>
        public void AutoInstallWhenNeed(string stapName)
        {
            List<InstallPos> posList = endParent.GetNeedAutoInstallPosList();
            startParent.InstallPosListObjects(posList);
        }

        public void UnInstall(string stapName)
        {
            SetStapActive(stapName);
            List<InstallPos> posList = endParent.GetInstalledPosList();
            startParent.UnInstallPosListObjects(posList);
        }

        public void QuickUnInstall(string stapName)
        {
            SetStapActive(stapName);
            List<InstallPos> posList = endParent.GetInstalledPosList();
            startParent.QuickUnInstallPosListObjects(posList);
        }
    }

}