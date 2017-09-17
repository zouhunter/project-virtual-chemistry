using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace FlowSystem
{

    public class PickUpController : IPickUpController
    {
        private bool pickUped;
        private IPickUpAble pickedUpObj;
        private Ray ray;
        private RaycastHit hit;
        private RaycastHit shortHit;
        private TimeSpanInfo _Timer;
        private float scrollSpeed;
        private float distence;
        public PickUpController(float timeSpan, float distence, float scrollSpeed)
        {
            this.scrollSpeed = scrollSpeed;
            this.distence = distence;
            _Timer = new TimeSpanInfo(timeSpan);
        }
        public bool PickUped { get { return pickUped; } }

        public bool PickDownPickedUpObject()
        {
            pickedUpObj.OnPickDown();
            pickedUpObj = null;
            pickUped = false;
            return true;
        }

        private bool PickUpObject(IPickUpAble pickedUpObj)
        {
            this.pickedUpObj = pickedUpObj;
            pickedUpObj.OnPickUp();
            pickUped = true;
            return true;
        }

        public bool TryPickUpObject(out IPickUpAble obj)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, LayerMask.GetMask(LayerConst.itemLayer)))
            {
                pickedUpObj = hit.collider.GetComponent<IPickUpAble>();
                if (pickedUpObj != null)
                {
                    PickUpObject(pickedUpObj);
                    obj = pickedUpObj;
                    return true;
                }
            }
            obj = null;
            return false;
        }

        public bool TryStayPickUpedObject()
        {
            pickedUpObj.OnPickStay();
            pickedUpObj = null;
            pickUped = false;
            return true;
        }

        public void UpdatePickUpdObject()
        {
            if (_Timer.OnSpanComplete())
            {
                if (pickedUpObj != null)
                {
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray,out shortHit,distence, LayerMask.GetMask(LayerConst.putLayer)))
                    {
                        pickedUpObj.Trans.position = shortHit.point;
                    }
                    else
                    {
                        pickedUpObj.Trans.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,distence));
                    }
                }
            }
            pickedUpObj.Trans.rotation = pickedUpObj.Trans.rotation * Quaternion.Euler(Vector3.up * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed);
        }
    }
}