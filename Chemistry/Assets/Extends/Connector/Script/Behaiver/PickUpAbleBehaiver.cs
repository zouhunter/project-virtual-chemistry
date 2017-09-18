using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Connector
{
    /// <summary>
    /// 可操作对象具体行为实现
    /// </summary>
    public class PickUpAbleBehaiver : MonoBehaviour,IPickUpAble
    {
        public UnityEvent onPickUp;
        public UnityEvent onPickDown;
        public UnityEvent onPickStay;

        private Vector3 pickUpPos;
        private Quaternion pickUpRotation;

        public GameObject Go
        {
            get
            {
                return gameObject;
            }

        }

        public Vector3 Position
        {
            get
            {
                return transform.position;
            }

            set
            {
                transform.position = value;

            }
        }

        public Quaternion Rotation
        {
            get
            {
                return transform.rotation;
            }

            set
            {
                transform.rotation = value;
            }
        }

        public void OnPickUp()
        {
            pickUpPos = transform.position;
            pickUpRotation = transform.rotation;
            onPickUp.Invoke();
        }

        public void OnPickStay()
        {
            onPickStay.Invoke();
        }

        public void OnPickDown()
        {
            transform.position = pickUpPos;
            transform.rotation = pickUpRotation;
            onPickDown.Invoke();
        }
    }
}