using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{

    /// <summary>
    /// 可操作对象具体行为实现
    /// </summary>
    public class PickUpAbleElement : PickUpAbleItem, IPlaceItem, ISupportElement
    {
        public class Tweener
        {
            private MonoBehaviour holder;
            private Vector3[] positons;
            private float animTime;
            private UnityAction onComplete;
            private Transform target;
            private Coroutine coroutine;
            private UnityAction<int> onwayPointChanged { get; set; }
            public Tweener(MonoBehaviour holder)
            {
                this.holder = holder;
            }

            internal void DOPath(Transform transform, Vector3[] vector3, int animTime, UnityAction onComplete)
            {
                this.positons = vector3;
                this.animTime = animTime;
                this.onComplete = onComplete;
                this.target = transform;
                coroutine = holder.StartCoroutine(MoveCore());
            }

            IEnumerator MoveCore()
            {
                float d_time = animTime / (positons.Length - 1);
                for (int i = 0; i < positons.Length - 1; i++)
                {
                    if (onwayPointChanged != null) onwayPointChanged(i);
                    var startPos = positons[i];
                    var targetPos = positons[i + 1];
                    for (float j = 0; j < d_time; j += Time.deltaTime)
                    {
                        target.position = Vector3.Lerp(startPos, targetPos, j);
                        yield return null;
                    }
                }
                if (onwayPointChanged != null) onwayPointChanged(positons.Length - 1);
                if (onComplete != null) onComplete();
            }
            internal void Kill()
            {
                if (coroutine != null)
                {
                    holder.StopCoroutine(coroutine);
                }
            }
            internal void OnWaypointChange(UnityAction<int> p)
            {
                onwayPointChanged = p;
            }
        }

        public string _name;
        private ActionGroup _system;
        public ActionGroup system { get { transform.SurchSystem(ref _system); return _system; } }
        protected ElementController elementCtrl { get { return ElementController.Instence; } }
        public int animTime { get { return Config.autoExecuteTime; } }
        public bool startActive = true;//如果是false，则到当前步骤时才会激活对象
        public bool HaveBinding { get { return target != null; } }

        public override string Name { get { if (string.IsNullOrEmpty(_name)) _name = name; return _name; } }

        public bool Started { get { return actived; } }

        public Renderer Render { get { return m_render; } }

        public event UnityAction onInstallOkEvent;
        public event UnityAction onUnInstallOkEvent;
#if ActionSystem_G
        [HideInInspector]
#endif
        public UnityEvent onPickUp, onPickDown, onStepActive, onStepComplete, onStepUnDo;

        [SerializeField]
        private Renderer m_render;
        [SerializeField]
        protected Color highLightColor = Color.green;
        protected Vector3 startRotation;
        protected Vector3 startPos;
        protected Tweener move;
        protected int smooth = 50;
        protected IHighLightItems highLighter;
        protected bool actived;
        protected PlaceObj target;
        public PlaceObj BindingObj { get { return target; } }
        protected bool hideOnInstall { get { return target ? target.hideOnInstall : false; } }//
        protected bool StraightMove { get { return target ? target.straightMove : false; } }
        protected bool IgnoreMiddle { get { return target ? target.ignoreMiddle : false; } }
        protected Transform Passby { get { return target ? target.passBy : null; } }
        public override bool PickUpAble { get; set; }
        protected bool tweening;
        protected UnityAction tweenCompleteAction;
        protected Vector3 lastPos;
        protected override void Awake()
        {
            base.Awake();
            elementCtrl.RegistElement(this);
            move = new Tweener(this);
        }
        protected virtual void Start()
        {
            InitRender();
            InitLayer();
            startPos = transform.position;
            startRotation = transform.eulerAngles;
            gameObject.SetActive(startActive);
        }
        private void InitLayer()
        {
            GetComponentInChildren<Collider>().gameObject.layer = LayerMask.NameToLayer(Layers.pickUpElementLayer);
        }

        protected virtual void OnDestroy()
        {
            move.Kill();
            elementCtrl.RemoveElement(this);
        }
        protected virtual void InitRender()
        {
            if (m_render == null) m_render = gameObject.GetComponentInChildren<Renderer>();
            highLighter = new ShaderHighLight();
        }

        protected virtual void CreatePosList(Vector3 end, Vector3 endRot, out List<Vector3> posList, out List<Vector3> rotList)
        {
            posList = new List<Vector3>();
            rotList = new List<Vector3>();
            Vector3 midPos = Vector3.zero;

            if (Passby != null)
            {
                midPos = Passby.position;
            }
            else
            {
                var player = FindObjectOfType<Camera>().transform;
                midPos = player.transform.position + player.transform.forward * Config.elementFoward;
            }

            var midRot = (endRot + transform.eulerAngles * 3) * 0.25f;
            if (StraightMove || IgnoreMiddle)
            {
                posList.Add(transform.position);
                rotList.Add(transform.eulerAngles);

                if (!IgnoreMiddle)
                {
                    posList.Add(midPos);
                    rotList.Add(midRot);
                }

                posList.Add(end);
                rotList.Add(endRot);
            }
            else
            {
                for (int i = 0; i < smooth; i++)
                {
                    float curr = (i + 0f) / (smooth - 1);
                    posList.Add(Bezier.CalculateBezierPoint(curr, transform.position, midPos, end));
                    rotList.Add(Bezier.CalculateBezierPoint(curr, transform.eulerAngles, midRot, endRot));
                }
            }

        }

        protected virtual void DoPath(Vector3 end, Vector3 endRot)
        {
            List<Vector3> poss;
            List<Vector3> rots;
            CreatePosList(end, endRot, out poss, out rots);
            tweening = true;
            move.DOPath(transform, poss.ToArray(), animTime, OnTweenComplete);
            move.OnWaypointChange((x) =>
            {
                transform.eulerAngles = rots[x];
            });
        }
        protected virtual void OnTweenComplete()
        {
            tweening = false;
            if (tweenCompleteAction != null)
            {
                tweenCompleteAction.Invoke();
            }
        }
        /// <summary>
        /// 动画安装
        /// </summary>
        /// <param name="target"></param>
        public virtual void NormalInstall(PlaceObj target, bool complete = true, bool binding = true)
        {
            StopTween();
            if (!HaveBinding)
            {
                Binding(target);

                tweenCompleteAction = () =>
                {
                    OnInstallComplete(complete);
                };

                DoPath(target.transform.position, target.transform.eulerAngles);

                if (!binding)
                {
                    UnBinding();
                }
            }
        }

        /// <summary>
        /// 定位安装
        /// </summary>
        /// <param name="target"></param>
        public virtual void QuickInstall(PlaceObj target, bool complete = true, bool binding = true)
        {
            StopTween();
            if (!HaveBinding)
            {
                Binding(target);
                transform.position = target.transform.position;
                transform.rotation = target.transform.rotation;

                if (!binding)
                    UnBinding();

                OnInstallComplete(complete);
            }
            else
            {
                Debug.LogError(this + "HaveBinding:" + BindingObj);
            }
        }

        /// <summary>
        /// 卸载
        /// </summary>
        public virtual void NormalUnInstall()
        {
            Debug.Log("NormalUnInstall");
            StopTween();
#if !NoFunction
            tweenCompleteAction = () =>
            {
                if (HaveBinding)
                {
                    UnBinding();
                }
                OnUnInstallComplete();
            };
            DoPath(startPos, startRotation);
#endif
        }

        /// <summary>
        /// 快速卸载
        /// </summary>
        public virtual void QuickUnInstall()
        {
            Debug.Log("QuickUnInstall");

#if !NoFunction
            StopTween();
            transform.eulerAngles = startRotation;
            transform.position = startPos;
            target = null;

            if (HaveBinding)
            {
                UnBinding();
            }

            OnUnInstallComplete();
#endif
        }

        /// <summary>
        /// 拿起事件
        /// </summary>
        public override void OnPickUp()
        {
            base.OnPickUp();
            StopTween();

            if (onPickUp != null)
            {
                onPickUp.Invoke();
            }
        }

        public override void OnPickDown()
        {
            base.OnPickDown();
            StopTween();
            if (onPickDown != null)
            {
                onPickDown.Invoke();
            }
        }
        public override void SetPosition(Vector3 pos)
        {
            if (lastPos != pos)
            {
                lastPos = pos;
                transform.position = pos;
            }
        }
        /// <summary>
        /// 步骤激活（随机选中的一些installObj）
        /// </summary>
        public virtual void StepActive()
        {
            actived = true;
            PickUpAble = true;
            onStepActive.Invoke();
            gameObject.SetActive(true);
        }
        /// <summary>
        /// 步骤结束（安装上之后整个步骤结束）
        /// </summary>
        public virtual void StepComplete()
        {
            actived = false;
            onStepComplete.Invoke();
            if (tweening)
            {
                StopTween();
                OnTweenComplete();
            }
        }

        /// <summary>
        /// 步骤重置(没有用到的元素)
        /// </summary>
        public virtual void StepUnDo()
        {
            actived = false;
            onStepUnDo.Invoke();
            gameObject.SetActive(startActive);
        }

        protected virtual void Update()
        {
            if (!Config.highLightNotice) return;
            if (m_render == null) return;
            if (actived)
            {
                highLighter.HighLightTarget(m_render, highLightColor);
            }
            else
            {
                highLighter.UnHighLightTarget(m_render);
            }
        }

        protected virtual void StopTween()
        {
            move.Kill();
        }

        protected virtual void OnInstallComplete(bool complete)
        {
            if (hideOnInstall)
            {
                gameObject.SetActive(false);
            }

            if (onInstallOkEvent != null)
                onInstallOkEvent();

            if (complete) StepComplete();
        }
        protected virtual void OnUnInstallComplete()
        {
            if (onUnInstallOkEvent != null)
                onUnInstallOkEvent();
        }
        protected virtual void Binding(PlaceObj target)
        {
            this.target = target;
        }
        protected virtual PlaceObj UnBinding()
        {
            var old = target;
            target = null;
            return old;
        }

    }

}