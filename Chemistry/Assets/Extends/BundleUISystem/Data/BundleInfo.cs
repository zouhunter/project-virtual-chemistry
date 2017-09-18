using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace BundleUISystem
{
    [System.Serializable]
    public class BundleInfo: ItemInfoBase
    {
        public string bundleName;
        public override string IDName { get { return bundleName + assetName; } }
    }
}
