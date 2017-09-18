using UnityEngine;

using System.Collections.Generic;
namespace Connector
{
    public interface IPortParent
    {
        string Name { get; }
        Transform Trans { get; }
        List<IPortItem> ChildNodes { get; }
        void ResetBodyTransform(IPortParent otherParent, Vector3 rPos, Vector3 rdDir);
    }
}