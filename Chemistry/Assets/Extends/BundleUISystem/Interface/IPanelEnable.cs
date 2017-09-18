using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
namespace BundleUISystem.Internal
{
    public interface IPanelEnable
    {
        event UnityAction OnDelete;
    }
}