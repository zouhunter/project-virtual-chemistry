using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public abstract class PlaceObj : ActionObj
    {
        public bool autoInstall;//自动安装
        public bool ignorePass;//反忽略
        public Transform passBy;//路过
        public bool straightMove;//直线移动
        public bool ignoreMiddle;//忽略中间点
        public bool hideOnInstall;//安装完后隐藏
        public virtual GameObject Go { get { return gameObject; } }
        public virtual bool AlreadyPlaced { get { return obj != null; } }
        public virtual PickUpAbleElement obj { get; protected set; }
        private static List<ActionObj> lockQueue = new List<ActionObj>();
        protected virtual void Awake()
        {
            InitLayer();
        }
        protected virtual void OnDestroy()
        {
            if(lockQueue.Contains(this))
            {
                lockQueue.Remove(this);
            }
        }

        private void InitLayer()
        {
            GetComponentInChildren<Collider>().gameObject.layer = LayerMask.NameToLayer(Layers.placePosLayer);
        }
    
        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            ActiveElements(this);
            if (auto || autoInstall){
                OnAutoInstall();
            }
        }
        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();
            CompleteElements(this, true);
        }
        protected override void OnBeforeEnd(bool force)
        {
            base.OnBeforeEnd(force);
            CompleteElements(this, false);
        }
        protected abstract void OnAutoInstall();

        public virtual void Attach(PickUpAbleElement obj)
        {
            if (this.obj != null)
            {
                Debug.LogError(this + "allready attached");
            }

            this.obj = obj;
            obj.onInstallOkEvent += OnInstallComplete;
            obj.onUnInstallOkEvent += OnUnInstallComplete;
        }

        protected virtual void OnInstallComplete() { }

        protected virtual void OnUnInstallComplete() { }

        public virtual PickUpAbleElement Detach()
        {
            PickUpAbleElement old = obj;
            old.onInstallOkEvent -= OnInstallComplete;
            old.onUnInstallOkEvent -= OnUnInstallComplete;
            obj = default(PickUpAbleElement);
            return old;
        }

        private void ActiveElements(ActionObj element) 
        {
            var actived = lockQueue.Find(x => x.Name == element.Name);
            if (actived == null)
            {
                var objs = ElementController.Instence.GetElements<PickUpAbleElement>(element.Name);
                if (objs == null) return;
                for (int i = 0; i < objs.Count; i++)
                {
                    if (log) Debug.Log("ActiveElements:" + element.Name + (!objs[i].Started && !objs[i].HaveBinding));

                    if (!objs[i].Started && !objs[i].HaveBinding)
                    {
                        objs[i].StepActive();
                    }
                }
            }
            lockQueue.Add(element);
        }

        private void CompleteElements(ActionObj element, bool undo)
        {
            lockQueue.Remove(element);
            var active = lockQueue.Find(x => x.Name == element.Name);
            if (active == null)
            {
                var objs = ElementController.Instence.GetElements<PickUpAbleElement>(element.Name);
                if (objs == null) return;
                for (int i = 0; i < objs.Count; i++)
                {
                    if (log) Debug.Log("CompleteElements:" + element.Name + objs[i].Started);

                    if (objs[i].Started)
                    {
                        if (undo)
                        {
                            objs[i].StepUnDo();
                        }
                        else
                        {
                            objs[i].StepComplete();
                        }
                    }
                }
            }


        }

    }
}