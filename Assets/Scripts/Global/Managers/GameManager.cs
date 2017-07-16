using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// 整个程序的入口，并且做程序全局管理
/// </summary>
public partial class GameManager : MonoBehaviour
{
    #region 多线程单例
    private static GameManager m_Instance;
    protected static readonly object m_staticSyncRoot = new object();
    private GameManager() { }
    public static GameManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                lock (m_staticSyncRoot)
                {
                    if (m_Instance == null && !isQuit)
                    {
                        GameObject go = new GameObject("GameManager");
                        m_Instance = go.AddComponent<GameManager>();
                    }
                }
            }
            return m_Instance;
        }
    }

    #endregion

    public static bool isQuit;
    public static bool isOn = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);  //防止销毁自己
        Application.targetFrameRate = 90;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            CloseEvent();
        }
        if (fallowEvent != null && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            FallowEvent();
        }
        hitCtrl.Update();
    }
    /// <summary>
    /// 是否退出
    /// </summary>
    void OnApplicationQuit()
    {
        isQuit = true;
    }
    /// <summary>
    /// 启动框架
    /// </summary>
    public static void LunchFrameWork()
    {
        if (!isOn)
        {
            AudioManager.Instance.ResetDefult(true, 0.3f);
            //Facade.Instance.RegisterCommand<SceneLoadCommond>("loadScene");
            isOn = true;
        }
        else
        {
            fallowEvent = null;
            closeEvent = null;
        }
    }
}
/// <summary>
/// 全局事件
/// </summary>
public partial class GameManager
{
    public static event UnityAction closeEvent;
    public static void CloseEvent()
    {
        if (closeEvent != null)
        {
            closeEvent();
            closeEvent = null;
        }
    }

    public static float distence = 1f;
    public static UnityAction<Vector3> fallowEvent;
    public static void FallowEvent()
    {
        if (fallowEvent != null)
        {
            fallowEvent(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distence)));
        }
    }

    public static event UnityAction<string> onSceneLoad;
    public static void OnLoadScene(string sceneName)
    {
        if (onSceneLoad != null)
        {
            onSceneLoad(sceneName);
            closeEvent = null;
            fallowEvent = null;
        }
    }
}
/// <summary>
/// 控制器集合
/// </summary>
public partial class GameManager
{
    public static BuildingCtrl buildCtrl = new BuildingCtrl();
    public static RecordCtrl recordCtrl = new RecordCtrl();
    public static HitController hitCtrl = new global::HitController();

}