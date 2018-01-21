using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
namespace WorldActionSystem
{
    /// <summary>
    /// 点击状态
    /// </summary>
    public enum ClickType
    {
        nil = 0,
        leftDown = 1,//左键
        centerDown = 1 << 1,
        centerScroll = 1 << 2,
        rightDown = 1 << 3
    }

    public class ViewCamera : MonoBehaviour
    {
        [Header("默认距离")]
        public float distence = 5f;
        private Vector3 cameraTarget;

        [Header("速度")]
        public float mouseWheelSenstitivity = 10;
        public float xSpeed = 20;
        public float ySpeed = 20;
        public float viewChangeSpeed = 1f;
        [Header("限制")]
        public float mouseZoomMin = 0.6f;
        public float mouseZoomMax = 1.2f;
        public float yMinLimit = -0.8f;
        public float yMaxLimit = 0.8f;
        public float viewMin = 2;
        public float viewMax = 30;
        [Header("目标")]
        public float targetDistence = 5f;
        public Vector3 targetRotate;
        public Vector3 targetPosition;
        public float targetView;
        [Header("调控")]
        public bool isNeedDamping = true;
        public float damping = 10f;

        [Header("变化")]
        private float xdlt = 0f;
        private float ydlt = 0f;
        private float ndlt = 0f;

        private ClickType clickType;
        private Vector3 screenPoint;
        private Vector3 offset;
        private Camera lookCamera;

        private float dotweenduration
        {
            get { return 1; }
        }

        void Awake()
        {
            lookCamera = GetComponent<Camera>();
            targetView = lookCamera.fieldOfView;
            SetSelf(transform.position, transform.rotation);
        }

        public void SetSelf(Vector3 position, Quaternion rotation)
        {
            targetDistence = distence;
            targetRotate = rotation.eulerAngles;
            targetPosition = position;
            cameraTarget = rotation * (Vector3.forward * distence) + transform.position;
        }
        public void SetTarget(Vector3 position)
        {
            cameraTarget = position;
            distence = targetDistence = Vector3.Distance(cameraTarget, position);
        }
        public void SetTarget(float distence)
        {
            cameraTarget = transform.forward * distence + transform.position;
            targetDistence = distence;
        }
        void Update()
        {
            ChangeClickType();
            StoreChangeData();
        }
        void LateUpdate()
        {
            ExecuteCameraChange();
            transform.LookAt(cameraTarget);
        }
        /// <summary>
        /// 改变当前的点击状态
        /// </summary>
        private void ChangeClickType()
        {
            clickType = ClickType.nil;
            if (Input.GetMouseButton(0))
            {
                clickType |= ClickType.leftDown;
            }
            if (Input.GetMouseButton(1))
            {
                clickType |= ClickType.rightDown;
            }
            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                clickType |= ClickType.centerScroll;
            }
            if (Input.GetMouseButton(2))
            {
                clickType |= ClickType.centerDown;
            }
        }
        /// <summary>
        /// 存储要改变的信息
        /// </summary>
        private void StoreChangeData()
        {
            xdlt = ydlt = ndlt = 0f;

            //单击左键(记录xy方向的变化量)
            if ((clickType & ClickType.leftDown) == ClickType.leftDown)
            {
                //xdlt = Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                //ydlt = -Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            }
            //单击右键（加速）
            if (clickType == ClickType.rightDown)
            {
                xdlt += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                ydlt += -Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            }
            //滑动中键（记录距离）
            if ((clickType & ClickType.centerScroll) == ClickType.centerScroll)
            {
                ndlt = Input.GetAxis("Mouse ScrollWheel") * mouseWheelSenstitivity * 0.02f;
            }
            //右键加中键（改变视角大小）
            if (((clickType & ClickType.centerScroll) == ClickType.centerScroll) && (clickType & ClickType.rightDown) == ClickType.rightDown)
            {
                targetView += ndlt * mouseWheelSenstitivity * viewChangeSpeed * 0.02f;
                targetView = Mathf.Clamp(targetView, viewMin, viewMax);
                return;
            }
            //目标旋转量和坐标
            if (targetDistence >= mouseZoomMin * distence && targetDistence <= mouseZoomMax *distence)
            {
                targetDistence -= ndlt * mouseWheelSenstitivity;
            }
            targetDistence = Mathf.Clamp(targetDistence, mouseZoomMin *distence, mouseZoomMax *distence);

            targetRotate = targetRotate + new Vector3(ydlt, xdlt, 0);
            targetPosition = Quaternion.Euler(targetRotate) * new Vector3(0f, 0f, -targetDistence) + cameraTarget;
            targetPosition = ClampPositon(targetPosition, cameraTarget.y + yMinLimit * targetDistence, cameraTarget.y + yMaxLimit * targetDistence);
        }
        /// <summary>
        /// 利用得到的变量设置相机的新
        /// </summary>
        private void ExecuteCameraChange()
        {
            if (isNeedDamping)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotate), Time.deltaTime * damping);
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * damping);
                lookCamera.fieldOfView = Mathf.Lerp(lookCamera.fieldOfView, targetView, Time.deltaTime * damping);
            }
            else
            {
                transform.rotation = Quaternion.Euler(targetRotate);
                transform.position = targetPosition;
                lookCamera.fieldOfView = targetView;
            }
        }
        /// <summary>
        /// 限定角度的范围
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private Vector3 ClampPositon(Vector3 pos, float min, float max)
        {
            pos.y = Mathf.Clamp(pos.y, min, max);
            return pos;
        }

    }

}