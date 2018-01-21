using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{

    public class SequencesCommand : IActionCommand
    {
        private int index;
        public string StepName { get; private set; }
        public bool Startd
        {
            get
            {
                return started;
            }
        }
        public bool Completed
        {
            get
            {
                return completed;
            }
        }

        public ActionObjCtroller ActionObjCtrl
        {
            get
            {
                if (currentCmd == null) return null;
                return currentCmd.ActionObjCtrl;
            }
        }

        private IList<IActionCommand> commandList;
        private bool forceAuto;
        private bool started;
        private bool completed;
        private IActionCommand currentCmd;
        private bool log = false;

        public SequencesCommand(string stepName, IList<IActionCommand> commandList)
        {
            StepName = stepName;
            this.commandList = commandList;
            currentCmd = commandList[0];
        }

        public bool StartExecute(bool forceAuto)
        {
            if(!started)
            {
                started = true;
                index = 0;
                this.forceAuto = forceAuto;
                ContinueExecute();
                return true;
            }
            else
            {
                Debug.Log("already started" + StepName);
                return false;
            }
        }


        public bool EndExecute()
        {
            if(!completed)
            {
                completed = true;
                foreach (var item in commandList)
                {
                    if(!item.Startd){
                        item.StartExecute(forceAuto);
                    }
                    if(!item.Completed){
                        item.EndExecute();
                    }
                }
                OnEndExecute();
                return true;
            }
           else
            {
                Debug.Log("already complete" + StepName);
                return false;
            }
        }
      
        public void UnDoExecute()
        {
            started = false;
            completed = false;
            index = 0;
            for (int i = commandList.Count - 1; i >= 0; i--)
            {
                var item = commandList[i];
                if (item.Startd)
                {
                    item.UnDoExecute();
                }
            }
        }

        internal bool ContinueExecute()
        {
            if (index < commandList.Count)
            {
                if(log) Debug.Log("Execute:" + index + "->continue StartExecute");
                currentCmd = commandList[index++];
                currentCmd.StartExecute(forceAuto);
                return true;
            }
            if (log) Debug.Log("Execute:" + index + "->EndExecute");
            return false;
        }

        public void OnEndExecute()
        {
            index = commandList.Count - 1;
        }
    }

}