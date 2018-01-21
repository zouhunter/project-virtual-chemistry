using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{

    public class DragCtrl : OperateController
    {
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Drag;
            }
        }

        private DragObj selectedObj;
        private Ray ray;
        private RaycastHit hit;
        private Vector3 previousMousePosition;
        //private Vector3 originalTargetPosition;
        private Vector3 direction;
        private float distence { get { return Config.hitDistence; } }
        private float elementDistence;
        public override void Update()
        {
            if(Input.GetMouseButtonDown(0)) {
                TrySelectObj();
            }
            if (selectedObj != null){
                TransformSelected();
            }
        }
        private bool TrySelectObj()
        {
            if (viewCamera == null) return false;

            ray = viewCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, distence, LayerMask.GetMask( Layers.dragItemLayer)))
            {
                var obj = hit.collider.GetComponentInParent<DragObj>();
                if (obj.Started && !obj.Complete)
                {
                    selectedObj = obj;
                }
                else
                {
                    selectedObj = null;
                }
            }

            return selectedObj != null;
        }



        void TransformSelected()
        {
            if (Input.GetMouseButtonDown(0))
            {
                //originalTargetPosition = selectedObj.transform.position;
                direction = (selectedObj.targetPos - selectedObj.startPos).normalized;
                previousMousePosition = Vector3.zero;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
                {
                    elementDistence = Vector3.Distance(viewCamera.transform.position, selectedObj.transform.position);
                    previousMousePosition.z = mousePosition.z = elementDistence;
                    var dir = viewCamera.ScreenToWorldPoint(mousePosition) - viewCamera.ScreenToWorldPoint(previousMousePosition);
                    selectedObj.TryMove(Vector3.Project(dir, direction));
                }

                previousMousePosition = mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                selectedObj.Clamp();
            }
        }

    }

}