using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace BundleUISystem
{
    [System.Serializable]
    public class PrefabInfo: ItemInfoBase
    {
        public GameObject prefab;
        public override string IDName { get { return assetName + "(clone)"; } }
    }
}
