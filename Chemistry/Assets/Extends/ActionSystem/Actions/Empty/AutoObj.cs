using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
namespace WorldActionSystem
{
    [AddComponentMenu(MenuName.AutoObj)]
    public sealed class AutoObj : ActionObj
    {
        public Coroutine autocoroutine;
        public override ControllerType CtrlType
        {
            get
            {
                return 0;
            }
        }

        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            autocoroutine = StartCoroutine(AutoExecute());
        }

        IEnumerator AutoExecute()
        {
            yield return null;
            OnEndExecute(false);
        }

        public override void OnEndExecute(bool force)
        {
            base.OnEndExecute(force);
            if(autocoroutine != null)
            {
                StopCoroutine(autocoroutine);
            }
        }
        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();
            if (autocoroutine != null){
                StopCoroutine(autocoroutine);
            }
        }
    }
}
