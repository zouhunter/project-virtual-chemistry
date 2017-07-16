using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
//using DG.Tweening;
using UnityStandardAssets.Characters.FirstPerson;
public class LabPlayer : MonoBehaviour
{
    //public Tweener move;
    public Transform Camera;
    private Camera m_Camera;
    private Transform cameraTemp;

    public Rigidbody controller;
    public float damp = 2f;
    private float cameraSpeed
    {
        get { return 200;/* Laboratory.Main!=null ? Laboratory.Main.settingData.playerLab.cameraMoveSpeed:1; */}
    }

    private float minView = 11;
    private float maxView = 40;
    //private float wasdSpeed
    //{
    //    get { return Laboratory.Main != null ? Laboratory.Main.settingData.playerLab.playerMoveSpeed : 1; }
    //}
    private Transform target;
    private bool isPlaying;

    private RigidbodyFirstPersonController.MovementSettings movementSettings;
    public RigidbodyFirstPersonController.MovementSettings MovementSettings {

        get {
            if(movementSettings == null)
                movementSettings = GetComponent<RigidbodyFirstPersonController>().movementSettings;
            return movementSettings;
        } }

    private MouseLook mouselook;
    public MouseLook MouseLook
    {
        get {
            if (mouselook == null) mouselook = GetComponent<RigidbodyFirstPersonController>().mouseLook;
            return mouselook;
        }
    }
    void OnEnable()
    {
        //move = transform.DOMove(transform.position, distence / cameraSpeed).OnComplete(() =>
        //{
        //    isPlaying = false;/* controller.isKinematic = false;*/
        //}
        //).SetAutoKill(false).Pause();
        m_Camera = Camera.GetComponent<Camera>();
    }

    void Start()
    {
        //if (Laboratory.Main)
        //{
        //    //动态修改人物速度
        //    Laboratory.Main.playerSpeedChanged += ChangePlayerSpeed;
        //}
        cameraTemp = new GameObject("temp").transform;
        cameraTemp.SetParent(transform);
    }
    void OnDestroy()
    {
        //if(Laboratory.Main) Laboratory.Main.playerSpeedChanged -= ChangePlayerSpeed;
    }
    void SetPlayerHight(float value)
    {
        GetComponent<CapsuleCollider>().height = value * 2;
        transform.position.Set(transform.position.x, value, transform.position.z);
    }
    /// <summary>
    /// 改变坐标
    /// </summary>
    public void ChangePos(Transform pos)
    {
        //distence = Vector3.Distance(pos.position, transform.position);
        //move.ChangeValues(transform.position, pos.position, distence / cameraSpeed).Restart();
        isPlaying = true;
        //controller.isKinematic = true;
    }
    /// <summary>
    /// 改变朝向
    /// </summary>
    public void ChangeTarget(Transform targetPos)
    {
        target = targetPos;
        Cursor.visible = false;
    }
    /// <summary>
    /// 改变速度
    /// </summary>
    /// <param name="speed"></param>
    public void ChangePlayerSpeed(float speed)
    {
        MovementSettings.ForwardSpeed = speed;
        MovementSettings.BackwardSpeed = speed;
        MovementSettings.StrafeSpeed = speed;
    }
    float scrollWeel;
    void LateUpdate()
    {
        if (isPlaying)
        {
            Camera.LookAt(target);
            MouseLook.Init(transform,Camera);
        }
        scrollWeel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWeel != 0)
        {
            m_Camera.fieldOfView -= scrollWeel * Time.deltaTime * cameraSpeed;
            if (m_Camera.fieldOfView < minView)
            {
                m_Camera.fieldOfView = minView;
            }
            else if(m_Camera.fieldOfView > maxView){
                m_Camera.fieldOfView = maxView;
            }
        }
    }
}
