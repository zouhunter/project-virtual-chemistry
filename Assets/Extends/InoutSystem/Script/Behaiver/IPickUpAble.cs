using UnityEngine;
namespace FlowSystem
{
    public interface IPickUpAble
    {
        Transform Trans { get; }
        void OnPickUp();
        void OnPickStay();
        void OnPickDown();
    }
}