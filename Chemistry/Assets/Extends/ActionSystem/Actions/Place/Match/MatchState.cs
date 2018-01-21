using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public class MatchState : IPlaceState
    {
        public ControllerType CtrlType { get { return ControllerType.Match; } }

        private PlaceCtrl placeCtrl;
        public MatchState(PlaceCtrl placeCtrl)
        {
            this.placeCtrl = placeCtrl;
        }
        public bool CanPlace(PlaceObj matchPos, PickUpAbleItem element, out string why)
        {
            var matchAble = true;
            if (matchPos == null)
            {
                why = "【配制错误】:零件未挂MatchObj脚本";
                Debug.LogError("【配制错误】:零件未挂MatchObj脚本");
                matchAble = false;
            }
            else if (!matchPos.Started)
            {
                matchAble = false;
                why = "操作顺序错误";
            }
            else if (matchPos.AlreadyPlaced)
            {
                matchAble = false;
                why = "已经触发结束";
            }
            else if (matchPos.Name != element.Name)
            {
                matchAble = false;
                why = "零件不匹配";
            }
            else
            {
                why = null;
                matchAble = true;
            }
            return matchAble;
        }

        public void PlaceObject(PlaceObj pos, PickUpAbleElement pickup)
        {
            pos.Attach(placeCtrl.pickedUpObj);
            placeCtrl.pickedUpObj.QuickInstall(pos, false, false);
        }

        public void PlaceWrong(PickUpAbleElement pickup)
        {
            if (pickup)
            {
                pickup.OnPickDown();
            }
        }
    }

}