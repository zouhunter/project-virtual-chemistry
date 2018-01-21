using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{
    [AddComponentMenu(MenuName.DragObj)]
    public class DragObj : ActionObj
    {
        [SerializeField, Header("target (child transform)")]
        private Transform targetHolder;
        [SerializeField]
        private float clampTime = 0.2f;
        [SerializeField]
        private bool clampHard;
        private float autoDragTime { get { return Config.autoExecuteTime; } }
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Drag;
            }
        }
        private Coroutine waitCoroutine;
        public Vector3 targetPos { get; private set; }
        public Vector3 startPos { get; private set; }

        protected override void Start()
        {
            base.Start();
            InitPositions();
            InitLayer();
        }
        private void InitLayer()
        {
            GetComponentInChildren<Collider>().gameObject.layer =LayerMask.NameToLayer( Layers.dragItemLayer);
        }
        private void InitPositions()
        {
            startPos = transform.localPosition;
            targetPos = startPos + targetHolder.localPosition;
        }
        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            if (auto)
            {
                if (waitCoroutine == null)
                {
                    StartCoroutine(AutoDrag());
                }
            }
        }
        IEnumerator AutoDrag()
        {
            for (float i = 0; i < autoDragTime; i += Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(startPos, targetPos, i / autoDragTime);
                yield return null;
            }
            OnEndExecute(false);
        }
        protected override void OnBeforeEnd(bool force)
        {
            base.OnBeforeEnd(force);
            if (auto && waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }
            transform.localPosition = targetPos;
        }
        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();
            if (auto && waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }
            transform.localPosition = startPos;
        }

        internal void Clamp()
        {
            if(Vector3.Dot(transform.localPosition - startPos, targetPos - startPos) < 0)
            {
                if (gameObject.activeInHierarchy)
                    StartCoroutine(ClampInternal(startPos));
            }
            else if (Vector3.Distance(transform.localPosition, startPos) > Vector3.Distance(targetPos, startPos))
            {
                if (gameObject.activeInHierarchy)
                    StartCoroutine(ClampInternal(targetPos));
            }
            else
            {
                TryTrigger();
            }
        }

        IEnumerator ClampInternal(Vector3 pos)
        {
            var s_pos = transform.localPosition;
            for (float i = 0; i < clampTime; i += Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(s_pos, pos, i/clampTime);
                yield return null;
            }
            TryTrigger();
        }

        private void TryTrigger()
        {
            if(Vector3.Distance(transform.localPosition, targetPos) < 0.2f)
            {
                OnEndExecute(false);
            }
        }

        internal void TryMove(Vector3 vector3)
        {
            if(clampHard)
            {
                var newpos = transform.localPosition + vector3;
                if(Vector3.Distance(newpos, startPos) > Vector3.Distance(targetPos,startPos))
                {
                    return;
                }
                if (Vector3.Dot(newpos - startPos,targetPos -startPos) < 0)
                {
                    return;
                }
            }
            transform.localPosition += vector3;
        }
    }

}