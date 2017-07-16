using UnityEngine;
namespace FlowSystem
{
    public interface IHighLightItems
    {
        void HighLightTarget(Renderer render, int id = 0);
        void UnHighLightTarget(Renderer renderer);
    }
}