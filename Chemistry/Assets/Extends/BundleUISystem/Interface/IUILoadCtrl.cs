using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace BundleUISystem.Internal
{
    public interface IUILoadCtrl
    {
        void GetGameObjectInfo(ItemInfoBase trigger);
        void CansaleLoadObject(string assetName);
    }
}