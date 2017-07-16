using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    /// <summary>
    /// 注册所有动画命令
    /// </summary>
    public class AnimationObjectsHolder : ActionHolder
    {

        List<AnimObj> animObjects = new List<AnimObj>();
        void Start()
        {
            foreach (Transform item in transform)
            {
                AnimObj anim = item.GetComponent<AnimObj>();
                animObjects.Add(anim);
            }
            RegisterActionCommand();
        }

        protected override void RegisterActionCommand()
        {
            ActionCommand cmd;
            foreach (var item in animObjects)
            {
                cmd = new AnimCommand(item.stapName, item, true);
                ActionSystem.Instance.AddActionCommand(cmd);
            }
        }
    }

}