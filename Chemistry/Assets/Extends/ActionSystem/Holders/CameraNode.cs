using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public class CameraNode : MonoBehaviour
    {
        //[SerializeField,Range(1,10)]
        private float _speed = 5;//缓慢移入
        //[SerializeField]
        private float _field = 60;
        [SerializeField]
        private Transform _target;
        private float _distence = 1;
        private Quaternion _rotate;
        public string ID { get { return name; } }
        public float Speed { get { return _speed; } }
        public float Distence { get { return _distence; } }
        public float CameraField { get { return _field; } }
        public Quaternion Rotation { get { return _rotate; } }
        public bool MoveAble { get { return _target != null; } }
        private ActionGroup _system;
        private ActionGroup system { get {  transform.SurchSystem(ref _system);return _system; } }
        protected CameraController cameraCtrl { get { return  ActionSystem.Instence.cameraCtrl; } }

        private void Awake()
        {
            if (_target == null && transform.childCount > 0)
            {
                _target = transform.GetChild(0);
            }

            if(_target != null)
            {
                _distence = Vector3.Distance(transform.position, _target.position);
                _rotate = Quaternion.LookRotation(_target.position - transform.position);
            }
            else
            {
                _rotate = transform.rotation;
            }

        }
        private void Start()
        {
            if(cameraCtrl != null)
            {
                cameraCtrl.RegistNode(this);
            }
        }
    }

}