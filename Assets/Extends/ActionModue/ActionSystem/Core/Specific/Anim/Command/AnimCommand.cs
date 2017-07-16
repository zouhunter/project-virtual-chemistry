using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
namespace WorldActionSystem
{

    public class AnimCommand : ActionCommand
    {
        public AnimObj anim;
        public bool undoActive;
        public bool endactive;

        public AnimCommand(string stapName, AnimObj anim, bool endactive = true) : base(stapName)
        {
            this.anim = anim;
            this.endactive = endactive;
            this.undoActive = anim.gameObject.activeSelf;
        }
        public override void StartExecute()
        {
            anim.PlayAnim();
            base.StartExecute();
        }
        public override void EndExecute()
        {
            anim.EndPlay();
            base.EndExecute();
        }
        public override void UnDoCommand()
        {
            anim.UnDoPlay();
            base.UnDoCommand();
        }
    }


}