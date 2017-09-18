using UnityEngine;
namespace ReactSystem
{
    public interface IPickUpAble
    {
        Transform Trans { get; }
        void OnPickUp();
        void OnPickStay();
        void OnPickDown();
    }
}