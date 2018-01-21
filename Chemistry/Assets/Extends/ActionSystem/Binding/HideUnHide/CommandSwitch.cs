using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using WorldActionSystem;

namespace WorldActionSystem.Binding
{
    /// <summary>
    /// 在一个ActionCommand时间内触发状态改变
    /// (隐藏变显示,显示变隐藏)
    /// </summary>
    public class CommandSwitch : ActionCommandBinding
    {
        public bool activeOnComplete;
        public bool activeOnStart;
        public GameObject[] objs;

        private bool[] startStates;
        protected override void Awake()
        {
            base.Awake();
            startStates = new bool[objs.Length];
            for (int i = 0; i < startStates.Length; i++)
            {
                startStates[i] = objs[i].activeSelf;
            }
        }
        protected override void OnBeforeActive(string step)
        {
            SetElementState(activeOnStart);
        }
        protected override void OnBeforePlayEnd(string step)
        {
            SetElementState(activeOnComplete);
        }
        protected override void OnBeforeUnDo(string step)
        {
            for (int i = 0; i < startStates.Length; i++)
            {
                objs[i].SetActive(startStates[i]);
            }
        }

        private void SetElementState(bool statu)
        {
            for (int i = 0; i < startStates.Length; i++)
            {
                objs[i].SetActive(statu);
            }
        }

    }
}
