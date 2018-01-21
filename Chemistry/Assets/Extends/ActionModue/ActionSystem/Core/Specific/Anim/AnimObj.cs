using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace ChargeSystem
{

    /// <summary>
    /// 动画对象，播放动画，停止动画
    /// </summary>
    public class AnimObj : MonoBehaviour, ActionObj
    {
        public string stapName;
        public string animName;
        public bool endActive;
        public bool startActive;
        

        private Animation anim;
        private AnimationClip clip;
        private AnimationState state;
        private float animTime;
        private AnimationEvent even;
        public IRemoteController RemoteCtrl
        {
            get
            {
                return ActionSystem.Instance.remoteController;
            }
        }

        void Awake()
        {
            anim = GetComponent<Animation>();
            state = anim[animName];
            animTime = state.length;
            Debug.Log("动画时间" + animTime);
            clip = anim.GetClip(animName);
            even = new AnimationEvent();
            even.time = animTime;
            even.functionName = "OnPlayToEnd";
            clip.AddEvent(even);
            gameObject.SetActive(startActive);
        }
        /// <summary>
        /// 播放动画
        /// </summary>
        public void PlayAnim()
        {
            anim.gameObject.SetActive(true);
            anim.Play();
        }

        /// <summary>
        /// 强制完成
        /// </summary>
        public void EndPlay()
        {
            state.normalizedTime = 1f;
            state.normalizedSpeed = 0;
            anim.gameObject.SetActive(endActive);
        }

        /// <summary>
        /// 完成事件
        /// </summary>
        public void OnPlayToEnd()
        {
            RemoteCtrl.EndExecuteCommand();
            anim.gameObject.SetActive(endActive);
        }
        public void UnDoPlay()
        {
            state.normalizedTime = 0f;
            state.normalizedSpeed = 1;
            anim.gameObject.SetActive(startActive);
        }
    }
}