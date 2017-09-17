using UnityEngine.Events;
namespace FlowSystem
{
    public interface IPickUpController
    {
        bool PickUped { get; }
        bool TryPickUpObject(out IPickUpAble pickedUpObj);
        bool TryStayPickUpedObject();
        bool PickDownPickedUpObject();
        void UpdatePickUpdObject();
    }
}