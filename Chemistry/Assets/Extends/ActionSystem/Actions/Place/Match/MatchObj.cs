using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    [AddComponentMenu(MenuName.MatchObj)]
    public class MatchObj : PlaceObj
    {
        public bool completeMoveBack = true;//结束时退回
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Match;
            }
        }

        public bool Matched { get { return obj != null; } }
        protected override void OnBeforeEnd(bool force)
        {
            base.OnBeforeEnd(force);

            if (Matched && completeMoveBack){
                obj.QuickUnInstall();
            }
        }
        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();

            if (Matched){
                var obj = Detach();
                obj.QuickUnInstall();
            }
        }
        protected override void OnAutoInstall()
        {
            var obj = GetUnInstalledObj(Name);
            Attach(obj);
            if (Config.quickMoveElement && !ignorePass)
            {
                if (!completeMoveBack)
                {
                    obj.QuickInstall(this,false,false);
                }
                else
                {
                    OnInstallComplete();
                }
            }
            else
            {
                obj.NormalInstall(this, false,false);
            }
        }
        /// <summary>
        /// 找出一个没有安装的元素
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public PickUpAbleElement GetUnInstalledObj(string elementName)
        {
            var elements = elementCtrl.GetElements<PickUpAbleElement>(elementName);
            if (elements != null)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (!elements[i].HaveBinding)
                    {
                        return elements[i];
                    }
                }
            }
            throw new Exception("配制错误,缺少" + elementName);
        }

        protected override void OnInstallComplete()
        {
            if (!Complete)
            {
                OnEndExecute(false);
            }
        }
    }
}