using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace WorldActionSystem
{
    public class ChargeCtrl : OperateController
    {
        private PickUpController pickCtrl;
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Charge;
            }
        }
        private ChargeTool chargeTool { get { return pickCtrl.pickedUpObj != null && pickCtrl.pickedUpObj is ChargeTool ? pickCtrl.pickedUpObj as ChargeTool : null; } }
        private ChargeObj chargeObj;
        private ChargeObj lastMatchChargeObj;
        private ChargeResource chargeResource;
        private ChargeResource lastMatchChargeResource;
        IHighLightItems highter;
        public ChargeCtrl()
        {
            highter = new ShaderHighLight();
            pickCtrl = ActionSystem.Instence.pickupCtrl;
            pickCtrl.onPickStay += OnPickStay;
        }
        public override void Update()
        {
            if (chargeTool != null)
            {
                if (chargeTool.charged)
                {
                    if (ChargeUtil.FindChargeObj(chargeTool, out chargeObj))
                    {
                        if(chargeObj != lastMatchChargeObj)
                        {
                            if (lastMatchChargeObj != null)
                            {
                                highter.UnHighLightTarget(lastMatchChargeObj.gameObject);
                            }
                            highter.HighLightTarget(chargeObj.gameObject, Color.green);
                            lastMatchChargeObj = chargeObj;
                        }
                    }
                    else
                    {
                        if (lastMatchChargeObj != null)
                        {
                            highter.UnHighLightTarget(lastMatchChargeObj.gameObject);
                            lastMatchChargeObj = null;
                        }
                    }
                }
                else
                {
                    if (ChargeUtil.FindResource(chargeTool, out chargeResource))
                    {
                        if (chargeResource != lastMatchChargeResource)
                        {
                            if (lastMatchChargeResource != null)
                            {
                                highter.UnHighLightTarget(lastMatchChargeResource.gameObject);
                            }
                            highter.HighLightTarget(chargeResource.gameObject, Color.green);
                            lastMatchChargeResource = chargeResource;
                        }
                    }
                    else
                    {
                        if(lastMatchChargeResource != null)
                        {
                            highter.UnHighLightTarget(lastMatchChargeResource.gameObject);
                            lastMatchChargeResource = null;
                        }
                    }
                }
            }
        }

        private void OnPickStay(PickUpAbleItem item)
        {
            if (item is ChargeTool)
            {
                var currTool = chargeTool;
                if (chargeResource != null)
                {
                    var value = Mathf.Min(currTool.capacity, chargeResource.current);
                    var type = chargeResource.type;
                    currTool.PickUpAble = false;
                    currTool.LoadData(chargeResource.transform.position,new ChargeData(type, value),()=> {
                        currTool.PickUpAble = true;
                    });
                    chargeResource.Subtruct(value,()=> { });

                    highter.UnHighLightTarget(chargeResource.gameObject);
                    lastMatchChargeResource = chargeResource = null;
                }
                else if (chargeObj != null)
                {
                    var data = currTool.data;
                    ChargeData worpData = chargeObj.JudgeLeft(data);
                    if(!string.IsNullOrEmpty(worpData.type)){
                        currTool.PickUpAble = false;
                        currTool.OnCharge(chargeObj.transform.position, worpData.value,()=> { currTool.PickUpAble = true; });
                        chargeObj.Charge(worpData, () => { chargeObj.JudgeComplete(); });
                    }
                    highter.UnHighLightTarget(chargeObj.gameObject);
                    lastMatchChargeObj = null;
                }
            }

        }
    }

}
