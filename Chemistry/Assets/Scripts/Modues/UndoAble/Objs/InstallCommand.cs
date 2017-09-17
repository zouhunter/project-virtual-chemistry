using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
namespace WorldActionSystem
{
    [Serializable]
    public class InstallCommand : ActionCommand
    {
        public IList<InstallPos> installers;
        public IInstallCtrl installCtrl;
        public InstallCommand(string stapName, IInstallCtrl installCtrl, IList<InstallPos> installers) : base(stapName)
        {
            this.installers = installers;
            this.installCtrl = installCtrl;
            this.StapName = stapName;
            for (int i = 0; i < installers.Count; i++)
            {
                installers[i].installCtrl = installCtrl;
            }
        }
        public override void StartExecute()
        {
            installCtrl.SetStapActive(StapName);
            installCtrl.AutoInstallWhenNeed(StapName);
            base.StartExecute();
        }
        public override void EndExecute()
        {
            installCtrl.EndInstall(StapName);
            base.EndExecute();
        }
        public override void UnDoCommand()
        {
            installCtrl.UnInstall(StapName);
            base.UnDoCommand();
        }
    }
}