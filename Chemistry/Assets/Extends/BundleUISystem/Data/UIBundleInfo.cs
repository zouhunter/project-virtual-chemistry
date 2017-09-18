using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace BundleUISystem
{
    [System.Serializable]
    public class UIBundleInfo : BundleInfo
    {
#if UNITY_EDITOR
        public string guid;
        public bool good;
#endif
        public UIBundleInfo()
        {
            type = Type.Name;
        }
    }
}