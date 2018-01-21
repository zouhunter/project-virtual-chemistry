using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public class AnimChargeCtrl
    {
        private Transform parent;
        private Animation anim;
        private AnimationState state;
        public bool Started { get; private set; }
        public AnimChargeCtrl(Transform parent, Animation anim)
        {
            this.anim = anim;
            this.parent = parent;
        }

        public void PlayAnim(string animName,Vector3 position,float time)
        {
            state = anim[animName];
            state.speed = state.length / time;
            parent.transform.position = position;
            anim.Play(animName);
            Started = true;
        }
        public void StopAnim()
        {
            anim.Stop();
            state.normalizedTime = 0;
            var currentPos = anim.transform.position;
            anim.transform.localPosition = Vector3.zero;
            parent.transform.position = currentPos;
            Started = false;
        }
    }
}