using UnityEngine.Events;
using UnityEngine;
namespace Connector
{
    public interface IPickUpController
    {
        event UnityAction<GameObject> onPickUp;
        event UnityAction<GameObject> onPickDown;
        event UnityAction<GameObject> onPickStatu;
        IPickUpAble PickedUpObj { get; }
        void Update();
    }
}