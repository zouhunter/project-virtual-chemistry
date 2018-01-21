using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using WorldActionSystem;
using HighlightingSystem;
namespace WorldActionSystem
{
    public class SwitchRegister : MonoBehaviour
    {
        [SerializeField]
        private string key;
        [SerializeField]
        private List<GameObject> m_Objs;
        private string resetKey { get { return "HideResetObjects"; } }
        private string hideKey { get { return "HideObjects"; } }
        private string showKey { get { return "UnHideObjects"; } }
        protected EventController eventCtrl { get; set; }

        private bool[] startStates;

        protected void Awake()
        {
            startStates = new bool[m_Objs.Count];
            for (int i = 0; i < startStates.Length; i++)
            {
                startStates[i] = m_Objs[i].activeSelf;
            }
        }
        private void Start()
        {
            eventCtrl.AddDelegate<string>(hideKey, HideGameObjects);
            eventCtrl.AddDelegate<string>(showKey, UnHideGameObjects);
            eventCtrl.AddDelegate<string>(resetKey, ResetGameObjects);
        }

        private void OnDestroy()
        {
            eventCtrl.RemoveDelegate<string>(hideKey, HideGameObjects);
            eventCtrl.RemoveDelegate<string>(showKey, UnHideGameObjects);
            eventCtrl.RemoveDelegate<string>(resetKey, ResetGameObjects);
        }

        public void HideGameObjects(string key)
        {
            if (this.key == key)
            {
                for (int i = 0; i < m_Objs.Count; i++)
                {
                    m_Objs[i].SetActive(false);
                }
            }

        }
        public void UnHideGameObjects(string key)
        {
            if (this.key == key)
            {
                SetElementState(true);
            }
        }
        public void ResetGameObjects(string key)
        {
            if (this.key == key)
            {
                for (int i = 0; i < startStates.Length; i++)
                {
                    m_Objs[i].SetActive(startStates[i]);
                }
            }
        }

        private void SetElementState(bool statu)
        {
            for (int i = 0; i < startStates.Length; i++)
            {
                m_Objs[i].SetActive(statu);
            }
        }

    }
}