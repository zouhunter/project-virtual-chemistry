using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
namespace WorldActionSystem
{

    public abstract class ActionCommand
    {
        public event UnityAction<bool> executeAction;
        public event UnityAction endExecuteAction;
        public string StapName { get; set; }
        public ActionCommand(string stapName)
        {
            this.StapName = stapName;
        }
        public virtual void StartExecute()
        {
            if (executeAction != null)
            {
                executeAction(true);
            }
        }
        public virtual void EndExecute()
        {
            if (endExecuteAction != null)
            {
                endExecuteAction();
            }
        }

        public virtual void UnDoCommand()
        {
            if (executeAction != null)
            {
                executeAction(false);
            }
        }
    }


}