using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem.Binding
{
    public abstract class ActionObjEventSender : ActionObjBinding
    {
        private ActionGroup _system;
        private ActionGroup system { get { transform.SurchSystem(ref _system); return _system; } }
        protected EventController eventCtrl { get { return system.EventCtrl; } }

        [SerializeField]
        protected string key;
    }
}