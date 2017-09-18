using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace Connector
{
    public class PickUpController : IPickUpController
    {
        private IPickUpAble pickedUpObj;
        private Ray ray;
        private RaycastHit hit;
        private RaycastHit shortHit;
        private float _Timer;
        private float scrollSpeed;
        private float distence;
        private float timeSpan;

        public event UnityAction<GameObject> onPickUp;
        public event UnityAction<GameObject> onPickDown;
        public event UnityAction<GameObject> onPickStatu;

        public PickUpController(float timeSpan, float distence, float scrollSpeed)
        {
            this.scrollSpeed = scrollSpeed;
            this.distence = distence;
            this.timeSpan = timeSpan;
        }

        public IPickUpAble PickedUpObj
        {
            get
            {
                return pickedUpObj;
            }
        }

        public void Update()
        {
            if (pickedUpObj !=null)
            {
                UpdatePickUpdObject();

                if (Input.GetMouseButtonDown(0))
                {
                    OnPickupedObjectClicked();
                }
                else if (Input.GetMouseButtonDown(2))
                {
                    PickDownPickedUpObject();
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                TryPickUpObject();
            }

        }
        public bool PickDownPickedUpObject()
        {
            if (onPickDown != null) onPickDown.Invoke(pickedUpObj.Go);
            pickedUpObj.OnPickDown();
            pickedUpObj = null;
            return true;
        }

        private bool PickUpObject(IPickUpAble pickedUpObj)
        {
            if (onPickUp != null) onPickUp.Invoke(pickedUpObj.Go);
            this.pickedUpObj = pickedUpObj;
            pickedUpObj.OnPickUp();
            return true;
        }

        public bool TryPickUpObject()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1<<LayerConst.elementLayer))
            {
                pickedUpObj = hit.collider.GetComponent<IPickUpAble>();
                if (pickedUpObj != null)
                {
                    PickUpObject(pickedUpObj);
                    return true;
                }
            }
            return false;
        }

        public bool OnPickupedObjectClicked()
        {
            if (onPickStatu != null) onPickStatu.Invoke(pickedUpObj.Go);
            pickedUpObj.OnPickStay();
            pickedUpObj = null;
            return true;
        }

        public void UpdatePickUpdObject()
        {
            _Timer += Time.deltaTime;
            if (_Timer> timeSpan)
            {
                _Timer = 0f;
                if (pickedUpObj != null)
                {
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray,out shortHit,distence, 1 << LayerConst.putLayer))
                    {
                        pickedUpObj.Position = shortHit.point;
                    }
                    else
                    {
                        pickedUpObj.Position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,distence));
                    }
                }
            }
            pickedUpObj.Rotation = pickedUpObj.Rotation * Quaternion.Euler(Vector3.up * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed);
        }
        
    }
}