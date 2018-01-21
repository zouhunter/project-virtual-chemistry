using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem.Binding
{
    public class ActionObjEventRegister : MonoBehaviour
    {
        [SerializeField]
        protected string key;
        private ActionGroup _group;
        private ActionGroup group { get { transform.SurchSystem(ref _group); return _group; } }
        protected EventController eventCtrl { get { return group.EventCtrl; } }
    }
}