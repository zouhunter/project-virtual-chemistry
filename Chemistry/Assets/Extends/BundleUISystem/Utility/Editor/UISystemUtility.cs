using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace BundleUISystem
{
    public static class UISystemUtility
    {
        private const string _defineLoadType = "UsePrefab";
        private static bool usePrefab = false;
        public static bool UsePrefab { get { return usePrefab; } }
        public static void DefineLoadType()
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            if (defines.Contains(_defineLoadType))
            {
                var newDefines = defines.Replace(_defineLoadType, "").Replace(";;", ";");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
            }
            usePrefab = true;
        }
        public static void UnDefineLoadType()
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            if (!defines.Contains(_defineLoadType))
            {
                var newDefines = defines.TrimEnd(';');
                newDefines += _defineLoadType;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
            }
            usePrefab = false;
        }
    }
}