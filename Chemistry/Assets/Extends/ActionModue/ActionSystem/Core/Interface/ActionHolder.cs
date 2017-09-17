using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
		
public abstract class ActionHolder : MonoBehaviour
{
    /// <summary>
    /// 注册安装命令
    /// </summary>
    protected abstract void RegisterActionCommand();
}

	}