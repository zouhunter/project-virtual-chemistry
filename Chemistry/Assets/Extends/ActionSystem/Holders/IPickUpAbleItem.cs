using UnityEngine;

namespace WorldActionSystem
{
    public abstract class PickUpAbleItem : MonoBehaviour
    {
        public virtual string Name { get { return name; } }
        private bool _pickUpAble = false;
        public virtual bool PickUpAble { get { return _pickUpAble; }set { _pickUpAble = value; } }
        public virtual void OnPickUp() { }
        public virtual void OnPickStay() { }
        public virtual void OnPickDown() { }
        public abstract void SetPosition(Vector3 pos);
        private Collider _collider;
        public Collider Collider { get { return _collider; } }
        
        protected virtual void Awake()
        {
            _collider = GetComponentInChildren<Collider>();
            _collider.gameObject.layer = LayerMask.NameToLayer( Layers.pickUpElementLayer);
        }
    }
}